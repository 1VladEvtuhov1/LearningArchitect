using System;
using LearningArchitect.Modules.Animation3D;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Drives a humanoid visual from <see cref="HumanoidAnimationProfileSO"/>.
    /// Nested actor under <see cref="visualAnchor"/> is required (Edit Mode + Play Mode).
    /// Arena presentation is melee-only: sword locomotion, no Fire / bow overlay / aim IK.
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
        [Tooltip("Melee prop under the actor (WeaponSocket_R / Longsword). Auto-resolved if empty.")]
        [SerializeField] private Transform meleeWeaponRoot;
        [Header("Debug Gizmos")]
        [SerializeField] private bool drawFacingGizmos = true;
        [SerializeField] private float facingGizmoLength = 1.75f;
        [SerializeField] private float facingGizmoHeight = 1.1f;

        private HumanoidCrowdActor crowdActor;
        private Animator animator;
        private Rigidbody body;
        private PlayerMotor playerMotor;
        private ArenaCursorAim cursorAim;
        private BowAimDirectionProvider bowAimProvider;
        private MeleeStrikeController meleeStrike;
        private EnemyMeleeAttack enemyMelee;
        private EnemyCrossbowAttack enemyCrossbow;
        private MeleeBlockController meleeBlock;
        private PlayerActionCoordinator actionCoordinator;
        private Health health;
        private Quaternion currentBodyRotation = Quaternion.identity;
        private bool bodyRotationInitialized;
        private bool isTurningInPlace;
        private float lastTurnYawSign = 1f;
        private float currentYawDelta;
        private float turnExitHoldTimer;
        private float lockedTurnDirection = 1f;
        private bool wasJumpAnimActive;
        private bool wasDashAnimActive;
        private int lastMeleeAnimStartVersion;
        private int lastRangedAnimStartVersion;
        private int lastBlockStartVersion;
        private bool pendingHitTrigger;
        private Vector3 pendingHitDirection;

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
            cursorAim = GetComponent<ArenaCursorAim>();
            bowAimProvider = GetComponent<BowAimDirectionProvider>();
            meleeStrike = GetComponent<MeleeStrikeController>();
            enemyMelee = GetComponent<EnemyMeleeAttack>();
            enemyCrossbow = GetComponent<EnemyCrossbowAttack>();
            meleeBlock = GetComponent<MeleeBlockController>();
            actionCoordinator = GetComponent<PlayerActionCoordinator>();
            health = GetComponent<Health>();

            if (hideCapsuleRenderer)
                DisableCapsuleRenderer();

            BindVisualActor();
            SyncBowAimProviderLimit();
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

        private void HandleDamaged(Health _, DamageInfo info)
        {
            pendingHitTrigger = true;
            pendingHitDirection = info.HitDirection;
        }

        private void LateUpdate()
        {
            Vector3 flatVelocity = ReadFlatVelocity();
            float moveSpeedThreshold = profile.MovingThreshold * profile.SpeedNormalization;
            bool isIdle = flatVelocity.sqrMagnitude < moveSpeedThreshold * moveSpeedThreshold;
            bool actionActive = IsActionAnimActive();

            ApplyFacing(isIdle, actionActive);
            ApplyLocomotion(flatVelocity, isIdle, actionActive);
            return;

            Vector3 ReadFlatVelocity()
            {
                Vector3 velocity = body != null ? body.linearVelocity : Vector3.zero;
                return new Vector3(velocity.x, 0f, velocity.z);
            }

            void ApplyFacing(bool idle, bool actionLocked)
            {
                EnsureBodyRotationInitialized();

                Vector3 cursorAimDirection = ResolveAimDirection();
                Vector3 currentBodyForward = LogicalBodyForward;
                Vector3 desiredBodyForward = BodyFacingRules.ResolveDesiredForward(
                    idle,
                    flatVelocity,
                    cursorAimDirection,
                    currentBodyForward);

                currentYawDelta = HumanoidPlanarAimMath.StabilizeSignedYaw(
                    currentBodyForward,
                    desiredBodyForward,
                    ref lastTurnYawSign);

                UpdateTurnInPlaceState(idle, actionLocked, currentYawDelta);

                if (actionCoordinator == null || actionCoordinator.CanTurn)
                    StepBodyRotation(idle, currentYawDelta, Time.deltaTime);

                ApplyVisualAnchorRotation();
            }

            void ApplyLocomotion(Vector3 velocity, bool idle, bool actionLocked)
            {
                HumanoidLocomotionAnimation.ResolveLocalMove(
                    velocity,
                    LogicalBodyForward,
                    profile.SpeedNormalization,
                    out float moveX,
                    out float moveY,
                    out float speed01);

                if (actionLocked || idle || speed01 < profile.MovingThreshold)
                {
                    moveX = 0f;
                    moveY = 0f;
                }

                ApplyActionTriggers();
                ApplyAnimatorParameters(moveX, moveY, actionLocked, Time.deltaTime);
            }
        }

        public void ApplyProfile(HumanoidAnimationProfileSO animationProfile, Transform anchor = null)
        {
            profile = animationProfile;
            if (anchor != null)
                visualAnchor = anchor;

            animator = null;
            currentBodyRotation = Quaternion.identity;
            bodyRotationInitialized = false;
            isTurningInPlace = false;
            lastTurnYawSign = 1f;
            currentYawDelta = 0f;
            turnExitHoldTimer = 0f;
            lockedTurnDirection = 1f;
            wasJumpAnimActive = false;
            wasDashAnimActive = false;
            lastMeleeAnimStartVersion = 0;
            lastRangedAnimStartVersion = 0;
            lastBlockStartVersion = 0;
            pendingHitTrigger = false;
            pendingHitDirection = Vector3.zero;
            crowdActor = null;
            BindVisualActor();
            SyncBowAimProviderLimit();
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
            float yawOffset = profile.VisualYawOffsetDegrees;

            Quaternion desiredVisualWorld =
                currentBodyRotation * Quaternion.Euler(0f, yawOffset, 0f);

            if (pivot.parent != null && aimPivot != null && pivot.parent == aimPivot)
                pivot.localRotation = Quaternion.Inverse(aimPivot.rotation) * desiredVisualWorld;
            else
                pivot.rotation = desiredVisualWorld;
        }

        private void SyncBowAimProviderLimit()
        {
            if (bowAimProvider != null)
                bowAimProvider.ApplyMaxAimYawDegrees(PlanarAimLimitDegrees);
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

            if (meleeStrike != null)
            {
                PulseTriggerOnVersionChange(
                    ref lastMeleeAnimStartVersion,
                    meleeStrike.MeleeAnimStartVersion,
                    meleeStrike.ActiveMeleeAnimTrigger);
            }
            else if (enemyMelee != null)
            {
                PulseTriggerOnVersionChange(
                    ref lastMeleeAnimStartVersion,
                    enemyMelee.MeleeAnimStartVersion,
                    enemyMelee.ActiveMeleeAnimTrigger);
            }

            if (enemyCrossbow != null)
            {
                PulseTriggerOnVersionChange(
                    ref lastRangedAnimStartVersion,
                    enemyCrossbow.ShootAnimStartVersion,
                    profile.MeleeTriggerParameter);
            }

            if (meleeBlock == null)
                meleeBlock = GetComponent<MeleeBlockController>();

            if (meleeBlock != null)
            {
                SetBool(profile.IsBlockingParameter, meleeBlock.IsBlocking);
                int blockVersion = meleeBlock.BlockStartVersion;
                if (blockVersion != lastBlockStartVersion)
                {
                    lastBlockStartVersion = blockVersion;
                    if (blockVersion > 0)
                        PulseTrigger(profile.BlockStartTriggerParameter);
                }
            }

            if (pendingHitTrigger)
            {
                PulseTrigger(ResolveHitTrigger(pendingHitDirection));
                pendingHitTrigger = false;
                pendingHitDirection = Vector3.zero;
            }
        }

        private void BindVisualActor()
        {
            if (profile == null || !profile.HasActorPrefab)
            {
                InterviewArenaAuthoringLog.MissingReference(this, nameof(profile));
                throw new InvalidOperationException(
                    $"{name}: {nameof(HumanoidAnimationProfileSO)} with an actor prefab is required.");
            }

            Transform parent = visualAnchor != null ? visualAnchor : transform;
            if (crowdActor == null)
                crowdActor = parent.GetComponentInChildren<HumanoidCrowdActor>(true);

            if (crowdActor == null)
                throw new InvalidOperationException(
                    $"{name}: nested {nameof(HumanoidCrowdActor)} is required under VisualAnchor. Bake the visual in Interview Arena setup.");

            if (crowdActor.transform.parent == parent)
            {
                crowdActor.transform.localPosition = localPosition;
                crowdActor.transform.localRotation = Quaternion.Euler(localEulerAngles);
                crowdActor.transform.localScale = localScale;
            }

            crowdActor.Initialize(profile, 0f);
            animator = crowdActor.Animator;
            if (animator == null)
                throw new InvalidOperationException(
                    $"{name}: {nameof(HumanoidCrowdActor)} has no Animator.");

            DisableShootingPresentation();
            SyncMeleeWeaponVisible();
        }

        private void DisableShootingPresentation()
        {
            HumanoidUpperBodyAimIk aimIk = crowdActor.GetComponent<HumanoidUpperBodyAimIk>();
            if (aimIk != null)
                aimIk.enabled = false;

            SetBool(profile.AimStanceParameter, false);

            int upperLayerIndex = ResolveUpperBodyLayerIndex();
            if (upperLayerIndex >= 0)
                animator.SetLayerWeight(upperLayerIndex, 0f);
        }

        private int ResolveUpperBodyLayerIndex()
        {
            if (animator == null)
                return -1;

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerName(i).IndexOf("Upper", StringComparison.OrdinalIgnoreCase) >= 0)
                    return i;
            }

            return -1;
        }

        private void DisableCapsuleRenderer()
        {
            if (TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
                renderer.enabled = false;
        }

        private void ResolveMeleeWeaponRoot()
        {
            if (meleeWeaponRoot != null || crowdActor == null)
                return;

            meleeWeaponRoot = FindNamedChild(crowdActor.transform, "WeaponSocket_R", "Longsword");
        }

        private static Transform FindNamedChild(Transform root, params string[] names)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null)
                    continue;

                for (int n = 0; n < names.Length; n++)
                {
                    if (candidate.name == names[n])
                        return candidate;
                }
            }

            return null;
        }

        private void SyncMeleeWeaponVisible()
        {
            ResolveMeleeWeaponRoot();
            if (meleeWeaponRoot != null)
                meleeWeaponRoot.gameObject.SetActive(true);

            Transform bowRoot = FindNamedChild(crowdActor.transform, "WeaponSocket_L", "Bow");
            if (bowRoot != null)
                bowRoot.gameObject.SetActive(false);
        }

        private string ResolveHitTrigger(Vector3 hitTravelDirection)
        {
            HitReactDirection direction = HitReactDirectionRules.Resolve(
                LogicalBodyForward,
                hitTravelDirection);

            string trigger = direction switch
            {
                HitReactDirection.Front => profile.HitFrontTriggerParameter,
                HitReactDirection.Back => profile.HitBackTriggerParameter,
                HitReactDirection.Left => profile.HitLeftTriggerParameter,
                HitReactDirection.Right => profile.HitRightTriggerParameter,
                _ => profile.HitTriggerParameter
            };

            return string.IsNullOrEmpty(trigger) ? profile.HitTriggerParameter : trigger;
        }

        private bool IsActionAnimActive()
        {
            if (playerMotor != null &&
                (playerMotor.IsJumpAnimActive || playerMotor.IsDashAnimActive))
                return true;

            if (actionCoordinator != null && !actionCoordinator.CanMove)
                return true;

            if (enemyMelee != null && enemyMelee.IsBusy)
                return true;

            if (enemyCrossbow != null && enemyCrossbow.IsShootingAnimActive)
                return true;

            return false;
        }

        private void ApplyAnimatorParameters(float moveX, float moveY, bool actionActive, float deltaTime)
        {
            if (animator == null || profile == null)
                return;

            float dampTime = profile.ParameterDampTime;
            SetFloat(profile.MoveXParameter, moveX, dampTime, deltaTime);
            SetFloat(profile.MoveYParameter, moveY, dampTime, deltaTime);

            bool turnActive = BodyFacingRules.ShouldPlayTurnClip(isTurningInPlace, actionActive);
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

        private void PulseTriggerOnVersionChange(ref int lastVersion, int version, string trigger)
        {
            if (version == lastVersion)
                return;

            lastVersion = version;
            if (version <= 0)
                return;

            if (string.IsNullOrEmpty(trigger))
                trigger = profile.MeleeTriggerParameter;

            PulseTrigger(trigger);
        }

        private void PulseTrigger(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                return;

            animator.ResetTrigger(parameterName);
            animator.SetTrigger(parameterName);
        }

        private void OnDrawGizmos()
        {
            if (!drawFacingGizmos)
                return;

            EnsureGizmoReferences();

            Vector3 origin = transform.position + Vector3.up * facingGizmoHeight;
            float length = Mathf.Max(0.25f, facingGizmoLength);

            Vector3 bodyForward = bodyRotationInitialized
                ? LogicalBodyForward
                : FlattenDirection(transform.forward);
            DrawDirectionGizmo(origin, bodyForward, new Color(0.15f, 0.95f, 0.35f), length);

            Vector3 aim = FlattenDirection(ResolveAimDirection());
            DrawDirectionGizmo(origin + Vector3.up * 0.05f, aim, new Color(0.2f, 0.75f, 1f), length);

            DrawDirectionGizmo(origin + Vector3.up * 0.1f, FlattenDirection(transform.forward), new Color(0.85f, 0.85f, 0.85f, 0.7f), length * 0.7f);

            Transform mesh = ResolveVisualMeshTransform();
            if (mesh != null)
                DrawDirectionGizmo(origin + Vector3.up * 0.15f, FlattenDirection(mesh.forward), new Color(1f, 0.35f, 0.85f), length * 0.85f);

            if (body != null)
            {
                Vector3 velocity = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
                if (velocity.sqrMagnitude > 0.01f)
                    DrawDirectionGizmo(origin + Vector3.up * 0.2f, velocity.normalized, new Color(1f, 0.85f, 0.15f), Mathf.Min(length, velocity.magnitude * 0.35f + 0.4f));
            }

            float limitDegrees = PlanarAimLimitDegrees;
            if (limitDegrees > 0.1f && bodyForward.sqrMagnitude > 0.001f)
            {
                Vector3 left = Quaternion.AngleAxis(-limitDegrees, Vector3.up) * bodyForward;
                Vector3 right = Quaternion.AngleAxis(limitDegrees, Vector3.up) * bodyForward;
                Gizmos.color = new Color(0.15f, 0.95f, 0.35f, 0.35f);
                Gizmos.DrawLine(origin, origin + left * length * 0.55f);
                Gizmos.DrawLine(origin, origin + right * length * 0.55f);
            }
        }

        private void EnsureGizmoReferences()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();
            if (cursorAim == null)
                cursorAim = GetComponent<ArenaCursorAim>();
            if (crowdActor == null && visualAnchor != null)
                crowdActor = visualAnchor.GetComponentInChildren<HumanoidCrowdActor>(true);
            if (crowdActor == null)
                crowdActor = GetComponentInChildren<HumanoidCrowdActor>(true);
        }

        private Transform ResolveVisualMeshTransform()
        {
            if (crowdActor != null)
                return crowdActor.transform;
            if (visualAnchor != null)
                return visualAnchor;
            return null;
        }

        private static Vector3 FlattenDirection(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private static void DrawDirectionGizmo(Vector3 origin, Vector3 direction, Color color, float length)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Vector3 tip = origin + direction.normalized * length;
            Gizmos.color = color;
            Gizmos.DrawLine(origin, tip);
            Gizmos.DrawSphere(tip, 0.04f);
        }
    }
}
