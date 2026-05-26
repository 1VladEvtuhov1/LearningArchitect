using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class MeleeStrikeController : MonoBehaviour
    {
        [SerializeField] private MeleeWeaponConfig config;
        [SerializeField] private Transform strikeOrigin;
        [SerializeField] private CombatTeam sourceTeam = CombatTeam.Player;

        private readonly PhysicsQueryService queryService = new PhysicsQueryService();
        private float cooldownTimer;

        public void ApplyConfig(MeleeWeaponConfig weaponConfig, Transform origin, CombatTeam team)
        {
            config = weaponConfig;
            strikeOrigin = origin;
            sourceTeam = team;
        }

        public bool TryStrike()
        {
            if (config == null || cooldownTimer > 0f)
                return false;

            Transform origin = strikeOrigin != null ? strikeOrigin : transform;
            cooldownTimer = config.Cooldown;

            CombatStrikeUtility.TryMeleeOverlapStrike(
                queryService,
                origin,
                config.ForwardOffset,
                config.StrikeRadius,
                config.HitMask,
                sourceTeam,
                config.Damage,
                config.KnockbackImpulse,
                gameObject.GetInstanceID(),
                out _);

            CombatHitFeedback.PlayMeleeSwing(origin, config.StrikeRadius);
            return true;
        }

        private void Update()
        {
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
        }
    }
}
