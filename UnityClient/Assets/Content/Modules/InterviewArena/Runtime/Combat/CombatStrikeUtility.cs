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
            overlapCount = 0;
            if (queryService == null || strikeOrigin == null)
                return 0;

            Vector3 center = strikeOrigin.position + strikeOrigin.forward * forwardOffset;
            overlapCount = queryService.OverlapSphere(center, strikeRadius, hitMask);
            if (overlapCount <= 0)
                return 0;

            DamageInfo template = new DamageInfo(
                damage,
                sourceTeam,
                center,
                strikeOrigin.forward,
                knockbackImpulse);

            return CombatHitResolver.ApplyStrikeHits(
                queryService,
                overlapCount,
                template,
                excludeInstanceId);
        }
    }
}
