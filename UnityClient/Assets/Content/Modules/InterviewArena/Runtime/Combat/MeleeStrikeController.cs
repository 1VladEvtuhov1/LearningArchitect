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
        private Rigidbody body;
        private PlayerBuffController buffController;
        private float cooldownTimer;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            buffController = GetComponent<PlayerBuffController>();
        }

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

            ApplyStrikeLunge(origin);

            float damage = config.Damage;
            if (buffController != null)
                damage *= buffController.GetMultiplier(BuffKind.MeleeDamage);

            int damaged = CombatStrikeUtility.TryMeleeOverlapStrike(
                queryService,
                origin,
                config.ForwardOffset,
                config.StrikeRadius,
                config.HitMask,
                sourceTeam,
                damage,
                config.KnockbackImpulse,
                gameObject.GetInstanceID(),
                out _);

            CombatHitFeedback.PlayMeleeSwing(origin, config.StrikeRadius);
            if (damaged > 0)
                CombatHitFeedback.PlayMeleeHitConfirm(origin.position + origin.forward * config.ForwardOffset);

            return true;
        }

        private void ApplyStrikeLunge(Transform origin)
        {
            if (body == null || config.StrikeLungeImpulse <= 0f)
                return;

            Vector3 forward = origin.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                return;

            forward.Normalize();
            body.AddForce(forward * config.StrikeLungeImpulse, ForceMode.Impulse);
        }

        private void Update()
        {
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
        }
    }
}
