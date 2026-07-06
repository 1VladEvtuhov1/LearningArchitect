using LearningArchitect.Modules.Animation3D;
using UnityEngine;
using IBodyFacingProvider = LearningArchitect.Modules.InterviewArena.IBodyFacingProvider;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Instantiates a humanoid visual from <see cref="HumanoidAnimationProfileSO"/>
    /// and drives it from root physics instead of a capsule mesh.
    /// Arena mode: lower body faces movement; idle turn-in-place toward aim; upper body IK.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(110)]
    public sealed class ArenaHumanoidVisual : MonoBehaviour, IBodyFacingProvider, IBodyFacingCommit
    {
        [SerializeField] private HumanoidAnimationProfileSO profile;
        [SerializeField] private Transform visualAnchor;
        [SerializeField] private Vector3 localPosition = new Vector3(0f, -1f, 0f);
        [SerializeField] private Vector3 localEulerAngles;
        [SerializeField] private Vector3 localScale = Vector3.one;
        [SerializeField] private bool hideCapsuleRenderer = true;

        private HumanoidCrowdActor crowdActor;
        private HumanoidUpperBodyAimIk upperBodyAim;
        private Animator animator;
        private Rigidbody body;
        private PlayerMotor playerMotor;
        private ArenaHoverMotor hoverMotor;
        private ArenaCursorAim cursorAim;
        private CrossbowWeaponController playerCrossbow;
        private EnemyCrossbowAttack enemyCrossbow;
        private MeleeStrikeController meleeStrike;
        private PlayerCombatStance combatStance;
        private PlayerActionCoordinator actionCoordinator;
        private Health health;
        private Quaternion currentBodyRotation = Quaternion.identity;
        private bool bodyRotationInitialized;
        private bool isTurningInPlace;
        private float lastTurnYawSign = 1f;
        private float currentYawDelta;
        private float turnExitHoldTimer;
        private float lockedTurnDirection = 1f;
        private bool initialized;
        private bool wasJumpAnimActive;
        private bool wasDashAnimActive;
        private bool wasShootingAnimActive;
        private bool wasMeleeAnimActive;
        private bool pendingHitTrigger;

        public HumanoidAnimationProfileSO Profile => profile;
        public Vector3 LogicalBodyForward => currentBodyRotation * Vector3.forward;
        public float PlanarAimLimitDegrees =>
            profile != null ? profile.MaxUpperBodyAimDegrees : 75f;

        public void SnapPlanarFacing(Vector3 planarDirection)
        {
            planarDirection.y = 0f;
            if (planarDirection.sqrMagnitude < 0.001f)
                return;

            currentBodyRotation = Quaternion.LookRotation(planarDirection.normalized, Vector3.up);
            bodyRotationInitialized = true;
            isTurningInPlace = false;
            turnExitHoldTimer = 0f;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            playerMotor = GetComponent<PlayerMotor>();
            hoverMotor = GetComponent<ArenaHoverMotor>();
            cursorAim = GetComponent<ArenaCursorAim>();
            playerCrossbow = GetComponent<CrossbowWeaponController>();
            enemyCrossbow = GetComponent<EnemyCrossbowAttack>();
            meleeStrike = GetComponent<MeleeStrikeController>();
            combatStance = GetComponent<PlayerCombatStance>();
            actionCoordinator = GetComponent<PlayerActionCoordinator>();
            health = GetComponent<Health>();

            if (hideCapsuleRenderer)
                DisableCapsuleRenderer();

            EnsureVisualInstance();
        }

        private void OnEnable()
        {
            if (health != null)
                health.Damaged += HandleDamaged;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Damaged -= HandleDamaged;
        }

        private void HandleDamaged(Health _, DamageInfo __) => pendingHitTrigger = true;

        private void LateUpdate()
        {
            if (profile == null || crowdActor == null)
                return;

            if (!initialized)
            {
                crowdActor.Initialize(profile, 0f);
                animator = crowdActor.Animator;
                EnsureUpperBodyAim();
                initialized = true;
            }

            EnsureBodyRotationInitialized();

            Vector3 velocity = body != null ? body.linearVelocity : Vector3.zero;
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float moveSpeedThreshold = profile.MovingThreshold * profile.SpeedNormalization;
            bool isIdle = flatVelocity.sqrMagnitude < moveSpeedThreshold * moveSpeedThreshold;
            bool actionActive = IsActionAnimActive();

            Vector3 aimDirection = ResolveAimDirection();
            Vector3 currentBodyForward = LogicalBodyForward;
            Vector3 desiredBodyForward = ResolveDesiredBodyForward(flatVelocity, aimDirection, isIdle, currentBodyForward);

            currentYawDelta = HumanoidPlanarAimMath.StabilizeSignedYaw(
                currentBodyForward,
                desiredBodyForward,
                ref lastTurnYawSign);

            UpdateTurnInPlaceState(isIdle, actionActive, currentYawDelta);

            if (!actionActive && (actionCoordinator == null || actionCoordinator.CanTurn))
                StepBodyRotation(isIdle, currentYawDelta, Time.deltaTime);

            ApplyVisualAnchorRotation();

            Vector3 bodyForward = LogicalBodyForward;
            ApplyUpperBodyAim(bodyForward, aimDirection, isIdle, actionActive);

            HumanoidLocomotionAnimation.ResolveLocalMove(
                flatVelocity,
                bodyForward,
                profile.SpeedNormalization,
                out float moveX,
                out float moveY,
                out float speed01);

            if (actionActive)
            {
                speed01 = 0f;
                moveX = 0f;
                moveY = 0f;
            }
            else if (isIdle || speed01 < profile.MovingThreshold)
            {
                speed01 = 0f;
                moveX = 0f;
                moveY = 0f;
            }

            float turnAmount = ResolveTurnAmount(flatVelocity, bodyForward);
            bool grounded = ResolveGrounded();
            ApplyActionTriggers();
            ApplyAnimatorParameters(speed01, moveX, moveY, turnAmount, grounded, actionActive, Time.deltaTime);
        }

        public void ApplyProfile(HumanoidAnimationProfileSO animationProfile, Transform anchor = null)
        {
            profile = animationProfile;
            if (anchor != null)
                visualAnchor = anchor;

            initialized = false;
            animator = null;
            upperBodyAim = null;
            currentBodyRotation = Quaternion.identity;
            bodyRotationInitialized = false;
            isTurningInPlace = false;
            lastTurnYawSign = 1f;
            currentYawDelta = 0f;
            turnExitHoldTimer = 0f;
            lockedTurnDirection = 1f;
            wasJumpAnimActive = false;
            wasDashAnimActive = false;
            wasShootingAnimActive = false;
            wasMeleeAnimActive = false;
            pendingHitTrigger = false;
            EnsureVisualInstance();
        }

        private void EnsureBodyRotationInitialized()
        {
            if (bodyRotationInitialized)
                return;

            Vector3 seed = ResolveAimDirection();
            if (seed.sqrMagnitude < 0.001f)
                seed = transform.forward;

            seed.y = 0f;
            if (seed.sqrMagnitude < 0.001f)
                seed = Vector3.forward;

            currentBodyRotation = Quaternion.LookRotation(seed.normalized, Vector3.up);
            bodyRotationInitialized = true;
        }

        private Vector3 ResolveAimDirection()
        {
            if (cursorAim != null && cursorAim.AimDirection.sqrMagnitude > 0.001f)
                return cursorAim.AimDirection;

            Transform aimPivot = ResolveAimPivot();
            if (aimPivot != null)
            {
                Vector3 forward = aimPivot.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.001f)
                    return forward.normalized;
            }

            return LogicalBodyForward.sqrMagnitude > 0.001f ? LogicalBodyForward : Vector3.forward;
        }

        private static Vector3 ResolveDesiredBodyForward(
            Vector3 flatVelocity,
            Vector3 aimDirection,
            bool isIdle,
            Vector3 currentBodyForward)
        {
            if (!isIdle)
            {
                if (flatVelocity.sqrMagnitude < 0.0001f)
                    return currentBodyForward;

                return flatVelocity.normalized;
            }

            if (aimDirection.sqrMagnitude < 0.001f)
                return currentBodyForward;

            return aimDirection.normalized;
        }

        private Transform ResolveAimPivot()
        {
            if (cursorAim != null)
                return cursorAim.AimPivot;

            if (playerMotor != null)
                return playerMotor.ViewPivot;

            return transform;
        }

        private void UpdateTurnInPlaceState(bool isIdle, bool actionActive, float yawDelta)
        {
            if (actionActive)
            {
                isTurningInPlace = false;
                turnExitHoldTimer = 0f;
                return;
            }

            float absYaw = Mathf.Abs(yawDelta);
            if (!isTurningInPlace)
            {
                if (isIdle && absYaw > profile.IdleTurnStartAngle)
                {
                    isTurningInPlace = true;
                    turnExitHoldTimer = 0f;
                    float sign = Mathf.Abs(yawDelta) > 0.01f ? Mathf.Sign(yawDelta) : lastTurnYawSign;
                    lockedTurnDirection = sign == 0f ? 1f : sign;
                }

                return;
            }

            if (!isIdle)
            {
                isTurningInPlace = false;
                turnExitHoldTimer = 0f;
                return;
            }

            if (absYaw < profile.IdleTurnStopAngle)
            {
                turnExitHoldTimer += Time.deltaTime;
                if (turnExitHoldTimer >= profile.IdleTurnExitHoldTime)
                    isTurningInPlace = false;
            }
            else
            {
                turnExitHoldTimer = 0f;
            }
        }

        private void StepBodyRotation(bool isIdle, float yawDelta, float deltaTime)
        {
            // Code owns logical body yaw; turn clips are footwork only (profile.ApplyRootMotion stays off).
            if (isIdle && !isTurningInPlace)
                return;

            float maxTurnSpeed = isIdle
                ? profile.TurnInPlaceDegreesPerSecond
                : profile.MovingTurnDegreesPerSecond;

            float maxStep = maxTurnSpeed * deltaTime;
            float yawStep = Mathf.Clamp(yawDelta, -maxStep, maxStep);
            currentBodyRotation = Quaternion.AngleAxis(yawStep, Vector3.up) * currentBodyRotation;
        }

        private void ApplyVisualAnchorRotation()
        {
            Transform pivot = visualAnchor != null ? visualAnchor : crowdActor.transform;
            Transform aimPivot = ResolveAimPivot();
            bool bowStance = combatStance == null || combatStance.IsBowAimStance;
            float yawOffset = bowStance ? profile.VisualYawOffsetDegrees : profile.VisualYawOffsetMeleeDegrees;

            Quaternion desiredVisualWorld =
                currentBodyRotation * Quaternion.Euler(0f, yawOffset, 0f);

            if (pivot.parent != null && aimPivot != null && pivot.parent == aimPivot)
                pivot.localRotation = Quaternion.Inverse(aimPivot.rotation) * desiredVisualWorld;
            else
                pivot.rotation = desiredVisualWorld;
        }

        private void ApplyUpperBodyAim(
            Vector3 bodyForward,
            Vector3 aimDirection,
            bool isIdle,
            bool actionActive)
        {
            if (upperBodyAim == null)
                return;

            float targetWeight = 1f;
            if (actionActive)
                targetWeight = 0f;
            else if (isTurningInPlace)
                targetWeight = profile.TurnInPlaceAimIkWeight;

            upperBodyAim.SetAim(bodyForward, aimDirection, targetWeight);
        }

        private static float ResolveTurnAmount(Vector3 flatVelocity, Vector3 bodyForward)
        {
            if (flatVelocity.sqrMagnitude < 0.01f || bodyForward.sqrMagnitude < 0.001f)
                return 0f;

            bodyForward.y = 0f;
            if (bodyForward.sqrMagnitude < 0.001f)
                return 0f;

            return Mathf.Clamp(
                Vector3.SignedAngle(bodyForward.normalized, flatVelocity.normalized, Vector3.up) / 90f,
                -1f,
                1f);
        }

        private bool ResolveGrounded()
        {
            if (playerMotor != null)
                return playerMotor.IsGrounded;

            if (hoverMotor != null)
                return hoverMotor.IsGrounded;

            return true;
        }

        private void ApplyActionTriggers()
        {
            if (animator == null || profile == null)
                return;

            if (playerMotor != null)
            {
                bool jumpAnimActive = playerMotor.IsJumpAnimActive;
                if (jumpAnimActive && !wasJumpAnimActive)
                {
                    SetBool(profile.JumpBackwardParameter, playerMotor.JumpAnimBackward);
                    SetTrigger(profile.JumpTriggerParameter);
                }

                wasJumpAnimActive = jumpAnimActive;

                bool dashAnimActive = playerMotor.IsDashAnimActive;
                if (dashAnimActive && !wasDashAnimActive)
                    SetTrigger(profile.DashTriggerParameter);

                wasDashAnimActive = dashAnimActive;
            }

            bool shootingAnimActive = IsShooting();
            if (shootingAnimActive && !wasShootingAnimActive)
                SetTrigger(profile.FireTriggerParameter);

            wasShootingAnimActive = shootingAnimActive;

            bool meleeAnimActive = meleeStrike != null && meleeStrike.IsMeleeAnimActive;
            if (meleeAnimActive && !wasMeleeAnimActive)
                SetTrigger(profile.MeleeTriggerParameter);

            wasMeleeAnimActive = meleeAnimActive;

            if (pendingHitTrigger)
            {
                SetTrigger(profile.HitTriggerParameter);
                pendingHitTrigger = false;
            }
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

            if (!Application.isPlaying)
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

        private void EnsureUpperBodyAim()
        {
            if (crowdActor == null || profile == null)
                return;

            upperBodyAim = crowdActor.GetComponent<HumanoidUpperBodyAimIk>();
            if (upperBodyAim == null)
                upperBodyAim = crowdActor.gameObject.AddComponent<HumanoidUpperBodyAimIk>();

            upperBodyAim.Configure(profile);
        }

        private void DisableCapsuleRenderer()
        {
            if (TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
                renderer.enabled = false;
        }

        private bool IsActionAnimActive()
        {
            if (playerMotor != null &&
                (playerMotor.IsJumpAnimActive || playerMotor.IsDashAnimActive))
                return true;

            if (actionCoordinator != null && actionCoordinator.HasActiveAction)
                return true;

            if (IsShooting())
                return true;

            return false;
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
            bool grounded,
            bool actionActive,
            float deltaTime)
        {
            if (animator == null || profile == null)
                return;

            float dampTime = profile.ParameterDampTime;
            bool isMoving = !actionActive && speed01 >= profile.MovingThreshold;

            SetFloat(profile.SpeedParameter, speed01, dampTime, deltaTime);
            SetFloat(profile.MoveXParameter, moveX, dampTime, deltaTime);
            SetFloat(profile.MoveYParameter, moveY, dampTime, deltaTime);
            SetFloat(profile.TurnParameter, turnAmount, dampTime, deltaTime);
            SetBool(profile.MovingParameter, isMoving);
            SetBool(profile.GroundedParameter, grounded);

            bool bowStance = combatStance == null || combatStance.IsBowAimStance;
            SetBool(profile.AimStanceParameter, bowStance);

            bool turnActive = isTurningInPlace && !actionActive;
            SetBool(profile.TurnInPlaceParameter, turnActive);

            float turnDirection = 0f;
            if (turnActive)
            {
                turnDirection = lockedTurnDirection;
                if (profile.InvertTurnDirection)
                    turnDirection = -turnDirection;
            }

            SetFloat(
                profile.TurnDirectionParameter,
                turnDirection,
                profile.TurnInPlaceAnimatorDampTime,
                deltaTime);
            SetFloat(profile.TurnAngleParameter, currentYawDelta, 0f, 0f);

            if (upperBodyAim != null)
                SetFloat(profile.AimYawParameter, upperBodyAim.NormalizedYaw, dampTime, deltaTime);
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

        private void SetTrigger(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                return;

            animator.SetTrigger(parameterName);
        }
    }
}
