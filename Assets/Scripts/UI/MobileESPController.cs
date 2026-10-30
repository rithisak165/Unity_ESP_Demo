using UnityEngine;

public class MobileESPController : MonoBehaviour
{
    public DebugVisualizerConfig config;
    private bool isMenuOpen = false;
    private Rect menuRect;

    private void Start()
    {
        UpdateMenuDimensions();
    }

    private void UpdateMenuDimensions()
    {
        Rect safeArea = Screen.safeArea;
        float menuWidth = Mathf.Min(240.0f, safeArea.width * 0.45f);
        menuRect = new Rect(safeArea.x + 15, safeArea.y + 15, menuWidth, 310.0f);
    }

    private void OnGUI()
    {
        if (config == null) return;

        UpdateMenuDimensions();

        // 1. Touch HUD Toggle Pill Button
        Rect toggleBtnRect = new Rect(menuRect.x, menuRect.y, 120, 38);
        if (GUI.Button(toggleBtnRect, isMenuOpen ? "Hide Menu" : "ESP HUD"))
        {
            isMenuOpen = !isMenuOpen;
        }

        if (!isMenuOpen) return;

        // 2. Main Controls Box
        Rect contentRect = new Rect(menuRect.x, menuRect.y + 44, menuRect.width, 260);
        GUI.Box(contentRect, "Visual Telemetry Menu");

        GUILayout.BeginArea(new Rect(contentRect.x + 10, contentRect.y + 25, contentRect.width - 20, contentRect.height - 30));

        config.isMasterEnabled = GUILayout.Toggle(config.isMasterEnabled, " Master Enable");
        config.showBoundingBoxes = GUILayout.Toggle(config.showBoundingBoxes, " Bounding Box");
        config.showSkeleton = GUILayout.Toggle(config.showSkeleton, " 15-Bone Skeleton");
        config.showHeadMarker = GUILayout.Toggle(config.showHeadMarker, " Head Marker");
        config.showChestMarker = GUILayout.Toggle(config.showChestMarker, " Chest Marker");
        config.showNameLabels = GUILayout.Toggle(config.showNameLabels, " Player Names");
        config.showDistanceLabels = GUILayout.Toggle(config.showDistanceLabels, " Distance [m]");
        config.showSnaplines = GUILayout.Toggle(config.showSnaplines, " Snaplines");

        GUILayout.EndArea();
    }
}

