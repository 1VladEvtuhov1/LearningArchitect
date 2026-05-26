using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Platform metrics for the Interview Arena cylinder (matches showcase PreviewPlatform scale).
    /// </summary>
    public static class InterviewArenaPlatformLayout
    {
        private const float DefaultCapsuleHalfHeight = 1f;
        private const float PlatformCylinderHalfHeight = 1f;
        private const float PlatformScaleY = 0.07f;
        private const float PlatformScaleXZ = 11.25f;
        private const float PlatformCylinderRadius = 0.5f;

        public static float PlatformTopY => PlatformCylinderHalfHeight * PlatformScaleY;
        public static float PlatformRadiusXZ => PlatformCylinderRadius * PlatformScaleXZ;
        public static float SpawnClearance => 0.08f;

        /// <summary>Legacy feet-level hint from default authored platform (not body center).</summary>
        public static float SurfaceY => PlatformTopY + SpawnClearance;

        public static float ResolvePlatformTopY(Collider platformCollider)
        {
            if (platformCollider != null)
                return platformCollider.bounds.max.y;

            return PlatformTopY;
        }

        public static float ResolveBodyCenterY(float capsuleHalfHeight, Collider platformCollider = null)
        {
            float halfHeight = Mathf.Max(0.01f, capsuleHalfHeight);
            return ResolvePlatformTopY(platformCollider) + halfHeight + SpawnClearance;
        }

        public static Vector3 ResolveSpawnPosition(Vector3 planarPosition, float capsuleHalfHeight, Collider platformCollider = null)
        {
            Vector3 spawn = planarPosition;
            spawn.y = ResolveBodyCenterY(capsuleHalfHeight, platformCollider);
            return spawn;
        }

        public static float ResolvePlanarRadius(Collider platformCollider, float edgeMargin = 0.35f)
        {
            if (platformCollider != null)
            {
                Bounds bounds = platformCollider.bounds;
                float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) - edgeMargin;
                return Mathf.Max(1f, radius);
            }

            return Mathf.Max(1f, PlatformRadiusXZ - edgeMargin);
        }

        public static Vector3 ClampToSurface(
            Vector3 position,
            float capsuleHalfHeight,
            Collider platformCollider = null,
            float edgeMargin = 0.35f)
        {
            float minCenterY = ResolveBodyCenterY(capsuleHalfHeight, platformCollider);
            position.y = Mathf.Max(position.y, minCenterY);

            Vector2 planar = new Vector2(position.x, position.z);
            float maxRadius = ResolvePlanarRadius(platformCollider, edgeMargin);
            if (planar.sqrMagnitude > maxRadius * maxRadius)
            {
                planar = planar.normalized * maxRadius;
                position.x = planar.x;
                position.z = planar.y;
            }

            return position;
        }

        public static float ResolveCapsuleHalfHeight(CapsuleCollider capsule, float fallbackHalfHeight = DefaultCapsuleHalfHeight)
        {
            if (capsule == null)
                return fallbackHalfHeight;

            return Mathf.Max(0.01f, capsule.height * 0.5f * capsule.transform.lossyScale.y);
        }
    }
}
