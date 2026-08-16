using UnityEngine;

namespace LearningArchitect.Modules.Animation3D
{
    [CreateAssetMenu(menuName = "Learning Architect/Animation/Humanoid Animation Profile", fileName = "HumanoidAnimationProfile")]
    public sealed class HumanoidAnimationProfileSO : ScriptableObject
    {
        [Header("Actor")]
        [SerializeField] private GameObject actorPrefab;
        [SerializeField] private Avatar avatar;
        [SerializeField] private RuntimeAnimatorController animatorController;

        [Header("Reference Clips")]
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip locomotionClip;
        [SerializeField] private AnimationClip turnLeftClip;
        [SerializeField] private AnimationClip turnRightClip;
        [SerializeField] private AnimationClip jumpClip;
        [SerializeField] private AnimationClip jumpBackwardClip;

        [Header("Animator Parameters")]
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string moveXParameter;
        [SerializeField] private string moveYParameter = "MoveY";
        [SerializeField] private string turnParameter = "Turn";
        [SerializeField] private string movingParameter = "IsMoving";
        [SerializeField] private string groundedParameter = "IsGrounded";
        [SerializeField] private string shootingParameter = "IsShooting";
        [SerializeField] private string fireTriggerParameter = "Fire";
        [SerializeField] private string jumpTriggerParameter = "Jump";
        [SerializeField] private string jumpBackwardParameter = "JumpBackward";
        [SerializeField] private string dashTriggerParameter = "Dash";
        [SerializeField] private string meleeTriggerParameter = "Melee";
        [SerializeField] private string hitTriggerParameter = "Hit";
        [SerializeField] private string hitFrontTriggerParameter = "HitFront";
        [SerializeField] private string hitBackTriggerParameter = "HitBack";
        [SerializeField] private string hitLeftTriggerParameter = "HitLeft";
        [SerializeField] private string hitRightTriggerParameter = "HitRight";
        [SerializeField] private string isBlockingParameter = "IsBlocking";
        [SerializeField] private string blockStartTriggerParameter = "BlockStart";
        [SerializeField] private string turnInPlaceParameter = "TurnInPlace";
        [SerializeField] private string turnDirectionParameter = "TurnDirection";
        [SerializeField] private string turnAngleParameter = "TurnAngle";

