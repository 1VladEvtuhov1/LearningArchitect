using UnityEngine;

namespace LearningArchitect.UI
{
    public readonly struct MetricsOverlayTheme
    {
        public MetricsOverlayTheme(
            Color titleColor,
            Color labelColor,
            Color valueColor,
            Color graphBackgroundColor,
            Color healthyColor,
            Color warningColor,
            Color criticalColor,
            float warningFps,
            float criticalFps)
        {
            TitleColor = titleColor;
            LabelColor = labelColor;
            ValueColor = valueColor;
            GraphBackgroundColor = graphBackgroundColor;
            HealthyColor = healthyColor;
            WarningColor = warningColor;
            CriticalColor = criticalColor;
            WarningFps = warningFps;
            CriticalFps = criticalFps;
        }

        public Color TitleColor { get; }
        public Color LabelColor { get; }
        public Color ValueColor { get; }
        public Color GraphBackgroundColor { get; }
        public Color HealthyColor { get; }
        public Color WarningColor { get; }
        public Color CriticalColor { get; }
        public float WarningFps { get; }
        public float CriticalFps { get; }

        public Color GetAccentColor(float fps)
        {
            if (fps < CriticalFps)
                return CriticalColor;

            if (fps < WarningFps)
                return WarningColor;

            return HealthyColor;
        }

        public static MetricsOverlayTheme FromSettings(MetricsThemeSettings settings, MetricsGraphSettings graphSettings)
        {
            return new MetricsOverlayTheme(
                settings.TitleColor,
                settings.LabelColor,
                settings.ValueColor,
                settings.GraphBackgroundColor,
                settings.HealthyColor,
                settings.WarningColor,
                settings.CriticalColor,
                graphSettings.WarningFps,
                graphSettings.CriticalFps);
        }
    }
}
