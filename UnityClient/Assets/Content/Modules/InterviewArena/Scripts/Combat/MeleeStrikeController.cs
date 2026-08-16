using LearningArchitect.Modules.Animation3D;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DefaultExecutionOrder(-25)]
    [DisallowMultipleComponent]
    public sealed class MeleeStrikeController : MonoBehaviour
    {
        [SerializeField] private MeleeWeaponConfig config;
        [SerializeField] private Transform strikeOrigin;
        [SerializeField] private CombatTeam sourceTeam = CombatTeam.Player;

        private readonly PhysicsQueryService queryService = new PhysicsQueryService();
        private readonly MeleeComboState comboState = new MeleeComboState();
        private Rigidbody body;
        private PlayerBuffController buffController;
        private IBodyFacingProvider bodyFacing;
        private IBodyFacingCommit bodyFacingCommit;
        private ArenaCursorAim cursorAim;
        private PlayerActionCoordinator actionCoordinator;
        private MeleeStrikeDefinition activeStrike;
        private int activeStrikeIndex = -1;
        private float strikeElapsed;
        private bool strikeActive;
        private bool hitWindowResolved;
        private bool swingFeedbackPlayed;
        private float lastMeleeYawSign = 1f;
        private int meleeAnimStartVersion;
        private Vector3 strikeStepDirection;

        /// <summary>Gameplay: a strike cycle is in progress (includes recovery).</summary>
        public bool IsStrikeActive => strikeActive;

        /// <summary>Presentation alias for active strike (Animator / weapon visibility).</summary>
        public bool IsMeleeAnimActive => strikeActive;

        public MeleeStrikeDefinition ActiveStrike => activeStrike;
        public int ActiveStrikeIndex => activeStrikeIndex;
        public string ActiveMeleeAnimTrigger =>
            activeStrike != null ? activeStrike.AnimTriggerOverride : string.Empty;

        /// <summary>
        /// Increments on each successful strike start (including combo transitions). Visuals watch
        /// this so a same-frame end→restart still fires the Animator trigger.
        /// </summary>
        public int MeleeAnimStartVersion => meleeAnimStartVersion;

        /// <summary>
        /// True while the authored planar step window is open. Hover / motor must not zero XZ.
        /// </summary>
        public bool IsStrikeStepActive =>
            strikeActive
            && MeleeStrikeStep.TryResolvePlanarVelocity(
                activeStrike,
                strikeElapsed,
                strikeStepDirection,
                out _);

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            buffController = GetComponent<PlayerBuffController>();
            bodyFacing = GetComponent<IBodyFacingProvider>();
            bodyFacingCommit = GetComponent<IBodyFacingCommit>();
            cursorAim = GetComponent<ArenaCursorAim>();
            actionCoordinator = GetComponent<PlayerActionCoordinator>();
        }

        private void OnDisable()
        {
            if (strikeActive)
                InterruptStrike();
            else
                comboState.Clear();
        }

        private PlayerActionCoordinator ResolveActionCoordinator()
        {
            if (actionCoordinator == null)
                actionCoordinator = GetComponent<PlayerActionCoordinator>();
            return actionCoordinator;
        }

        public void ApplyConfig(MeleeWeaponConfig weaponConfig, Transform origin, CombatTeam team)
        {
            config = weaponConfig;
            strikeOrigin = origin;
            sourceTeam = team;
        }

        /// <summary>
        /// Single melee input entry: start Strike 0 when idle, or try to queue combo while active.
        /// </summary>
        public AttackInputResult HandleMeleeAttackInput()
        {
            if (config == null)
                return AttackInputResult.Rejected;

            if (strikeActive)
            {
                if (comboState.TryQueue(
                        activeStrike,
                        strikeElapsed,
                        config.StrikeCount,
                        out _))
                    return AttackInputResult.QueuedNextStrike;

                return AttackInputResult.Rejected;
            }

            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate != null && !gate.CanAttack)
                return AttackInputResult.Rejected;

            return TryBeginStrike(0) ? AttackInputResult.StartedStrike : AttackInputResult.Rejected;
        }

        /// <summary>Legacy entry used by tests; starts a strike by index when idle.</summary>
        public bool TryStrike(int strikeIndex = 0)
        {
            if (strikeActive)
                return false;

            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate != null && !gate.CanAttack)
                return false;

            return TryBeginStrike(strikeIndex);
        }

        /// <summary>
        /// Cancels an in-progress strike (hitstun / hard interrupt). Clears combo queue.
        /// </summary>
        public void InterruptStrike()
        {
            comboState.Clear();

            if (!strikeActive)
            {
                activeStrike = null;
                activeStrikeIndex = -1;
                strikeStepDirection = Vector3.zero;
                return;
            }

            strikeActive = false;
            strikeElapsed = 0f;
            hitWindowResolved = false;
            swingFeedbackPlayed = false;
            activeStrike = null;
            activeStrikeIndex = -1;
            strikeStepDirection = Vector3.zero;
            ClearStrikeLock();
        }

        /// <summary>
        /// Advances an active strike. Used by Update and Edit Mode tests.
        /// </summary>
        public void TickActiveStrike(float deltaTime)
        {
            if (!strikeActive)
                return;

            if (config == null || activeStrike == null)
            {
                InterruptStrike();
                return;
            }

            if (deltaTime < 0f)
                deltaTime = 0f;

            strikeElapsed += deltaTime;

            if (comboState.ShouldTransition(activeStrike, strikeElapsed))
            {
                TransitionToQueuedStrike();
                return;
            }

            float duration = activeStrike.StrikeDuration;
            float hitStart = activeStrike.HitWindowStartNormalized * duration;
            float hitEnd = activeStrike.HitWindowEndNormalized * duration;

            PublishStrikeLock();

            if (!swingFeedbackPlayed && strikeElapsed >= hitStart)
            {
                swingFeedbackPlayed = true;
                Transform origin = ResolveStrikeOrigin();
                CombatHitFeedback.PlayMeleeSwing(origin, activeStrike.StrikeRadius);
            }

            if (!hitWindowResolved && strikeElapsed >= hitStart && strikeElapsed <= hitEnd)
                ResolveHitWindow();

            if (strikeElapsed >= duration)
                FinishStrikeToIdle();
        }

        private bool TryBeginStrike(int strikeIndex)
        {
            if (config == null)
                return false;

            if (!config.TryGetStrike(strikeIndex, out MeleeStrikeDefinition strike))
                return false;

            BeginStrike(strikeIndex, strike);
            return true;
        }

        private void BeginStrike(int strikeIndex, MeleeStrikeDefinition strike)
        {
            comboState.Clear();
            activeStrikeIndex = strikeIndex;
            activeStrike = strike;
            strikeActive = true;
            strikeElapsed = 0f;
            hitWindowResolved = false;
            swingFeedbackPlayed = false;
            meleeAnimStartVersion++;
            CommitAttackFacing();
            CaptureStrikeStepDirection();
            ZeroPlanarVelocity();
            PublishStrikeLock();
        }

        private void FinishStrikeToIdle()
        {
            comboState.Clear();
            strikeActive = false;
            strikeElapsed = 0f;
            hitWindowResolved = false;
            swingFeedbackPlayed = false;
            activeStrike = null;
            activeStrikeIndex = -1;
            strikeStepDirection = Vector3.zero;
            ClearStrikeLock();
        }

        private void TransitionToQueuedStrike()
        {
            if (!comboState.TryConsumeQueue(out int nextIndex))
                return;

            if (config == null || !config.TryGetStrike(nextIndex, out MeleeStrikeDefinition nextStrike))
            {
                FinishStrikeToIdle();
                return;
            }

            // Close old hit registration before swapping definition.
            hitWindowResolved = true;
            swingFeedbackPlayed = true;

            activeStrikeIndex = nextIndex;
            activeStrike = nextStrike;
            strikeActive = true;
            strikeElapsed = 0f;
            hitWindowResolved = false;
            swingFeedbackPlayed = false;
            meleeAnimStartVersion++;
            CommitAttackFacing();
            CaptureStrikeStepDirection();
            ZeroPlanarVelocity();
            // SetLock replaces MeleeStrike kind — no ClearLock gap / Idle frame.
            PublishStrikeLock();
        }

        private void CommitAttackFacing()
        {
            Vector3 aimDirection = ResolveAimDirection();
            if (bodyFacingCommit != null)
                bodyFacingCommit.SnapPlanarFacing(aimDirection);
        }

        private void CaptureStrikeStepDirection()
        {
            Vector3 direction = ResolveStrikeDirection();
            direction.y = 0f;
            strikeStepDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector3.zero;
        }

        private Rigidbody ResolveBody()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();
            return body;
        }

        private void ZeroPlanarVelocity()
        {
            Rigidbody rb = ResolveBody();
            if (rb == null)
                return;

            Vector3 velocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0f, velocity.y, 0f);
        }

        private void Update()
        {
            TickActiveStrike(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            TickStrikeStep();
        }

        /// <summary>
        /// Writes authored step velocity. Tests call this because Edit Mode does not run FixedUpdate.
        /// </summary>
        public void TickStrikeStep()
        {
            Rigidbody rb = ResolveBody();
            if (!strikeActive || rb == null)
                return;

            if (!MeleeStrikeStep.TryResolvePlanarVelocity(
                    activeStrike,
                    strikeElapsed,
                    strikeStepDirection,
                    out Vector3 planar))
                return;

            Vector3 velocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(planar.x, velocity.y, planar.z);
        }

        private void PublishStrikeLock()
        {
            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate == null || activeStrike == null)
                return;

            float remaining = activeStrike.StrikeDuration - strikeElapsed;
            bool inStartup = strikeElapsed < activeStrike.MovementLockDuration;
            gate.SetLock(CharacterActionLock.MeleeStrike(remaining, inStartup));
        }

        private void ClearStrikeLock()
        {
            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate != null)
                gate.ClearLock(CharacterActionKind.MeleeStrike);
        }

        private void ResolveHitWindow()
        {
            hitWindowResolved = true;
            Transform origin = ResolveStrikeOrigin();
            Vector3 strikeDirection = ResolveStrikeDirection();

            float damage = activeStrike.Damage;
            if (buffController != null)
                damage *= buffController.GetMultiplier(BuffKind.MeleeDamage);

            int damaged = CombatStrikeUtility.TryMeleeOverlapStrike(
                queryService,
                origin,
                strikeDirection,
                activeStrike.ForwardOffset,
                activeStrike.StrikeRadius,
                config.HitMask,
                sourceTeam,
                damage,
                activeStrike.KnockbackImpulse,
                gameObject.GetInstanceID(),
                out _,
                out Vector3 primaryHitPoint,
                out bool hasPrimaryHitPoint);

            if (damaged > 0 && hasPrimaryHitPoint)
                CombatHitFeedback.PlayMeleeHitConfirm(primaryHitPoint);
        }

        private Transform ResolveStrikeOrigin() =>
            strikeOrigin != null ? strikeOrigin : transform;

        private Vector3 ResolveAimDirection()
        {
            if (cursorAim != null && cursorAim.AimDirection.sqrMagnitude > 0.0001f)
                return cursorAim.AimDirection;

            Transform origin = ResolveStrikeOrigin();
            Vector3 forward = origin.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private Vector3 ResolveStrikeDirection()
        {
            Vector3 aimDirection = ResolveAimDirection();
            if (bodyFacing == null)
                return aimDirection;

            Vector3 bodyForward = bodyFacing.LogicalBodyForward;
            float maxDegrees = bodyFacing.PlanarAimLimitDegrees;
            Vector3 clamped = HumanoidPlanarAimMath.ClampAimDirection(
                bodyForward,
                aimDirection,
                maxDegrees,
                lastMeleeYawSign,
                out float rawYaw,
                out _);

            if (Mathf.Abs(rawYaw) > 1f)
                lastMeleeYawSign = Mathf.Sign(rawYaw);

            return clamped;
        }
    }
}
