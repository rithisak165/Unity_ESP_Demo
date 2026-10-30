#pragma once

#include <stdint.h>
#include <stdbool.h>

#ifdef __cplusplus
extern "C" {
#endif

#pragma pack(push, 4)

typedef struct {
    float x;
    float y;
} NativeVector2;

typedef struct {
    float x;
    float y;
    float z;
} NativeVector3;

typedef struct {
    float r;
    float g;
    float b;
    float a;
} NativeColor;

typedef struct {
    NativeVector3 worldPosition;
    int32_t       boneId;
    int32_t       parentBoneId;
} NativeBoneNode;

typedef struct {
    int32_t        entityId;
    bool           isAlive;
    NativeVector3  worldPosition;
    NativeVector3  headPosition;
    NativeVector3  feetPosition;
    float          distanceToCamera;
    NativeColor    entityColor;
    char           name[32];
    
    // Complete 15-joint skeleton payload
    int32_t        boneCount;
    NativeBoneNode bones[15];
} NativeEntityTelemetry;

typedef struct {
    NativeVector3 cameraPosition;
    float         viewMatrix[16];
    float         projectionMatrix[16];
    float         screenWidth;
    float         screenHeight;
    float         safeAreaTop;
    float         safeAreaBottom;
    float         safeAreaLeft;
    float         safeAreaRight;
} NativeCameraState;

typedef struct {
    bool        isMasterEnabled;
    bool        showBoundingBoxes;
    bool        showBoxOutline;
    bool        showSkeleton;
    bool        showHeadMarker;
    bool        showChestMarker;
    bool        showNameLabels;
    bool        showDistanceLabels;
    bool        showSnaplines;
    int32_t     snaplineOrigin; // 0 = Top, 1 = Center, 2 = Bottom
    float       lineThickness;
    float       markerRadius;
    int32_t     baseFontSize;
    NativeColor defaultBoxColor;
    NativeColor skeletonColor;
    NativeColor headMarkerColor;
    NativeColor chestMarkerColor;
    NativeColor boxOutlineColor;
    NativeColor textColor;
} NativeVisualizerSettings;

#pragma pack(pop)

// Exported C-ABI Functions called by Unity C#
void NativeTelemetry_Initialize(void);
void NativeTelemetry_Shutdown(void);
void NativeTelemetry_UpdateSettings(const NativeVisualizerSettings* settings);
void NativeTelemetry_UpdateCamera(const NativeCameraState* cameraState);
void NativeTelemetry_UpdateEntities(const NativeEntityTelemetry* entities, int32_t count);
bool NativeTelemetry_IsActive(void);

#ifdef __cplusplus
}
#endif

