using UnityEngine;

[ExecuteAlways]
public class DemoEnvironmentBuilder : MonoBehaviour
{
    private void Start()
    {
        if (Application.isPlaying)
        {
            BuildScene();
        }
    }

    [ContextMenu("Generate Demo Scene")]
    public void BuildScene()
    {
        // Check if scene is already generated
        if (GameObject.Find("Ground_Plane") != null) return;

        // 1. Create Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground_Plane";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(8, 1, 8); // 80m x 80m arena
        SetColor(ground, new Color(0.18f, 0.20f, 0.22f)); // Dark tarmac

        // 2. Spawn Obstacle Buildings
        SpawnBuilding("Building_A", new Vector3(-12, 3, 16), new Vector3(8, 6, 6), new Color(0.32f, 0.35f, 0.40f));
        SpawnBuilding("Building_B", new Vector3(14, 4, 22), new Vector3(6, 8, 8), new Color(0.28f, 0.30f, 0.34f));
        SpawnBuilding("Building_C", new Vector3(0, 2.5f, 32), new Vector3(16, 5, 4), new Color(0.35f, 0.38f, 0.42f));

        // 3. Spawn Test Dummies at Varying Distances
        SpawnDummy("Dummy Alpha (Near)", new Vector3(-3, 1, 6), Color.red);
        SpawnDummy("Dummy Bravo (Mid)", new Vector3(7, 1, 18), new Color(1.0f, 0.55f, 0.0f));
        SpawnDummy("Dummy Charlie (Far)", new Vector3(-8, 1, 35), Color.yellow);
    }

    private void SpawnBuilding(string name, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = name;
        building.transform.position = pos;
        building.transform.localScale = scale;
        SetColor(building, color);
    }

    private void SpawnDummy(string name, Vector3 pos, Color color)
    {
        GameObject dummy = new GameObject(name);
        dummy.transform.position = pos;

        // Body Capsule
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(dummy.transform, false);
        body.transform.localPosition = new Vector3(0, 1.0f, 0);
        body.transform.localScale = new Vector3(0.6f, 1.0f, 0.6f);
        SetColor(body, color);

        // Head Sphere
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(dummy.transform, false);
        head.transform.localPosition = new Vector3(0, 2.05f, 0);
        head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        SetColor(head, Color.white);

        // Joints
        GameObject neck = CreateJoint(dummy.transform, "Neck", new Vector3(0, 1.75f, 0));
        GameObject chest = CreateJoint(dummy.transform, "Chest", new Vector3(0, 1.4f, 0));
        GameObject spine = CreateJoint(dummy.transform, "Spine", new Vector3(0, 1.1f, 0));
        GameObject hips = CreateJoint(dummy.transform, "Hips", new Vector3(0, 0.85f, 0));

        // Arms
        GameObject lShoulder = CreateJoint(dummy.transform, "LeftShoulder", new Vector3(-0.35f, 1.4f, 0));
        GameObject lUpperArm = CreateJoint(dummy.transform, "LeftUpperArm", new Vector3(-0.45f, 1.25f, 0));
        GameObject lLowerArm = CreateJoint(dummy.transform, "LeftLowerArm", new Vector3(-0.45f, 1.0f, 0));
        GameObject lHand = CreateJoint(dummy.transform, "LeftHand", new Vector3(-0.45f, 0.75f, 0));

        GameObject rShoulder = CreateJoint(dummy.transform, "RightShoulder", new Vector3(0.35f, 1.4f, 0));
        GameObject rUpperArm = CreateJoint(dummy.transform, "RightUpperArm", new Vector3(0.45f, 1.25f, 0));
        GameObject rLowerArm = CreateJoint(dummy.transform, "RightLowerArm", new Vector3(0.45f, 1.0f, 0));
        GameObject rHand = CreateJoint(dummy.transform, "RightHand", new Vector3(0.45f, 0.75f, 0));

        // Legs
        GameObject lUpperLeg = CreateJoint(dummy.transform, "LeftUpperLeg", new Vector3(-0.2f, 0.6f, 0));
        GameObject lLowerLeg = CreateJoint(dummy.transform, "LeftLowerLeg", new Vector3(-0.2f, 0.35f, 0));
        GameObject lFoot = CreateJoint(dummy.transform, "LeftFoot", new Vector3(-0.2f, 0.05f, 0));

        GameObject rUpperLeg = CreateJoint(dummy.transform, "RightUpperLeg", new Vector3(0.2f, 0.6f, 0));
        GameObject rLowerLeg = CreateJoint(dummy.transform, "RightLowerLeg", new Vector3(0.2f, 0.35f, 0));
        GameObject rFoot = CreateJoint(dummy.transform, "RightFoot", new Vector3(0.2f, 0.05f, 0));

        // Attach & Bind TargetEntity
        TargetEntity entity = dummy.AddComponent<TargetEntity>();
        entity.entityName = name;
        entity.entityColor = color;
        entity.head = head.transform;
        entity.neck = neck.transform;
        entity.chest = chest.transform;
        entity.spine = spine.transform;
        entity.hips = hips.transform;

        entity.leftShoulder = lShoulder.transform;
        entity.leftUpperArm = lUpperArm.transform;
        entity.leftLowerArm = lLowerArm.transform;
        entity.leftHand = lHand.transform;

        entity.rightShoulder = rShoulder.transform;
        entity.rightUpperArm = rUpperArm.transform;
        entity.rightLowerArm = rLowerArm.transform;
        entity.rightHand = rHand.transform;

        entity.leftUpperLeg = lUpperLeg.transform;
        entity.leftLowerLeg = lLowerLeg.transform;
        entity.leftFoot = lFoot.transform;

        entity.rightUpperLeg = rUpperLeg.transform;
        entity.rightLowerLeg = rLowerLeg.transform;
        entity.rightFoot = rFoot.transform;
    }

    private GameObject CreateJoint(Transform parent, string name, Vector3 localPos)
    {
        GameObject joint = new GameObject(name);
        joint.transform.SetParent(parent, false);
        joint.transform.localPosition = localPos;
        return joint;
    }

    private void SetColor(GameObject go, Color color)
    {
        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            rend.material.color = color;
        }
    }
}

