#import <UIKit/UIKit.h>
#import <Metal/Metal.h>
#import <MetalKit/MetalKit.h>
#import <simd/simd.h>
#import <vector>
#import <mutex>
#import <string>
#import <cmath>
#import "NativeTelemetryBridge.h"

// ============================================================================
// NATIVE METAL OVERLAY VIEW FOR UNITY_ESP_DEMO
// ============================================================================

@interface NativeOverlayTouchView : MTKView
@end

@implementation NativeOverlayTouchView
- (BOOL)pointInside:(CGPoint)point withEvent:(UIEvent *)event {
    // Transparent overlay: Touches always pass through to Unity game controls
    return NO;
}
@end

// State Storage with Thread Safety
static std::mutex                      s_DataMutex;
static NativeCameraState              s_CameraState;
static NativeVisualizerSettings        s_Settings;
static std::vector<NativeEntityTelemetry> s_EntityBuffer;
static bool                            s_IsInitialized = false;

@interface NativeOverlayRenderer : NSObject <MTKViewDelegate>
@property (nonatomic, strong) id<MTLDevice> device;
@property (nonatomic, strong) id<MTLCommandQueue> commandQueue;
@property (nonatomic, strong) NativeOverlayTouchView *metalView;
@property (nonatomic, strong) id<MTLRenderPipelineState> pipelineState;
@end

static NativeOverlayRenderer *s_RendererInstance = nil;

@implementation NativeOverlayRenderer

- (instancetype)init {
    self = [super init];
    if (self) {
        _device = MTLCreateSystemDefaultDevice();
        if (_device) {
            _commandQueue = [_device newCommandQueue];
        }
    }
    return self;
}

- (void)setupOverlayOnWindow:(UIWindow *)window {
    if (!window || !_device) return;

    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.metalView != nil) return;

        self.metalView = [[NativeOverlayTouchView alloc] initWithFrame:window.bounds device:self.device];
        self.metalView.delegate = self;
        self.metalView.clearColor = MTLClearColorMake(0.0, 0.0, 0.0, 0.0);
        self.metalView.backgroundColor = [UIColor clearColor];
        self.metalView.userInteractionEnabled = NO;
        self.metalView.clipsToBounds = YES;
        self.metalView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;

        [window addSubview:self.metalView];
        [window bringSubviewToFront:self.metalView];
    });
}

// 3D to 2D World-To-Screen Projection Matrix Transformation
static bool ProjectWorldToScreen(const NativeVector3& world, const NativeCameraState& cam, CGPoint& outScreen) {
    simd_float4 wPos = simd_make_float4(world.x, world.y, world.z, 1.0f);

    // Apply 4x4 View Matrix
    simd_float4x4 viewMat = *(const simd_float4x4*)cam.viewMatrix;
    simd_float4 viewPos = simd_mul(viewMat, wPos);

    // Near-plane culling: objects behind camera lens
    if (viewPos.z >= -0.05f) return false;

    // Apply 4x4 Projection Matrix
    simd_float4x4 projMat = *(const simd_float4x4*)cam.projectionMatrix;
    simd_float4 clipPos = simd_mul(projMat, viewPos);

    if (clipPos.w <= 0.001f) return false;

    // Perspective Divide -> Normalized Device Coordinates [-1.0 .. 1.0]
    float ndcX = clipPos.x / clipPos.w;
    float ndcY = clipPos.y / clipPos.w;

    // Viewport Mapping to Screen Pixels
    float screenX = ((ndcX + 1.0f) * 0.5f) * cam.screenWidth;
    float screenY = ((1.0f - ndcY) * 0.5f) * cam.screenHeight; // Inverted Y for iOS Screen Space

    outScreen.x = screenX;
    outScreen.y = screenY;
    return true;
}

- (void)drawInMTKView:(MTKView *)view {
    if (!s_IsInitialized || !s_Settings.isMasterEnabled) return;

    // Snapshot state safely under lock
    NativeCameraState localCam;
    NativeVisualizerSettings localSettings;
    std::vector<NativeEntityTelemetry> entities;

    {
        std::lock_guard<std::mutex> lock(s_DataMutex);
        localCam = s_CameraState;
        localSettings = s_Settings;
        entities = s_EntityBuffer;
    }

    if (localCam.screenWidth <= 0.0f || localCam.screenHeight <= 0.0f) return;

    MTLRenderPassDescriptor *renderPassDesc = view.currentRenderPassDescriptor;
    if (!renderPassDesc) return;

    id<MTLCommandBuffer> commandBuffer = [self.commandQueue commandBuffer];
    if (!commandBuffer) return;

    id<MTLRenderCommandEncoder> renderEncoder = [commandBuffer renderCommandEncoderWithDescriptor:renderPassDesc];
    if (!renderEncoder) return;

    // End encoding and present buffer
    [renderEncoder endEncoding];
    [commandBuffer presentDrawable:view.currentDrawable];
    [commandBuffer commit];
}

- (void)mtkView:(MTKView *)view drawableSizeWillChange:(CGSize)size {
}

@end

// ============================================================================
// EXPORTED C-ABI FUNCTIONS
// ============================================================================

extern "C" {

void NativeTelemetry_Initialize(void) {
    if (s_IsInitialized) return;

    s_RendererInstance = [[NativeOverlayRenderer alloc] init];

    dispatch_async(dispatch_get_main_queue(), ^{
        UIWindow *keyWindow = nil;
        for (UIScene *scene in [UIApplication sharedApplication].connectedScenes) {
            if (scene.activationState == UISceneActivationStateForegroundActive && [scene isKindOfClass:[UIWindowScene class]]) {
                UIWindowScene *windowScene = (UIWindowScene *)scene;
                for (UIWindow *w in windowScene.windows) {
                    if (w.isKeyWindow) {
                        keyWindow = w;
                        break;
                    }
                }
            }
        }
        if (!keyWindow) {
            keyWindow = [UIApplication sharedApplication].windows.firstObject;
        }

        if (keyWindow) {
            [s_RendererInstance setupOverlayOnWindow:keyWindow];
        }
    });

    s_IsInitialized = true;
}

void NativeTelemetry_Shutdown(void) {
    std::lock_guard<std::mutex> lock(s_DataMutex);
    s_EntityBuffer.clear();
    s_IsInitialized = false;
}

void NativeTelemetry_UpdateSettings(const NativeVisualizerSettings* settings) {
    if (!settings) return;
    std::lock_guard<std::mutex> lock(s_DataMutex);
    s_Settings = *settings;
}

void NativeTelemetry_UpdateCamera(const NativeCameraState* cameraState) {
    if (!cameraState) return;
    std::lock_guard<std::mutex> lock(s_DataMutex);
    s_CameraState = *cameraState;
}

void NativeTelemetry_UpdateEntities(const NativeEntityTelemetry* entities, int32_t count) {
    std::lock_guard<std::mutex> lock(s_DataMutex);
    s_EntityBuffer.clear();
    if (entities && count > 0) {
        s_EntityBuffer.assign(entities, entities + count);
    }
}

bool NativeTelemetry_IsActive(void) {
    return s_IsInitialized;
}

}

