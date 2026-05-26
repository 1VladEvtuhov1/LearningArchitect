using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    public static class CombatStrikeUtility
    {
        public static bool TryMeleeOverlapStrike(
            PhysicsQueryService queryService,
            Transform strikeOrigin,
            float forwardOffset,
            float strikeRadius,
            LayerMask hitMask,
            CombatTeam sourceTeam,
            float damage,
            float knockbackImpulse,
            int excludeInstanceId,
            out int hitCount)
        {
            hitCount = 0;
            if (queryService == null || strikeOrigin == null)
                return false;

            Vector3 center = strikeOrigin.position + strikeOrigin.forward * forwardOffset;
            hitCount = queryService.OverlapSphere(center, strikeRadius, hitMask);
            if (hitCount <= 0)
                return false;

            DamageInfo template = new DamageInfo(
                damage,
                sourceTeam,
                center,
                strikeOrigin.forward,
                knockbackImpulse);

            CombatHitResolver.ApplyStrikeHits(
                queryService,
                hitCount,
                template,
                excludeInstanceId);

            return true;
        }
    }
}
