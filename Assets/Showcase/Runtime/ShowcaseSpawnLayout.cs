using UnityEngine;

namespace LearningArchitect.Core
{
    public static class ShowcaseSpawnLayout
    {
        public static Vector3 RandomPointOnPlatform(float radius, float heightFactor = 0.35f)
        {
            Vector3 point = Random.insideUnitSphere * radius;
            point.y = Mathf.Abs(point.y) * heightFactor;
            return point;
        }

        public static Vector3 RandomVelocity(float speed, float verticalFactor = 0.2f)
        {
            Vector3 velocity = Random.onUnitSphere * speed;
            velocity.y *= verticalFactor;
            return velocity;
        }
    }
}
