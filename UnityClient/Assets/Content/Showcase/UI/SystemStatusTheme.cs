using UnityEngine;

namespace LearningArchitect.UI
{
    public readonly struct SystemStatusTheme
    {
        public SystemStatusTheme(
            Color healthyDotColor,
            Color warningDotColor,
            Color criticalDotColor,
            float warningThresholdPercent,
            float criticalThresholdPercent)
        {
            HealthyDotColor = healthyDotColor;
            WarningDotColor = warningDotColor;
            CriticalDotColor = criticalDotColor;
            WarningThresholdPercent = warningThresholdPercent;
            CriticalThresholdPercent = criticalThresholdPercent;
        }

        public Color HealthyDotColor { get; }
        public Color WarningDotColor { get; }
        public Color CriticalDotColor { get; }
        public float WarningThresholdPercent { get; }
        public float CriticalThresholdPercent { get; }

        public Color GetStatusColor(float loadPercent)
        {
            if (loadPercent >= CriticalThresholdPercent)
                return CriticalDotColor;

            if (loadPercent >= WarningThresholdPercent)
                return WarningDotColor;

            return HealthyDotColor;
        }

        public static SystemStatusTheme FromSettings(SystemStatusThemeSettings settings)
        {
            return new SystemStatusTheme(
                settings.HealthyDotColor,
                settings.WarningDotColor,
                settings.CriticalDotColor,
                settings.WarningThresholdPercent,
                settings.CriticalThresholdPercent);
        }
    }
}
