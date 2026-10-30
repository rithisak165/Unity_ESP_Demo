using System;
using System.Runtime.InteropServices;
using UnityEngine;

[DefaultExecutionOrder(1050)]
public class NativeTelemetryClient : MonoBehaviour
{
    public DebugVisualizerConfig config;
    private Camera targetCamera;

    #region Native P/Invoke Bindings

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void NativeTelemetry_Initialize();

    [DllImport("__Internal")]
    private static extern void NativeTelemetry_Shutdown();

    [DllImport("__Internal")]
    private static extern void NativeTelemetry_UpdateSettings(ref NativeVisualizerSettings settings);

    [DllImport("__Internal")]
    private static extern void NativeTelemetry_UpdateCamera(ref NativeCameraState cameraState);

    [DllImport("__Internal")]
    private static extern void NativeTelemetry_UpdateEntities(IntPtr entitiesArray, int count);

    [DllImport("__Internal")]
    private static extern bool NativeTelemetry_IsActive();
#else
    // Fallback stubs for Unity Editor on Windows (Simulated execution)
    private static void NativeTelemetry_Initialize() { }
    private static void NativeTelemetry_Shutdown() { }
    private static void NativeTelemetry_UpdateSettings(ref NativeVisualizerSettings settings) { }
    private static void NativeTelemetry_UpdateCamera(ref NativeCameraState cameraState) { }
    private static void NativeTelemetry_UpdateEntities(IntPtr entitiesArray, int count) { }
    private static bool NativeTelemetry_IsActive() { return false; }
#endif

    #endregion

