using UnityEngine;

namespace LearningArchitect.Modules.Animation3D
{
    /// <summary>
    /// Applies clamped horizontal aim to the humanoid spine chain during the animator IK pass.
    /// Twist is applied on top of the evaluated clip pose via SetBoneLocalRotation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HumanoidUpperBodyAimIk : MonoBehaviour
    {
        private readonly struct AimBone
        {
            public readonly HumanBodyBones Bone;
            public readonly float Weight;

            public AimBone(HumanBodyBones bone, float weight)
            {
                Bone = bone;
                Weight = weight;
            }
        }

        private static readonly AimBone[] AimBoneDefinitions =
        {
            new(HumanBodyBones.Spine, 0.12f),
            new(HumanBodyBones.Chest, 0.22f),
            new(HumanBodyBones.UpperChest, 0.26f),
            new(HumanBodyBones.Neck, 0.18f),
            new(HumanBodyBones.Head, 0.22f),
        };

        [SerializeField] private Animator animator;

        private float maxYawDegrees = 75f;
        private float yawSmoothSpeed = 14f;
        private float ikWeightBlendSpeed = 12f;
        private float targetIkWeight = 1f;
        private float ikWeight = 1f;
        private float targetYawDegrees;
        private float smoothedYawDegrees;
        private float lastYawSign = 1f;

        public float CurrentYawDegrees => smoothedYawDegrees;
        public float NormalizedYaw => maxYawDegrees > 0.01f
            ? Mathf.Clamp(smoothedYawDegrees / maxYawDegrees, -1f, 1f)
            : 0f;

        public void Configure(HumanoidAnimationProfileSO profile)
        {
            if (profile == null)
                return;

            maxYawDegrees = profile.MaxUpperBodyAimDegrees;
            yawSmoothSpeed = profile.UpperBodyAimSmoothSpeed;
            ikWeightBlendSpeed = profile.UpperBodyAimWeightBlendSpeed;
        }

        public void SetAim(Vector3 bodyForward, Vector3 desiredAimDirection, float targetWeight = 1f)
        {
            targetIkWeight = Mathf.Clamp01(targetWeight);

            bodyForward.y = 0f;
            desiredAimDirection.y = 0f;
            if (bodyForward.sqrMagnitude < 0.0001f || desiredAimDirection.sqrMagnitude < 0.0001f)
                return;

            bodyForward.Normalize();
            desiredAimDirection.Normalize();

            float rawYaw = HumanoidPlanarAimMath.SignedPlanarYaw(
                bodyForward,
                desiredAimDirection,
                lastYawSign);
            targetYawDegrees = HumanoidPlanarAimMath.ClampSignedYaw(rawYaw, maxYawDegrees);

            if (Mathf.Abs(rawYaw) > 1f)
                lastYawSign = Mathf.Sign(rawYaw);
        }

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (animator == null || !animator.isHuman)
                return;

            float deltaTime = Time.deltaTime;
            float weightBlend = HumanoidPlanarAimMath.ExpLerpFactor(ikWeightBlendSpeed, deltaTime);
            ikWeight = Mathf.Lerp(ikWeight, targetIkWeight, weightBlend);

            float yawBlend = HumanoidPlanarAimMath.ExpLerpFactor(yawSmoothSpeed, deltaTime);
            smoothedYawDegrees = Mathf.Lerp(smoothedYawDegrees, targetYawDegrees, yawBlend);

            float appliedYaw = smoothedYawDegrees * ikWeight;
            if (Mathf.Abs(appliedYaw) < 0.25f)
                return;

            float totalWeight = 0f;
            for (int i = 0; i < AimBoneDefinitions.Length; i++)
            {
                if (animator.GetBoneTransform(AimBoneDefinitions[i].Bone) != null)
                    totalWeight += AimBoneDefinitions[i].Weight;
            }

            if (totalWeight <= 0f)
                return;

            for (int i = 0; i < AimBoneDefinitions.Length; i++)
            {
                AimBone definition = AimBoneDefinitions[i];
                Transform boneTransform = animator.GetBoneTransform(definition.Bone);
                if (boneTransform == null)
                    continue;

                float partYaw = appliedYaw * (definition.Weight / totalWeight);
                if (Mathf.Abs(partYaw) < 0.01f)
                    continue;

                Quaternion baseLocal = boneTransform.localRotation;
                Quaternion twist = Quaternion.AngleAxis(partYaw, Vector3.up);
                animator.SetBoneLocalRotation(definition.Bone, baseLocal * twist);
            }
        }
    }
}
