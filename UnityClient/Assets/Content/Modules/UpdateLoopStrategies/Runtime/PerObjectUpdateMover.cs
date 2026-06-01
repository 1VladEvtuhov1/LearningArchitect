using System.Diagnostics;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.Modules.Performance
{
    public sealed class PerObjectUpdateMover : MonoBehaviour
    {
        [SerializeField] private float radius = 7f;
        [SerializeField] private float speed = 0.85f;

        private static int currentFrame = -1;
        private static long currentFrameTicks;
        private static long lastCompletedTicks;

        private Vector3 velocity;

        public void Configure(float radius, float speed)
        {
            this.radius = radius;
            this.speed = speed;
            velocity = ShowcaseSpawnLayout.RandomVelocity(speed);
        }

        private void Update()
        {
            AdvanceFrameIfNeeded();
            long startedAt = Stopwatch.GetTimestamp();
            float radiusSquared = radius * radius;
            Vector3 position = transform.localPosition + velocity * Time.deltaTime;
            Vector2 planar = new Vector2(position.x, position.z);

            if (planar.sqrMagnitude > radiusSquared)
            {
                velocity = -velocity;
                position = transform.localPosition + velocity * Time.deltaTime;
            }

            transform.localPosition = ShowcaseSpawnLayout.ClampToSurface(position);
            currentFrameTicks += Stopwatch.GetTimestamp() - startedAt;
        }

        public static void ResetMetrics()
        {
            currentFrame = -1;
            currentFrameTicks = 0L;
            lastCompletedTicks = 0L;
        }

        public static float GetLastModuleCpuMs()
        {
            return (float)(lastCompletedTicks * 1000d / Stopwatch.Frequency);
        }

        private static void AdvanceFrameIfNeeded()
        {
            int frame = Time.frameCount;
            if (currentFrame == frame)
                return;

            lastCompletedTicks = currentFrameTicks;
            currentFrame = frame;
            currentFrameTicks = 0L;
        }
    }
}
