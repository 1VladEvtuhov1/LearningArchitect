using System;
using UnityEngine;

namespace LearningArchitect.UI
{
    [Serializable]
    public struct SystemStatusRefreshSettings
    {
        [SerializeField] private float intervalSeconds;

        public float IntervalSeconds => intervalSeconds;

        public SystemStatusRefreshSettings(float intervalSeconds)
        {
            this.intervalSeconds = intervalSeconds;
        }

        public void Validate()
        {
            if (intervalSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
        }
    }

    [Serializable]
    public struct SystemStatusSamplingSettings
    {
        [SerializeField] private float memoryBarMaxGb;
        [SerializeField] private int smoothingSamples;

        public float MemoryBarMaxGb => memoryBarMaxGb;
        public int SmoothingSamples => smoothingSamples;

        public SystemStatusSamplingSettings(float memoryBarMaxGb, int smoothingSamples)
        {
            this.memoryBarMaxGb = memoryBarMaxGb;
            this.smoothingSamples = smoothingSamples;
        }

        public void Validate()
        {
            if (memoryBarMaxGb <= 0f)
                throw new ArgumentOutOfRangeException(nameof(memoryBarMaxGb));

            if (smoothingSamples < 1)
                throw new ArgumentOutOfRangeException(nameof(smoothingSamples));
        }
    }

    [Serializable]
    public struct SystemStatusThemeSettings
    {
        [SerializeField] private Color healthyDotColor;
        [SerializeField] private Color warningDotColor;
        [SerializeField] private Color criticalDotColor;
        [SerializeField] private float warningThresholdPercent;
        [SerializeField] private float criticalThresholdPercent;

        public Color HealthyDotColor => healthyDotColor == default ? ShowcasePalette.Success : healthyDotColor;
        public Color WarningDotColor => warningDotColor == default ? ShowcasePalette.Warning : warningDotColor;
        public Color CriticalDotColor => criticalDotColor == default ? ShowcasePalette.Error : criticalDotColor;
        public float WarningThresholdPercent => warningThresholdPercent <= 0f ? 70f : warningThresholdPercent;
        public float CriticalThresholdPercent => criticalThresholdPercent <= 0f ? 90f : criticalThresholdPercent;

        public SystemStatusThemeSettings(
            Color healthyDotColor,
            Color warningDotColor,
            Color criticalDotColor,
            float warningThresholdPercent,
            float criticalThresholdPercent)
        {
            this.healthyDotColor = healthyDotColor;
            this.warningDotColor = warningDotColor;
            this.criticalDotColor = criticalDotColor;
            this.warningThresholdPercent = warningThresholdPercent;
            this.criticalThresholdPercent = criticalThresholdPercent;
        }
    }
}
