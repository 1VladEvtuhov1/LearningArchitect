using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class EnemyMeleeAttack : MonoBehaviour
    {
        [SerializeField] private Transform strikeOrigin;
        [SerializeField] private CombatTeam sourceTeam = CombatTeam.Enemy;

        private readonly PhysicsQueryService queryService = new PhysicsQueryService();
        private float cooldownTimer;
        private float windupTimer;
        private bool windupActive;

        public Transform StrikeOrigin => strikeOrigin != null ? strikeOrigin : transform;
        public bool IsBusy => windupActive || cooldownTimer > 0f;

        public bool TryBeginAttack(EnemyConfig config)
        {
            if (config == null || IsBusy)
                return false;

            windupActive = true;
            windupTimer = config.AttackWindup;
            return true;
        }

        public void Tick(EnemyConfig config, int excludeInstanceId)
        {
            if (!windupActive || config == null)
                return;

            windupTimer -= Time.deltaTime;
            if (windupTimer > 0f)
                return;

            windupActive = false;
            cooldownTimer = config.AttackCooldown;

            CombatStrikeUtility.TryMeleeOverlapStrike(
                queryService,
                StrikeOrigin,
                config.StrikeForwardOffset,
                config.StrikeRadius,
                config.HitMask,
                sourceTeam,
                config.AttackDamage,
                config.AttackKnockback,
                excludeInstanceId,
                out _);
        }

        private void Update()
        {
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
        }
    }
}
