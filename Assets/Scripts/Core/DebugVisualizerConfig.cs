using UnityEngine;

[CreateAssetMenu(fileName = "VisualizerConfig", menuName = "Telemetry/Visualizer Config")]
public class DebugVisualizerConfig : ScriptableObject
{
    [Header("Master Controls")]
    public bool isMasterEnabled = true;
    public KeyCode editorToggleKey = KeyCode.F1;
    public float maxRenderDistance = 300.0f;

    [Header("Feature Toggles")]
    public bool showBoundingBoxes = true;
    public bool showBoxOutline = true;
    public bool showSkeleton = true;
    public bool showHeadMarker = true;
    public bool showChestMarker = true;
    public bool showNameLabels = true;
    public bool showDistanceLabels = true;
    public bool showSnaplines = true;

    [Header("Snapline Origin")]
    public SnaplineOrigin snaplineOrigin = SnaplineOrigin.BottomScreen;

    [Header("Color Palette")]
    public Color defaultEnemyColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);
    public Color skeletonColor = new Color(0.2f, 0.8f, 1.0f, 1.0f);
    public Color headMarkerColor = new Color(1.0f, 0.9f, 0.0f, 1.0f);
    public Color chestMarkerColor = new Color(0.0f, 1.0f, 0.4f, 1.0f);
    public Color boxOutlineColor = new Color(0.0f, 0.0f, 0.0f, 0.9f);
    public Color textColor = Color.white;

    [Header("Dimensions & Mobile DPI Scaling")]
    public float lineThickness = 2.0f;
    public float markerRadius = 5.0f;
    public int baseFontSize = 13;

    public enum SnaplineOrigin
    {
        TopScreen = 0,
        CenterScreen = 1,
        BottomScreen = 2
    }
}