    #region Blittable Native Data Structs

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct NativeVector3
    {
        public float x, y, z;
        public NativeVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct NativeColor
    {
        public float r, g, b, a;
        public NativeColor(Color c) { r = c.r; g = c.g; b = c.b; a = c.a; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct NativeBoneNode
    {
        public NativeVector3 worldPosition;
        public int boneId;
        public int parentBoneId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct NativeEntityTelemetry
    {
        public int entityId;
        [MarshalAs(UnmanagedType.I1)]
        public bool isAlive;
        public NativeVector3 worldPosition;
        public NativeVector3 headPosition;
        public NativeVector3 feetPosition;
        public float distanceToCamera;
        public NativeColor entityColor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string name;
        public int boneCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public NativeBoneNode[] bones;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct NativeCameraState
    {
        public NativeVector3 cameraPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] viewMatrix;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] projectionMatrix;
        public float screenWidth;
        public float screenHeight;
        public float safeAreaTop;
        public float safeAreaBottom;
        public float safeAreaLeft;
        public float safeAreaRight;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct NativeVisualizerSettings
    {
        [MarshalAs(UnmanagedType.I1)] public bool isMasterEnabled;
        [MarshalAs(UnmanagedType.I1)] public bool showBoundingBoxes;
        [MarshalAs(UnmanagedType.I1)] public bool showBoxOutline;
        [MarshalAs(UnmanagedType.I1)] public bool showSkeleton;
        [MarshalAs(UnmanagedType.I1)] public bool showHeadMarker;
        [MarshalAs(UnmanagedType.I1)] public bool showChestMarker;
        [MarshalAs(UnmanagedType.I1)] public bool showNameLabels;
        [MarshalAs(UnmanagedType.I1)] public bool showDistanceLabels;
        [MarshalAs(UnmanagedType.I1)] public bool showSnaplines;
        public int snaplineOrigin;
        public float lineThickness;
        public float markerRadius;
        public int baseFontSize;
        public NativeColor defaultBoxColor;
        public NativeColor skeletonColor;
        public NativeColor headMarkerColor;
        public NativeColor chestMarkerColor;
        public NativeColor boxOutlineColor;
        public NativeColor textColor;
    }

    #endregion

    private NativeEntityTelemetry[] entityBuffer;
    private GCHandle pinnedBuffer;
    private const int MAX_BUFFERED_ENTITIES = 64;

    private void Awake()
    {
        targetCamera = Camera.main;
        entityBuffer = new NativeEntityTelemetry[MAX_BUFFERED_ENTITIES];
        for (int i = 0; i < MAX_BUFFERED_ENTITIES; i++)
        {
            entityBuffer[i].bones = new NativeBoneNode[15];
        }
        pinnedBuffer = GCHandle.Alloc(entityBuffer, GCHandleType.Pinned);

        NativeTelemetry_Initialize();
    }

    private void OnDestroy()
    {
        NativeTelemetry_Shutdown();
        if (pinnedBuffer.IsAllocated)
        {
            pinnedBuffer.Free();
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || config == null) return;

        // 1. Sync Settings
        NativeVisualizerSettings s = new NativeVisualizerSettings
        {
            isMasterEnabled = config.isMasterEnabled,
            showBoundingBoxes = config.showBoundingBoxes,
            showBoxOutline = config.showBoxOutline,
            showSkeleton = config.showSkeleton,
            showHeadMarker = config.showHeadMarker,
            showChestMarker = config.showChestMarker,
            showNameLabels = config.showNameLabels,
            showDistanceLabels = config.showDistanceLabels,
            showSnaplines = config.showSnaplines,
            snaplineOrigin = (int)config.snaplineOrigin,
            lineThickness = config.lineThickness,
            markerRadius = config.markerRadius,
            baseFontSize = config.baseFontSize,
            defaultBoxColor = new NativeColor(config.defaultEnemyColor),
            skeletonColor = new NativeColor(config.skeletonColor),
            headMarkerColor = new NativeColor(config.headMarkerColor),
            chestMarkerColor = new NativeColor(config.chestMarkerColor),
            boxOutlineColor = new NativeColor(config.boxOutlineColor),
            textColor = new NativeColor(config.textColor)
        };
        NativeTelemetry_UpdateSettings(ref s);

        // 2. Sync Camera State & Matrices
        Matrix4x4 vMat = targetCamera.worldToCameraMatrix;
        Matrix4x4 pMat = GL.GetGPUProjectionMatrix(targetCamera.projectionMatrix, false);
        Rect safe = Screen.safeArea;

        NativeCameraState camState = new NativeCameraState
        {
            cameraPosition = new NativeVector3(targetCamera.transform.position),
            viewMatrix = MatrixToFloatArray(vMat),
            projectionMatrix = MatrixToFloatArray(pMat),
            screenWidth = Screen.width,
            screenHeight = Screen.height,
            safeAreaTop = Screen.height - safe.yMax,
            safeAreaBottom = safe.yMin,
            safeAreaLeft = safe.xMin,
            safeAreaRight = Screen.width - safe.xMax
        };
        NativeTelemetry_UpdateCamera(ref camState);

        // 3. Collect Active Entities Telemetry
        int count = Mathf.Min(TargetEntity.ActiveEntities.Count, MAX_BUFFERED_ENTITIES);
        Vector3 camPos = targetCamera.transform.position;

        for (int i = 0; i < count; i++)
        {
            TargetEntity t = TargetEntity.ActiveEntities[i];
            if (t == null) continue;

            entityBuffer[i].entityId = t.GetInstanceID();
            entityBuffer[i].isAlive = t.isAlive;
            entityBuffer[i].worldPosition = new NativeVector3(t.transform.position);
            entityBuffer[i].headPosition = new NativeVector3(t.GetTopPosition());
            entityBuffer[i].feetPosition = new NativeVector3(t.GetBottomPosition());
            entityBuffer[i].distanceToCamera = Vector3.Distance(camPos, t.transform.position);
            entityBuffer[i].entityColor = new NativeColor(t.entityColor);
            entityBuffer[i].name = t.entityName ?? "Player";

            // Fill Bones
            PopulateBones(t, ref entityBuffer[i]);
        }

        NativeTelemetry_UpdateEntities(pinnedBuffer.AddrOfPinnedObject(), count);
    }

    private void PopulateBones(TargetEntity t, ref NativeEntityTelemetry data)
    {
        data.boneCount = 15;
        SetBone(ref data.bones[0], t.head, 0, 1);
        SetBone(ref data.bones[1], t.neck, 1, 2);
        SetBone(ref data.bones[2], t.chest, 2, 3);
        SetBone(ref data.bones[3], t.spine, 3, 4);
        SetBone(ref data.bones[4], t.hips, 4, -1);

        SetBone(ref data.bones[5], t.leftShoulder, 5, 2);
        SetBone(ref data.bones[6], t.leftUpperArm, 6, 5);
        SetBone(ref data.bones[7], t.leftLowerArm, 7, 6);
        SetBone(ref data.bones[8], t.leftHand, 8, 7);

        SetBone(ref data.bones[9], t.rightShoulder, 9, 2);
        SetBone(ref data.bones[10], t.rightUpperArm, 10, 9);
        SetBone(ref data.bones[11], t.rightLowerArm, 11, 10);
        SetBone(ref data.bones[12], t.rightHand, 12, 11);

        SetBone(ref data.bones[13], t.leftFoot, 13, 4);
        SetBone(ref data.bones[14], t.rightFoot, 14, 4);
    }

    private void SetBone(ref NativeBoneNode node, Transform trans, int id, int parentId)
    {
        node.boneId = id;
        node.parentBoneId = parentId;
        node.worldPosition = trans != null ? new NativeVector3(trans.position) : new NativeVector3(Vector3.zero);
    }

    private static float[] MatrixToFloatArray(Matrix4x4 m)
    {
        float[] arr = new float[16];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                arr[r * 4 + c] = m[r, c];
        return arr;
    }
}

