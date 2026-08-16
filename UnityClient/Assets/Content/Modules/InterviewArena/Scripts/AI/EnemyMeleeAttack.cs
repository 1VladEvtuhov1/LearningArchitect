using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class EnemyMeleeAttack : MonoBehaviour
    {
        [SerializeField] private Transform strikeOrigin;
        [SerializeField] private CombatTeam sourceTeam = CombatTeam.Enemy;
        [SerializeField] private string animTrigger = "ComboRight1";

        private readonly PhysicsQueryService queryService = new PhysicsQueryService();
        private float cooldownTimer;
        private float windupTimer;
        private bool windupActive;
        private int meleeAnimStartVersion;

        public Transform StrikeOrigin => strikeOrigin != null ? strikeOrigin : transform;
        public bool IsBusy => windupActive || cooldownTimer > 0f;
        public int MeleeAnimStartVersion => meleeAnimStartVersion;
        public string ActiveMeleeAnimTrigger => animTrigger ?? string.Empty;

        public bool TryBeginAttack(EnemyConfig config)
        {
            if (config == null || IsBusy)
                return false;

            windupActive = true;
            windupTimer = config.AttackWindup;
            meleeAnimStartVersion++;
            if (TryGetComponent(out IBodyFacingCommit facing))
                facing.SnapPlanarFacing(StrikeOrigin.forward);
            CombatHitFeedback.PlayMeleeWindup(StrikeOrigin, config.StrikeRadius);
            return true;
        }

        public void Interrupt()
        {
            windupActive = false;
            windupTimer = 0f;
        }

        public void Tick(EnemyConfig config, int excludeInstanceId) =>
            Tick(config, excludeInstanceId, Time.deltaTime);

        public void Tick(EnemyConfig config, int excludeInstanceId, float deltaTime)
        {
            if (!windupActive || config == null)
                return;

            if (deltaTime < 0f)
                deltaTime = 0f;

            windupTimer -= deltaTime;
            if (windupTimer > 0f)
                return;

            windupActive = false;
            cooldownTimer = config.AttackCooldown;

            Transform origin = StrikeOrigin;
            int damaged = CombatStrikeUtility.TryMeleeOverlapStrike(
                queryService,
                origin,
                config.StrikeForwardOffset,
                config.StrikeRadius,
                config.HitMask,
                sourceTeam,
                config.AttackDamage,
                config.AttackKnockback,
                excludeInstanceId,
                out _);

            CombatHitFeedback.PlayMeleeSwing(origin, config.StrikeRadius);
            if (damaged > 0)
                CombatHitFeedback.PlayMeleeHitConfirm(origin.position + origin.forward * config.StrikeForwardOffset);
        }

        private void Update()
        {
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
        }
    }
}
