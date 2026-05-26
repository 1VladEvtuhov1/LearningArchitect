using System;
using UnityEngine;

namespace LearningArchitect.UI
{
    [Serializable]
    public struct MetricsRefreshSettings
    {
        [SerializeField] private float intervalSeconds;

        public float IntervalSeconds => intervalSeconds;

        public MetricsRefreshSettings(float intervalSeconds)
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
    public struct MetricsGraphSettings
    {
        [SerializeField] private int sampleCount;
        [SerializeField] private float maxFps;
        [SerializeField] private float warningFps;
        [SerializeField] private float criticalFps;
        [SerializeField] private float lineThickness;
        [SerializeField] private float fillAlpha;
        [SerializeField] private float glowAlpha;
        [SerializeField] private float glowThicknessMultiplier;

        public int SampleCount => sampleCount;
        public float MaxFps => maxFps;
        public float WarningFps => warningFps;
        public float CriticalFps => criticalFps;
        public float LineThickness => lineThickness;
        public float FillAlpha => fillAlpha;
        public float GlowAlpha => glowAlpha;
        public float GlowThicknessMultiplier => glowThicknessMultiplier;

        public MetricsGraphSettings(
            int sampleCount,
            float maxFps,
            float warningFps,
            float criticalFps,
            float lineThickness,
            float fillAlpha,
            float glowAlpha,
            float glowThicknessMultiplier)
        {
            this.sampleCount = sampleCount;
            this.maxFps = maxFps;
            this.warningFps = warningFps;
            this.criticalFps = criticalFps;
            this.lineThickness = lineThickness;
            this.fillAlpha = fillAlpha;
            this.glowAlpha = glowAlpha;
            this.glowThicknessMultiplier = glowThicknessMultiplier;
        }

        public void Validate()
        {
            if (sampleCount < 2)
                throw new ArgumentOutOfRangeException(nameof(sampleCount));

            if (maxFps <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxFps));

            if (warningFps <= 0f)
                throw new ArgumentOutOfRangeException(nameof(warningFps));

            if (criticalFps <= 0f)
                throw new ArgumentOutOfRangeException(nameof(criticalFps));
        }
    }

    [Serializable]
    public struct MetricsThemeSettings
    {
        [SerializeField] private Color titleColor;
        [SerializeField] private Color labelColor;
        [SerializeField] private Color valueColor;
        [SerializeField] private Color graphBackgroundColor;
        [SerializeField] private Color healthyColor;
        [SerializeField] private Color warningColor;
        [SerializeField] private Color criticalColor;

        public Color TitleColor => titleColor == default ? ShowcasePalette.AccentMain : titleColor;
        public Color LabelColor => labelColor == default ? ShowcasePalette.TextSecondary : labelColor;
        public Color ValueColor => valueColor == default ? ShowcasePalette.TextPrimary : valueColor;
        public Color GraphBackgroundColor => graphBackgroundColor == default
            ? ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.055f)
            : graphBackgroundColor;
        public Color HealthyColor => healthyColor == default ? ShowcasePalette.AccentMain : healthyColor;
        public Color WarningColor => warningColor == default ? ShowcasePalette.Warning : warningColor;
        public Color CriticalColor => criticalColor == default ? ShowcasePalette.Error : criticalColor;

        public MetricsThemeSettings(
            Color titleColor,
            Color labelColor,
            Color valueColor,
            Color graphBackgroundColor,
            Color healthyColor,
            Color warningColor,
            Color criticalColor)
        {
            this.titleColor = titleColor;
            this.labelColor = labelColor;
            this.valueColor = valueColor;
            this.graphBackgroundColor = graphBackgroundColor;
            this.healthyColor = healthyColor;
            this.warningColor = warningColor;
            this.criticalColor = criticalColor;
        }
    }
}
