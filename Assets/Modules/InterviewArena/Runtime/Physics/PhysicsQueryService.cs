using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Zero-allocation physics queries for grounded checks and future combat traces.
    /// </summary>
    public sealed class PhysicsQueryService
    {
        private const int HitBufferSize = 8;
        private const int OverlapBufferSize = 16;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];
        private readonly Collider[] overlapBuffer = new Collider[OverlapBufferSize];

        public bool TrySphereCastDown(
            Vector3 origin,
            float radius,
            float maxDistance,
            LayerMask mask,
            out RaycastHit hit,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                Vector3.down,
                hitBuffer,
                maxDistance,
                mask,
                triggerInteraction);

            if (hitCount <= 0)
            {
                hit = default;
                return false;
            }

            return TrySelectClosestHit(hitCount, ignoreInstanceId: -1, out hit);
        }

        public int OverlapSphere(Vector3 center, float radius, LayerMask mask)
        {
            return Physics.OverlapSphereNonAlloc(
                center,
                radius,
                overlapBuffer,
                mask,
                QueryTriggerInteraction.Ignore);
        }

        public Collider GetOverlapCollider(int index)
        {
            return overlapBuffer[index];
        }

        public bool TrySphereCastAlong(
            Vector3 direction,
            Vector3 origin,
            float radius,
            float distance,
            LayerMask mask,
            int ignoreInstanceId,
            out RaycastHit hit)
        {
            hit = default;
            if (distance <= 0.0001f || direction.sqrMagnitude < 0.0001f)
                return false;

            Vector3 castDirection = direction.normalized;
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                castDirection,
                hitBuffer,
                distance,
                mask,
                QueryTriggerInteraction.Ignore);

            return TrySelectClosestHit(hitCount, ignoreInstanceId, out hit);
        }

        private bool TrySelectClosestHit(int hitCount, int ignoreInstanceId, out RaycastHit hit)
        {
            hit = default;
            float closestDistance = float.MaxValue;
            bool found = false;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit candidate = hitBuffer[i];
                if (candidate.collider == null)
                    continue;

                if (ignoreInstanceId >= 0
                    && candidate.collider.gameObject.GetInstanceID() == ignoreInstanceId)
                    continue;

                if (candidate.distance >= closestDistance)
                    continue;

                closestDistance = candidate.distance;
                hit = candidate;
                found = true;
            }

            return found;
        }
    }
}
