using System.Diagnostics;
using UnityEngine;

namespace LearningArchitect.Modules.Performance
{
    public sealed class PerObjectUpdateMover : MonoBehaviour
    {
        [SerializeField] private float radius = 7f;
        [SerializeField] private float speed = 0.85f;

        private static int currentFrame = -1;
        private static long currentFrameTicks;
        private static int currentFrameOperations;
        private static long lastCompletedTicks;
        private static int lastCompletedOperations;

        private Vector3 velocity;

        public void Configure(float radius, float speed)
        {
            this.radius = radius;
            this.speed = speed;
            velocity = Random.onUnitSphere * speed;
            velocity.y *= 0.2f;
        }

        private void Update()
        {
            AdvanceFrameIfNeeded();
            long startedAt = Stopwatch.GetTimestamp();
            float radiusSquared = radius * radius;
            Vector3 position = transform.localPosition + velocity * Time.deltaTime;

            if (position.sqrMagnitude > radiusSquared)
            {
                velocity = -velocity;
                position = transform.localPosition + velocity * Time.deltaTime;
            }

            transform.localPosition = position;
            currentFrameOperations++;
            currentFrameTicks += Stopwatch.GetTimestamp() - startedAt;
        }

        public static void ResetMetrics()
        {
            currentFrame = -1;
            currentFrameTicks = 0L;
            currentFrameOperations = 0;
            lastCompletedTicks = 0L;
            lastCompletedOperations = 0;
        }

        public static float GetLastSimulationTimeMs()
        {
            return (float)(lastCompletedTicks * 1000d / Stopwatch.Frequency);
        }

        public static int GetLastOperationsPerFrame()
        {
            return lastCompletedOperations;
        }

        private static void AdvanceFrameIfNeeded()
        {
            int frame = Time.frameCount;
            if (currentFrame == frame)
                return;

            lastCompletedTicks = currentFrameTicks;
            lastCompletedOperations = currentFrameOperations;
            currentFrame = frame;
            currentFrameTicks = 0L;
            currentFrameOperations = 0;
        }
    }
}
