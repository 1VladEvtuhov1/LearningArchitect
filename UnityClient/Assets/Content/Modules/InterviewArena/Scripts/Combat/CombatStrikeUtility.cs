using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    public static class CombatStrikeUtility
    {
        public static int TryMeleeOverlapStrike(
            PhysicsQueryService queryService,
            Transform strikeOrigin,
            float forwardOffset,
            float strikeRadius,
            LayerMask hitMask,
            CombatTeam sourceTeam,
            float damage,
            float knockbackImpulse,
            int excludeInstanceId,
            out int overlapCount)
        {
            return TryMeleeOverlapStrike(
                queryService,
                strikeOrigin,
                forwardOffset,
                strikeRadius,
                hitMask,
                sourceTeam,
                damage,
                knockbackImpulse,
                excludeInstanceId,
                out overlapCount,
                out _,
                out _);
        }

        public static int TryMeleeOverlapStrike(
            PhysicsQueryService queryService,
            Transform strikeOrigin,
            float forwardOffset,
            float strikeRadius,
            LayerMask hitMask,
            CombatTeam sourceTeam,
            float damage,
            float knockbackImpulse,
            int excludeInstanceId,
            out int overlapCount,
            out Vector3 primaryHitPoint,
            out bool hasPrimaryHitPoint)
        {
            Vector3 strikeDirection = strikeOrigin != null ? strikeOrigin.forward : Vector3.forward;
            return TryMeleeOverlapStrike(
                queryService,
                strikeOrigin,
                strikeDirection,
                forwardOffset,
                strikeRadius,
                hitMask,
                sourceTeam,
                damage,
                knockbackImpulse,
                excludeInstanceId,
                out overlapCount,
                out primaryHitPoint,
                out hasPrimaryHitPoint);
        }

        public static int TryMeleeOverlapStrike(
            PhysicsQueryService queryService,
            Transform strikeOrigin,
            Vector3 strikeDirection,
            float forwardOffset,
            float strikeRadius,
            LayerMask hitMask,
            CombatTeam sourceTeam,
            float damage,
            float knockbackImpulse,
            int excludeInstanceId,
            out int overlapCount,
            out Vector3 primaryHitPoint,
            out bool hasPrimaryHitPoint)
        {
            overlapCount = 0;
            primaryHitPoint = default;
            hasPrimaryHitPoint = false;

            if (queryService == null || strikeOrigin == null)
                return 0;

            strikeDirection.y = 0f;
            if (strikeDirection.sqrMagnitude < 0.0001f)
                strikeDirection = strikeOrigin.forward;

            strikeDirection.Normalize();

            Vector3 center = strikeOrigin.position + strikeDirection * forwardOffset;
            overlapCount = queryService.OverlapSphere(center, strikeRadius, hitMask);
            if (overlapCount <= 0)
                return 0;

            DamageInfo template = new DamageInfo(
                damage,
                sourceTeam,
                center,
                strikeDirection,
                knockbackImpulse);

            return CombatHitResolver.ApplyStrikeHits(
                queryService,
                overlapCount,
                template,
                excludeInstanceId,
                out primaryHitPoint,
                out hasPrimaryHitPoint);
        }
    }
}
