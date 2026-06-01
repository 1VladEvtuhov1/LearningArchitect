using UnityEngine;

namespace LearningArchitect.Core
{
    /// <summary>
    /// Spawn layout aligned with <c>PreviewPlatform</c> in <c>ArchitectureShowcase</c>
    /// (Unity cylinder height 2, radius 0.5, scale 11.25 x 0.035 x 11.25).
    /// </summary>
    public static class ShowcaseSpawnLayout
    {
        private const float PlatformCylinderHalfHeight = 1f;
        private const float PlatformScaleY = 0.035f;
        private const float PlatformScaleXZ = 11.25f;
        private const float PlatformCylinderRadius = 0.5f;

        public static float PlatformTopY => PlatformCylinderHalfHeight * PlatformScaleY;
        public static float PlatformRadiusXZ => PlatformCylinderRadius * PlatformScaleXZ;
        public static float SpawnClearance => 0.06f;
        public static float SurfaceY => PlatformTopY + SpawnClearance;

        public static Vector3 RandomPointOnPlatform(float radius, float heightJitter = 0f)
        {
            float planarRadius = Mathf.Min(Mathf.Abs(radius), PlatformRadiusXZ);
            Vector2 planar = Random.insideUnitCircle * planarRadius;
            float y = SurfaceY;

            if (heightJitter > 0f)
                y += Random.value * Mathf.Abs(heightJitter);

            return new Vector3(planar.x, y, planar.y);
        }

        public static Vector3 RandomVelocity(float speed, float verticalFactor = 0f)
        {
            Vector2 planar = Random.insideUnitCircle;
            if (planar.sqrMagnitude < 0.0001f)
                planar = Vector2.right;

            planar.Normalize();
            Vector3 velocity = new Vector3(planar.x, 0f, planar.y) * speed;
            velocity.y = Mathf.Clamp(velocity.y, -speed, speed) * Mathf.Abs(verticalFactor);
            return velocity;
        }

        public static Vector3 ClampToSurface(Vector3 position)
        {
            position.y = Mathf.Max(position.y, SurfaceY);
            return position;
        }

        public static Vector3 ClampPlanarToPlatform(Vector3 position, float radius)
        {
            float planarRadius = Mathf.Min(Mathf.Abs(radius), PlatformRadiusXZ);
            Vector2 planar = new Vector2(position.x, position.z);
            float planarRadiusSquared = planarRadius * planarRadius;

            if (planar.sqrMagnitude > planarRadiusSquared)
            {
                planar = planar.normalized * planarRadius;
                position.x = planar.x;
                position.z = planar.y;
            }

            return ClampToSurface(position);
        }
    }
}
