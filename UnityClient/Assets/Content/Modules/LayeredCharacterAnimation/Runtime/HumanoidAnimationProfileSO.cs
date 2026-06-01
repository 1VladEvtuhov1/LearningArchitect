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

        [Header("Animator Parameters")]
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string moveXParameter;
        [SerializeField] private string moveYParameter = "MoveY";
        [SerializeField] private string turnParameter = "Turn";
        [SerializeField] private string movingParameter = "IsMoving";
        [SerializeField] private string groundedParameter = "IsGrounded";
        [SerializeField] private string shootingParameter = "IsShooting";

        [Header("Runtime Tuning")]
        [SerializeField] private float speedNormalization = 1.8f;
        [SerializeField] private float parameterDampTime = 0.12f;
        [SerializeField] private float turnResponsiveness = 8f;
        [SerializeField] private float movingThreshold = 0.08f;
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
        public string SpeedParameter => speedParameter;
        public string MoveXParameter => moveXParameter;
        public string MoveYParameter => moveYParameter;
        public string TurnParameter => turnParameter;
        public string MovingParameter => movingParameter;
        public string GroundedParameter => groundedParameter;
        public string ShootingParameter => shootingParameter;
        public float SpeedNormalization => Mathf.Max(0.01f, speedNormalization);
        public float ParameterDampTime => Mathf.Max(0f, parameterDampTime);
        public float TurnResponsiveness => Mathf.Max(0f, turnResponsiveness);
        public float MovingThreshold => Mathf.Max(0f, movingThreshold);
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
