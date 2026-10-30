using UnityEngine;

[DefaultExecutionOrder(1000)]
public class DebugVisualizer : MonoBehaviour
{
    public DebugVisualizerConfig config;
    private Camera mainCam;
    private static Texture2D solidTexture;
    private GUIStyle labelStyle;
    private int screenWidth;
    private int screenHeight;

    private void Awake()
    {
        mainCam = Camera.main;
        EnsureTexture();
    }

    private void EnsureTexture()
    {
        if (solidTexture == null)
        {
            solidTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            solidTexture.SetPixel(0, 0, Color.white);
            solidTexture.Apply();
        }
    }

    private void Update()
    {
        if (config != null && Input.GetKeyDown(config.editorToggleKey))
        {
            config.isMasterEnabled = !config.isMasterEnabled;
        }
    }

    private void OnGUI()
    {
        if (config == null || !config.isMasterEnabled) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        EnsureTexture();
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        SetupLabelStyle();

        Vector3 camPos = mainCam.transform.position;
        Vector2 snapOrigin = GetSnaplineOrigin();

        for (int i = 0; i < TargetEntity.ActiveEntities.Count; i++)
        {
            TargetEntity entity = TargetEntity.ActiveEntities[i];
            if (entity == null || !entity.isAlive) continue;

            float distance = Vector3.Distance(camPos, entity.transform.position);
            if (distance > config.maxRenderDistance || distance < 0.2f) continue;

            // 1. Calculate World Points
            Vector3 worldTop = entity.GetTopPosition();
            Vector3 worldBot = entity.GetBottomPosition();

            // 2. Project to Screen Space
            Vector3 screenTop = mainCam.WorldToScreenPoint(worldTop);
            Vector3 screenBot = mainCam.WorldToScreenPoint(worldBot);

            // Frustum Culling: Skip targets behind the camera plane
            if (screenTop.z <= 0.1f || screenBot.z <= 0.1f) continue;

            // 3. Invert Y for Top-Left 2D GUI coordinates
            Vector2 guiTop = new Vector2(screenTop.x, screenHeight - screenTop.y);
            Vector2 guiBot = new Vector2(screenBot.x, screenHeight - screenBot.y);

            // 4. Calculate 2D Bounding Box Dimensions
            float boxHeight = Mathf.Abs(guiBot.y - guiTop.y);
            if (boxHeight < 4.0f) continue;
            float boxWidth = boxHeight * 0.52f;
            float boxCenterX = (guiTop.x + guiBot.x) * 0.5f;

            Rect boxRect = new Rect(boxCenterX - boxWidth * 0.5f, guiTop.y, boxWidth, boxHeight);
            Color entityCol = entity.entityColor;

            // 5. Draw Bounding Box
            if (config.showBoundingBoxes)
            {
                if (config.showBoxOutline)
                    DrawRectOutline(boxRect, config.boxOutlineColor, config.lineThickness + 2.0f);
                DrawRectOutline(boxRect, entityCol, config.lineThickness);
            }

            // 6. Draw Snapline
            if (config.showSnaplines)
            {
                DrawLine(snapOrigin, new Vector2(boxCenterX, guiBot.y), entityCol, config.lineThickness);
            }

            // 7. Draw Full 15-Joint Skeleton Hierarchy
            if (config.showSkeleton)
            {
                DrawFullSkeleton(entity, config.skeletonColor);
            }

            // 8. Draw Head & Chest Markers
            if (config.showHeadMarker && entity.head != null)
            {
                Vector3 screenHead = mainCam.WorldToScreenPoint(entity.head.position);
                if (screenHead.z > 0.1f)
                    DrawMarker(new Vector2(screenHead.x, screenHeight - screenHead.y), config.headMarkerColor, config.markerRadius);
            }

            if (config.showChestMarker)
            {
                Transform chestTransform = entity.chest ?? entity.spine;
                if (chestTransform != null)
                {
                    Vector3 screenChest = mainCam.WorldToScreenPoint(chestTransform.position);
                    if (screenChest.z > 0.1f)
                        DrawMarker(new Vector2(screenChest.x, screenHeight - screenChest.y), config.chestMarkerColor, config.markerRadius * 0.9f);
                }
            }

            // 9. Draw Labels (Name & Distance)
            string label = "";
            if (config.showNameLabels) label += entity.entityName;
            if (config.showDistanceLabels) label += $" [{Mathf.RoundToInt(distance)}m]";

            if (!string.IsNullOrEmpty(label))
            {
                Vector2 textSize = labelStyle.CalcSize(new GUIContent(label));
                Rect labelRect = new Rect(boxCenterX - textSize.x * 0.5f, boxRect.yMin - textSize.y - 3.0f, textSize.x, textSize.y);
                DrawOutlinedText(labelRect, label, config.textColor, config.boxOutlineColor);
            }
        }
    }