        [Header("Runtime Tuning")]
        [Tooltip("Single authored facing bake for this profile relative to LogicalBodyForward. Clip packs must share one convention.")]
        [SerializeField] private float visualYawOffsetDegrees;
        [Tooltip("Legacy melee-only offset. Prefer matching VisualYawOffsetDegrees; Arena ignores this when packs are aligned.")]
        [SerializeField] private float visualYawOffsetMeleeDegrees;
        [SerializeField] private string aimStanceParameter = "IsBowStance";
        [SerializeField] private string aimYawParameter = "AimYaw";
        [SerializeField] private float speedNormalization = 1.8f;
        [SerializeField] private float parameterDampTime = 0.12f;
        [SerializeField] private float turnResponsiveness = 8f;
        [SerializeField] private float movingThreshold = 0.08f;
        [Header("Upper Body Aim")]
        [SerializeField] private float maxUpperBodyAimDegrees = 75f;
        [SerializeField] private float upperBodyAimBodyWeight = 0.35f;
        [SerializeField] private float upperBodyAimHeadWeight = 0.85f;
        [SerializeField] private float upperBodyAimEyesWeight = 1f;
        [SerializeField] private float upperBodyAimClampWeight = 0.45f;
        [SerializeField] private float upperBodyAimLookDistance = 8f;
        [SerializeField] private float upperBodyAimSmoothSpeed = 14f;
        [SerializeField] private float upperBodyAimWeightBlendSpeed = 12f;
        [Header("Turn In Place")]
        [SerializeField] private float idleTurnStartAngle = 20f;
        [SerializeField] private float idleTurnStopAngle = 5f;
        [SerializeField] private float idleTurnExitHoldTime = 0.2f;
        [SerializeField] private bool invertTurnDirection;
        [SerializeField] private float turnInPlaceDegreesPerSecond = 240f;
        [SerializeField] private float turnInPlaceAnimatorDampTime = 0.08f;
        [SerializeField] private float turnInPlaceAimIkWeight = 0.65f;
        [SerializeField] private float movingTurnDegreesPerSecond = 720f;
        [SerializeField] private bool randomizeStartTime = true;
        [SerializeField] private bool applyRootMotion;
        [SerializeField] private AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal;
        [SerializeField] private AnimatorCullingMode cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        public GameObject ActorPrefab => actorPrefab;
        public Avatar Avatar => avatar;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public AnimationClip IdleClip => idleClip;
        public AnimationClip LocomotionClip => locomotionClip;
        public AnimationClip TurnLeftClip => turnLeftClip;
        public AnimationClip TurnRightClip => turnRightClip;
        public AnimationClip JumpClip => jumpClip;
        public AnimationClip JumpBackwardClip => jumpBackwardClip;
        public string SpeedParameter => speedParameter;
        public string MoveXParameter => moveXParameter;
        public string MoveYParameter => moveYParameter;
        public string TurnParameter => turnParameter;
        public string MovingParameter => movingParameter;
        public string GroundedParameter => groundedParameter;
        public string ShootingParameter => shootingParameter;
        public string FireTriggerParameter => fireTriggerParameter;
        public string JumpTriggerParameter => jumpTriggerParameter;
        public string JumpBackwardParameter => jumpBackwardParameter;
        public string DashTriggerParameter => dashTriggerParameter;
        public string MeleeTriggerParameter => meleeTriggerParameter;
        public string HitTriggerParameter => hitTriggerParameter;
        public string HitFrontTriggerParameter => hitFrontTriggerParameter;
        public string HitBackTriggerParameter => hitBackTriggerParameter;
        public string HitLeftTriggerParameter => hitLeftTriggerParameter;
        public string HitRightTriggerParameter => hitRightTriggerParameter;
        public string IsBlockingParameter => isBlockingParameter;
        public string BlockStartTriggerParameter => blockStartTriggerParameter;
        public string TurnInPlaceParameter => turnInPlaceParameter;
        public string TurnDirectionParameter => turnDirectionParameter;
        public string TurnAngleParameter => turnAngleParameter;
        public float VisualYawOffsetDegrees => visualYawOffsetDegrees;
        public float VisualYawOffsetMeleeDegrees => visualYawOffsetMeleeDegrees;
        public string AimStanceParameter => aimStanceParameter;
        public string AimYawParameter => aimYawParameter;
        public float SpeedNormalization => Mathf.Max(0.01f, speedNormalization);
        public float ParameterDampTime => Mathf.Max(0f, parameterDampTime);
        public float TurnResponsiveness => Mathf.Max(0f, turnResponsiveness);
        public float MovingThreshold => Mathf.Max(0f, movingThreshold);
        public float MaxUpperBodyAimDegrees => Mathf.Clamp(maxUpperBodyAimDegrees, 0f, 120f);
        public float UpperBodyAimBodyWeight => Mathf.Clamp01(upperBodyAimBodyWeight);
        public float UpperBodyAimHeadWeight => Mathf.Clamp01(upperBodyAimHeadWeight);
        public float UpperBodyAimEyesWeight => Mathf.Clamp01(upperBodyAimEyesWeight);
        public float UpperBodyAimClampWeight => Mathf.Clamp01(upperBodyAimClampWeight);
        public float UpperBodyAimLookDistance => Mathf.Max(1f, upperBodyAimLookDistance);
        public float UpperBodyAimSmoothSpeed => Mathf.Max(0f, upperBodyAimSmoothSpeed);
        public float UpperBodyAimWeightBlendSpeed => Mathf.Max(0f, upperBodyAimWeightBlendSpeed);
        public float IdleTurnStartAngle => Mathf.Max(0f, idleTurnStartAngle);
        public float IdleTurnStopAngle => Mathf.Max(0f, idleTurnStopAngle);
        public float IdleTurnExitHoldTime => Mathf.Max(0f, idleTurnExitHoldTime);
        public bool InvertTurnDirection => invertTurnDirection;
        public float TurnInPlaceDegreesPerSecond => Mathf.Max(0f, turnInPlaceDegreesPerSecond);
        public float TurnInPlaceAnimatorDampTime => Mathf.Max(0f, turnInPlaceAnimatorDampTime);
        public float TurnInPlaceAimIkWeight => Mathf.Clamp01(turnInPlaceAimIkWeight);
        public float MovingTurnDegreesPerSecond => Mathf.Max(0f, movingTurnDegreesPerSecond);
        public bool RandomizeStartTime => randomizeStartTime;
        public bool ApplyRootMotion => applyRootMotion;
        public AnimatorUpdateMode UpdateMode => updateMode;
        public AnimatorCullingMode CullingMode => cullingMode;

        public bool HasActorPrefab => actorPrefab != null;
        public bool HasAnimatorController => animatorController != null;
        public bool HasAvatar => avatar != null;
        public bool ActorPrefabHasAnimator => actorPrefab != null && actorPrefab.GetComponentInChildren<Animator>(true) != null;
        public bool ActorPrefabHasCrowdActor => actorPrefab != null && actorPrefab.GetComponent<HumanoidCrowdActor>() != null;
    }
}
