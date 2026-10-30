using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TargetEntity : MonoBehaviour
{
    [Header("Entity Metadata")]
    public string entityName = "Enemy Dummy";
    public Color entityColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);
    public bool isAlive = true;

    [Header("Physical Dimensions (Fallback if no collider)")]
    public float modelHeight = 1.9f;
    public float modelWidth = 0.6f;

    [Header("Complete 15-Bone Skeleton Rig")]
    public Transform head;
    public Transform neck;
    public Transform chest;
    public Transform spine;
    public Transform hips;

    // Left Arm
    public Transform leftShoulder;
    public Transform leftUpperArm;
    public Transform leftLowerArm;
    public Transform leftHand;

    // Right Arm
    public Transform rightShoulder;
    public Transform rightUpperArm;
    public Transform rightLowerArm;
    public Transform rightHand;

    // Left Leg
    public Transform leftUpperLeg;
    public Transform leftLowerLeg;
    public Transform leftFoot;

    // Right Leg
    public Transform rightUpperLeg;
    public Transform rightLowerLeg;
    public Transform rightFoot;

    // Static registry of all active targets
    public static readonly List<TargetEntity> ActiveEntities = new List<TargetEntity>();

    private void OnEnable()
    {
        if (!ActiveEntities.Contains(this))
            ActiveEntities.Add(this);

        AutoDetectBones();
    }

    private void OnDisable()
    {
        ActiveEntities.Remove(this);
    }

    public void AutoDetectBones()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman)
        {
            head = animator.GetBoneTransform(HumanBodyBones.Head);
            neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            chest = animator.GetBoneTransform(HumanBodyBones.Chest) ?? animator.GetBoneTransform(HumanBodyBones.Spine);
            spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);

            leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

            rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

            leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

            rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
    }

    public Vector3 GetTopPosition()
    {
        if (head != null) return head.position + Vector3.up * 0.22f;
        return transform.position + Vector3.up * modelHeight;
    }

    public Vector3 GetBottomPosition()
    {
        if (leftFoot != null && rightFoot != null)
            return (leftFoot.position + rightFoot.position) * 0.5f;
        return transform.position;
    }
}

