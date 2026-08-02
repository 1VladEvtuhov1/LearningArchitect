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
        private Rigidbody body;
        private PlayerBuffController buffController;
        private IBodyFacingProvider bodyFacing;
        private IBodyFacingCommit bodyFacingCommit;
        private ArenaCursorAim cursorAim;
        private PlayerActionCoordinator actionCoordinator;
        private float strikeElapsed;
        private bool strikeActive;
        private bool hitWindowResolved;
        private bool swingFeedbackPlayed;
        private float lastMeleeYawSign = 1f;
        private int meleeAnimStartVersion;

        public bool IsMeleeAnimActive => strikeActive;

        /// <summary>
        /// Increments on each successful <see cref="TryStrike"/>. Visuals watch this so a
        /// same-frame end→restart (input buffer) still fires the Animator Melee trigger.
        /// </summary>
        public int MeleeAnimStartVersion => meleeAnimStartVersion;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            buffController = GetComponent<PlayerBuffController>();
            bodyFacing = GetComponent<IBodyFacingProvider>();
            bodyFacingCommit = GetComponent<IBodyFacingCommit>();
            cursorAim = GetComponent<ArenaCursorAim>();
            actionCoordinator = GetComponent<PlayerActionCoordinator>();
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

        public bool TryStrike()
        {
            if (config == null || strikeActive)
                return false;

            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate != null && !gate.CanAttack)
                return false;

            strikeActive = true;
            strikeElapsed = 0f;
            hitWindowResolved = false;
            swingFeedbackPlayed = false;
            meleeAnimStartVersion++;
            CommitAttackFacing();
            ZeroPlanarVelocity();
            PublishStrikeLock();
            return true;
        }

        /// <summary>
        /// Cancels an in-progress strike (hitstun / hard interrupt). No hit window after this.
        /// </summary>
        public void InterruptStrike()
        {
            if (!strikeActive)
                return;

            strikeActive = false;
            strikeElapsed = 0f;
            hitWindowResolved = false;
            swingFeedbackPlayed = false;
            ClearStrikeLock();
        }

        private void CommitAttackFacing()
        {
            Vector3 aimDirection = ResolveAimDirection();
            if (bodyFacingCommit != null)
                bodyFacingCommit.SnapPlanarFacing(aimDirection);
        }

        private void ZeroPlanarVelocity()
        {
            if (body == null)
                return;

            Vector3 velocity = body.linearVelocity;
            body.linearVelocity = new Vector3(0f, velocity.y, 0f);
        }

        private void Update()
        {
            if (!strikeActive)
                return;

            if (config == null)
            {
                InterruptStrike();
                return;
            }

            strikeElapsed += Time.deltaTime;
            float duration = config.StrikeDuration;
            float hitStart = config.HitWindowStartNormalized * duration;
            float hitEnd = config.HitWindowEndNormalized * duration;

            PublishStrikeLock();

            if (!swingFeedbackPlayed && strikeElapsed >= hitStart)
            {
                swingFeedbackPlayed = true;
                Transform origin = ResolveStrikeOrigin();
                CombatHitFeedback.PlayMeleeSwing(origin, config.StrikeRadius);
                ApplyStrikeLunge(ResolveStrikeDirection());
            }

            if (!hitWindowResolved && strikeElapsed >= hitStart && strikeElapsed <= hitEnd)
                ResolveHitWindow();

            if (strikeElapsed >= duration)
                InterruptStrike();
        }

        private void PublishStrikeLock()
        {
            PlayerActionCoordinator gate = ResolveActionCoordinator();
            if (gate == null || config == null)
                return;

            float remaining = config.StrikeDuration - strikeElapsed;
            bool inStartup = strikeElapsed < config.MovementLockDuration;
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

            float damage = config.Damage;
            if (buffController != null)
                damage *= buffController.GetMultiplier(BuffKind.MeleeDamage);

            int damaged = CombatStrikeUtility.TryMeleeOverlapStrike(
                queryService,
                origin,
                strikeDirection,
                config.ForwardOffset,
                config.StrikeRadius,
                config.HitMask,
                sourceTeam,
                damage,
                config.KnockbackImpulse,
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

        private void ApplyStrikeLunge(Vector3 strikeDirection)
        {
            if (body == null || config.StrikeLungeImpulse <= 0f)
                return;

            strikeDirection.y = 0f;
            if (strikeDirection.sqrMagnitude < 0.0001f)
                return;

            body.AddForce(strikeDirection.normalized * config.StrikeLungeImpulse, ForceMode.Impulse);
        }
    }
}
