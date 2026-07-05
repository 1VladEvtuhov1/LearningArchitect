using LearningArchitect.Modules.Animation3D;
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
        private IBodyFacingProvider bodyFacing;
        private ArenaCursorAim cursorAim;
        private float strikeElapsed;
        private bool strikeActive;
        private bool hitWindowResolved;
        private bool swingFeedbackPlayed;
        private float lastMeleeYawSign = 1f;

        public bool IsMeleeAnimActive => strikeActive;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            buffController = GetComponent<PlayerBuffController>();
            bodyFacing = GetComponent<IBodyFacingProvider>();
            cursorAim = GetComponent<ArenaCursorAim>();
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

            strikeActive = true;
            strikeElapsed = 0f;
            hitWindowResolved = false;
            swingFeedbackPlayed = false;
            return true;
        }

        private void Update()
        {
            if (!strikeActive || config == null)
                return;

            strikeElapsed += Time.deltaTime;
            float duration = config.StrikeDuration;
            float hitStart = config.HitWindowStartNormalized * duration;
            float hitEnd = config.HitWindowEndNormalized * duration;

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
                strikeActive = false;
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

        /// <summary>
        /// Melee hits within the same ±arc as upper-body aim (variant B), not full ViewPivot rotation.
        /// </summary>
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
