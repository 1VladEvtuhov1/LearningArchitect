using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Hold-to-block: frontal cone absorbs hits. Animator is presentation-only.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class MeleeBlockController : MonoBehaviour, IIncomingHitGuard
    {
        [SerializeField] private PlayerConfig config;

        private PlayerInputReader input;
        private PlayerActionCoordinator actionCoordinator;
        private PlayerMotor playerMotor;
        private MeleeStrikeController meleeStrike;
        private CrossbowWeaponController crossbow;
        private PlayerCombatStance stance;
        private IBodyFacingProvider bodyFacing;
        private IBodyFacingCommit bodyFacingCommit;
        private Health health;
        private bool blocking;
        private int blockStartVersion;

        public bool IsBlocking => blocking;
        public int BlockStartVersion => blockStartVersion;

        private void Awake() => EnsureBindings();

        private void OnEnable() => BindHealth();

        private void OnDisable()
        {
            if (health != null)
                health.Damaged -= HandleDamaged;
            Stop();
        }

        private void EnsureBindings()
        {
            if (input == null)
                input = GetComponent<PlayerInputReader>();
            if (actionCoordinator == null)
                actionCoordinator = GetComponent<PlayerActionCoordinator>();
            if (playerMotor == null)
                playerMotor = GetComponent<PlayerMotor>();
            if (meleeStrike == null)
                meleeStrike = GetComponent<MeleeStrikeController>();
            if (crossbow == null)
                crossbow = GetComponent<CrossbowWeaponController>();
            if (stance == null)
                stance = GetComponent<PlayerCombatStance>();
            if (bodyFacing == null)
                bodyFacing = GetComponent<IBodyFacingProvider>();
            if (bodyFacingCommit == null)
                bodyFacingCommit = GetComponent<IBodyFacingCommit>();
            BindHealth();
        }

        private void BindHealth()
        {
            if (health == null)
                health = GetComponent<Health>();
            if (health == null)
                return;

            health.Damaged -= HandleDamaged;
            if (isActiveAndEnabled)
                health.Damaged += HandleDamaged;
        }

        public void ApplyConfig(PlayerConfig playerConfig) => config = playerConfig;

        public void Stop()
        {
            if (!blocking && (actionCoordinator == null || !actionCoordinator.HasLock(CharacterActionKind.Block)))
            {
                blocking = false;
                return;
            }

            blocking = false;
            if (actionCoordinator != null)
                actionCoordinator.ClearLock(CharacterActionKind.Block);
        }

        private void Update()
        {
            TickBlock();
        }

        /// <summary>Edit Mode tests drive this instead of Update.</summary>
        public void TickBlock() => TickBlock(input != null && input.BlockHeld);

        public void TickBlock(bool wantBlock)
        {
            EnsureBindings();
            bool strikeActive = meleeStrike != null && meleeStrike.IsStrikeActive;
            bool denied = actionCoordinator != null
                && (actionCoordinator.HasLock(CharacterActionKind.Hitstun)
                    || actionCoordinator.HasLock(CharacterActionKind.Dash)
                    || actionCoordinator.HasLock(CharacterActionKind.Jump));
            if (playerMotor != null && (playerMotor.IsDashing || playerMotor.IsJumpAnimActive))
                denied = true;

            // LMB cancels guard into melee; do not re-enter until the strike ends.
            if (!wantBlock || denied || strikeActive || (health != null && !health.IsAlive))
            {
                Stop();
                return;
            }

            BeginOrRefresh();
        }

        public bool TryAbsorb(in DamageInfo incoming, out DamageInfo absorbed)
        {
            EnsureBindings();
            absorbed = incoming;
            if (!blocking)
                return false;

            Vector3 bodyForward = bodyFacing != null
                ? bodyFacing.LogicalBodyForward
                : transform.forward;
            float cone = config != null
                ? config.BlockConeHalfAngleDegrees
                : MeleeBlockRules.DefaultConeHalfAngleDegrees;

            if (!MeleeBlockRules.IsFrontalBlock(bodyForward, incoming.HitDirection, cone))
                return false;

            absorbed = new DamageInfo(
                0f,
                incoming.SourceTeam,
                incoming.HitPoint,
                incoming.HitDirection,
                0f,
                wasBlocked: true);
            return true;
        }

        private void BeginOrRefresh()
        {
            if (crossbow != null && crossbow.IsShotActive)
                crossbow.InterruptShot();
            if (stance != null)
                stance.SetStance(ArenaCombatStance.MeleeReady);

            if (!blocking)
            {
                blocking = true;
                blockStartVersion++;
                if (bodyFacingCommit != null && bodyFacing != null)
                    bodyFacingCommit.SnapPlanarFacing(bodyFacing.LogicalBodyForward);
            }

            if (actionCoordinator != null)
                actionCoordinator.SetLock(CharacterActionLock.Block());
        }

        private void HandleDamaged(Health _, DamageInfo __) => Stop();
    }
}
