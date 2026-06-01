using UnityEngine;

namespace LearningArchitect.UI
{
    public sealed class MetricsSampler
    {
        private readonly float smoothingFactor;

        private float smoothedFps;
        private float smoothedFrameTimeMs;

        public MetricsSampler(float smoothingFactor = 0.1f)
        {
            this.smoothingFactor = Mathf.Clamp01(smoothingFactor);
        }

        public MetricsSample Current => new(smoothedFps, smoothedFrameTimeMs);

        public MetricsSample Sample(float deltaTime)
        {
            if (deltaTime <= 0f)
                return Current;

            float fps = 1f / deltaTime;
            smoothedFps = smoothedFps <= 0f ? fps : Mathf.Lerp(smoothedFps, fps, smoothingFactor);

            float frameTimeMs = deltaTime * 1000f;
            smoothedFrameTimeMs = smoothedFrameTimeMs <= 0f
                ? frameTimeMs
                : Mathf.Lerp(smoothedFrameTimeMs, frameTimeMs, smoothingFactor);

            return Current;
        }
    }
}
