using LearningArchitect.Modules.Animation3D;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Instantiates a humanoid visual from <see cref="HumanoidAnimationProfileSO"/>
    /// and drives it from root physics instead of a capsule mesh.
    /// Arena mode: body yaw follows cursor aim; legs animate from planar speed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ArenaHumanoidVisual : MonoBehaviour
    {
        [SerializeField] private HumanoidAnimationProfileSO profile;
        [SerializeField] private Transform visualAnchor;
        [SerializeField] private Vector3 localPosition = new Vector3(0f, -1f, 0f);
        [SerializeField] private Vector3 localEulerAngles;
        [SerializeField] private Vector3 localScale = Vector3.one;
        [SerializeField] private bool hideCapsuleRenderer = true;

        private HumanoidCrowdActor crowdActor;
        private Animator animator;
        private Rigidbody body;
        private PlayerMotor playerMotor;
        private ArenaCursorAim cursorAim;
        private CrossbowWeaponController playerCrossbow;
        private EnemyCrossbowAttack enemyCrossbow;
        private bool initialized;

        public HumanoidAnimationProfileSO Profile => profile;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            playerMotor = GetComponent<PlayerMotor>();
            cursorAim = GetComponent<ArenaCursorAim>();
            playerCrossbow = GetComponent<CrossbowWeaponController>();
            enemyCrossbow = GetComponent<EnemyCrossbowAttack>();

            if (hideCapsuleRenderer)
                DisableCapsuleRenderer();

            EnsureVisualInstance();
        }

        private void LateUpdate()
        {
            if (profile == null || crowdActor == null)
                return;

            if (!initialized)
            {
                crowdActor.Initialize(profile, 0f);
                animator = crowdActor.Animator;
                initialized = true;
            }

            Transform aimPivot = ResolveAimPivot();
            ApplyBodyYaw(aimPivot);

            Vector3 velocity = body != null ? body.linearVelocity : Vector3.zero;
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);

            Vector3 bodyForward = aimPivot != null ? aimPivot.forward : transform.forward;
            HumanoidLocomotionAnimation.ResolveLocalMove(
                flatVelocity,
                bodyForward,
                profile.SpeedNormalization,
                out float moveX,
                out float moveY,
                out float speed01);

            float turnAmount = ResolveStrafeTurn(flatVelocity, aimPivot);
            ApplyAnimatorParameters(speed01, moveX, moveY, turnAmount, Time.deltaTime, IsShooting());
        }

        public void ApplyProfile(HumanoidAnimationProfileSO animationProfile, Transform anchor = null)
        {
            profile = animationProfile;
            if (anchor != null)
                visualAnchor = anchor;

            initialized = false;
            animator = null;
            EnsureVisualInstance();
        }

        private Transform ResolveAimPivot()
        {
            if (cursorAim != null)
                return cursorAim.AimPivot;

            if (playerMotor != null)
                return playerMotor.ViewPivot;

            return transform;
        }

        private void ApplyBodyYaw(Transform aimPivot)
        {
            if (aimPivot == null)
                return;

            Vector3 aimForward = aimPivot.forward;
            aimForward.y = 0f;
            if (aimForward.sqrMagnitude < 0.001f)
                return;

            Quaternion bodyRotation = Quaternion.LookRotation(aimForward.normalized, Vector3.up);
            Transform pivot = visualAnchor != null ? visualAnchor : crowdActor.transform;
            pivot.rotation = bodyRotation;
        }

        private static float ResolveStrafeTurn(Vector3 flatVelocity, Transform aimPivot)
        {
            if (flatVelocity.sqrMagnitude < 0.01f || aimPivot == null)
                return 0f;

            Vector3 aimForward = aimPivot.forward;
            aimForward.y = 0f;
            if (aimForward.sqrMagnitude < 0.001f)
                return 0f;

            return Mathf.Clamp(
                Vector3.SignedAngle(aimForward.normalized, flatVelocity.normalized, Vector3.up) / 90f,
                -1f,
                1f);
        }

        private void EnsureVisualInstance()
        {
            if (profile == null || !profile.HasActorPrefab)
            {
                InterviewArenaAuthoringLog.MissingReference(this, nameof(profile));
                return;
            }

            Transform parent = visualAnchor != null ? visualAnchor : transform;
            if (crowdActor == null)
                crowdActor = parent.GetComponentInChildren<HumanoidCrowdActor>(true);

            if (crowdActor != null)
                return;

            GameObject instance = Instantiate(profile.ActorPrefab, parent);
            instance.name = "HumanoidVisual";
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(localEulerAngles);
            instance.transform.localScale = localScale;
            crowdActor = instance.GetComponent<HumanoidCrowdActor>();
            if (crowdActor == null)
                crowdActor = instance.AddComponent<HumanoidCrowdActor>();

            initialized = false;
        }

        private void DisableCapsuleRenderer()
        {
            if (TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
                renderer.enabled = false;
        }

        private bool IsShooting()
        {
            if (playerCrossbow != null && playerCrossbow.IsShootingAnimActive)
                return true;

            if (enemyCrossbow != null && enemyCrossbow.IsShootingAnimActive)
                return true;

            return false;
        }

        private void ApplyAnimatorParameters(
            float speed01,
            float moveX,
            float moveY,
            float turnAmount,
            float deltaTime,
            bool isShooting)
        {
            if (animator == null || profile == null)
                return;

            float dampTime = profile.ParameterDampTime;
            bool isMoving = speed01 >= profile.MovingThreshold;

            SetFloat(profile.SpeedParameter, speed01, dampTime, deltaTime);
            SetFloat(profile.MoveXParameter, moveX, dampTime, deltaTime);
            SetFloat(profile.MoveYParameter, moveY, dampTime, deltaTime);
            SetFloat(profile.TurnParameter, turnAmount, dampTime, deltaTime);
            SetBool(profile.MovingParameter, isMoving);
            SetBool(profile.GroundedParameter, true);
            SetBool(profile.ShootingParameter, isShooting);
        }

        private void SetFloat(string parameterName, float value, float dampTime, float deltaTime)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                return;

            if (dampTime > 0f)
                animator.SetFloat(parameterName, value, dampTime, deltaTime);
            else
                animator.SetFloat(parameterName, value);
        }

        private void SetBool(string parameterName, bool value)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                return;

            animator.SetBool(parameterName, value);
        }
    }
}
