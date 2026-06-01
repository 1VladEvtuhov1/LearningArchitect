using System;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    public static class CombatHitResolver
    {
        public static bool TryResolveDamageable(Collider collider, out IDamageable damageable)
        {
            damageable = null;
            if (collider == null)
                return false;

            damageable = collider.GetComponentInParent<IDamageable>();
            return damageable != null && damageable.IsAlive;
        }

        public static int ApplyStrikeHits(
            PhysicsQueryService queryService,
            int overlapCount,
            in DamageInfo damageTemplate,
            int excludeInstanceId)
        {
            Span<int> damagedTargets = stackalloc int[OverlapCapacity];
            int damagedCount = 0;

            for (int i = 0; i < overlapCount; i++)
            {
                Collider collider = queryService.GetOverlapCollider(i);
                if (collider == null || collider.gameObject.GetInstanceID() == excludeInstanceId)
                    continue;

                if (!TryResolveDamageable(collider, out IDamageable damageable))
                    continue;

                if (damageable is not Component component)
                    continue;

                int targetId = component.gameObject.GetInstanceID();
                if (ContainsInstanceId(damagedTargets, damagedCount, targetId))
                    continue;

                damagedTargets[damagedCount++] = targetId;

                Vector3 hitPoint = collider.ClosestPoint(damageTemplate.HitPoint);
                DamageInfo info = new DamageInfo(
                    damageTemplate.Amount,
                    damageTemplate.SourceTeam,
                    hitPoint,
                    damageTemplate.HitDirection,
                    damageTemplate.KnockbackImpulse);
                damageable.ApplyDamage(in info);
            }

            return damagedCount;
        }

        private const int OverlapCapacity = 16;

        private static bool ContainsInstanceId(ReadOnlySpan<int> ids, int count, int instanceId)
        {
            for (int i = 0; i < count; i++)
            {
                if (ids[i] == instanceId)
                    return true;
            }

            return false;
        }
    }
}