    private void DrawFullSkeleton(TargetEntity entity, Color color)
    {
        // Central Spine
        DrawBone(entity.head, entity.neck, color);
        DrawBone(entity.neck, entity.chest, color);
        DrawBone(entity.chest, entity.spine, color);
        DrawBone(entity.spine, entity.hips, color);

        // Left Arm Chain
        DrawBone(entity.chest, entity.leftShoulder, color);
        DrawBone(entity.leftShoulder ?? entity.chest, entity.leftUpperArm, color);
        DrawBone(entity.leftUpperArm, entity.leftLowerArm, color);
        DrawBone(entity.leftLowerArm, entity.leftHand, color);

        // Right Arm Chain
        DrawBone(entity.chest, entity.rightShoulder, color);
        DrawBone(entity.rightShoulder ?? entity.chest, entity.rightUpperArm, color);
        DrawBone(entity.rightUpperArm, entity.rightLowerArm, color);
        DrawBone(entity.rightLowerArm, entity.rightHand, color);

        // Left Leg Chain
        DrawBone(entity.hips, entity.leftUpperLeg, color);
        DrawBone(entity.leftUpperLeg, entity.leftLowerLeg, color);
        DrawBone(entity.leftLowerLeg, entity.leftFoot, color);

        // Right Leg Chain
        DrawBone(entity.hips, entity.rightUpperLeg, color);
        DrawBone(entity.rightUpperLeg, entity.rightLowerLeg, color);
        DrawBone(entity.rightLowerLeg, entity.rightFoot, color);
    }

    private void DrawBone(Transform a, Transform b, Color color)
    {
        if (a == null || b == null) return;
        Vector3 sA = mainCam.WorldToScreenPoint(a.position);
        Vector3 sB = mainCam.WorldToScreenPoint(b.position);
        if (sA.z <= 0.1f || sB.z <= 0.1f) return;

        DrawLine(new Vector2(sA.x, screenHeight - sA.y), new Vector2(sB.x, screenHeight - sB.y), color, config.lineThickness);
    }

    private static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        Color oldColor = GUI.color;
        Matrix4x4 oldMatrix = GUI.matrix;

        GUI.color = color;
        float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * Mathf.Rad2Deg;
        float length = Vector2.Distance(pointA, pointB);

        GUIUtility.RotateAroundPivot(angle, pointA);
        GUI.DrawTexture(new Rect(pointA.x, pointA.y - width * 0.5f, length, width), solidTexture);

        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
    }

    private static void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        Color oldColor = GUI.color;
        GUI.color = color;

        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), solidTexture); // Top
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), solidTexture); // Bottom
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), solidTexture); // Left
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), solidTexture); // Right

        GUI.color = oldColor;
    }

    private static void DrawMarker(Vector2 center, Color color, float radius)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(center.x - radius, center.y - radius, radius * 2.0f, radius * 2.0f), solidTexture);
        GUI.color = oldColor;
    }

    private void DrawOutlinedText(Rect rect, string text, Color textCol, Color outlineCol)
    {
        Color old = labelStyle.normal.textColor;
        labelStyle.normal.textColor = outlineCol;

        GUI.Label(new Rect(rect.x - 1, rect.y, rect.width, rect.height), text, labelStyle);
        GUI.Label(new Rect(rect.x + 1, rect.y, rect.width, rect.height), text, labelStyle);
        GUI.Label(new Rect(rect.x, rect.y - 1, rect.width, rect.height), text, labelStyle);
        GUI.Label(new Rect(rect.x, rect.y + 1, rect.width, rect.height), text, labelStyle);

        labelStyle.normal.textColor = textCol;
        GUI.Label(rect, text, labelStyle);
        labelStyle.normal.textColor = old;
    }

    private Vector2 GetSnaplineOrigin()
    {
        switch (config.snaplineOrigin)
        {
            case DebugVisualizerConfig.SnaplineOrigin.TopScreen: return new Vector2(screenWidth * 0.5f, 0.0f);
            case DebugVisualizerConfig.SnaplineOrigin.CenterScreen: return new Vector2(screenWidth * 0.5f, screenHeight * 0.5f);
            default: return new Vector2(screenWidth * 0.5f, screenHeight);
        }
    }

    private void SetupLabelStyle()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
        labelStyle.fontSize = Mathf.Max(11, Mathf.RoundToInt(config.baseFontSize * (Screen.dpi > 0 ? Screen.dpi / 160f : 1.0f)));
    }
}

