using UnityEngine;

namespace LearningArchitect.UI
{
    public sealed class MetricsGraphController
    {
        private readonly MetricsOverlayView view;
        private readonly int sampleCapacity;
        private readonly float graphMaxFps;
        private readonly float warningFps;
        private readonly float lineThickness;
        private readonly float fillAlpha;
        private readonly float glowAlpha;
        private readonly float glowThicknessMultiplier;

        public MetricsGraphController(
            MetricsOverlayView view,
            int sampleCapacity,
            float graphMaxFps,
            float warningFps,
            float lineThickness,
            float fillAlpha,
            float glowAlpha,
            float glowThicknessMultiplier)
        {
            this.view = view ?? throw new System.ArgumentNullException(nameof(view));
            this.sampleCapacity = Mathf.Max(2, sampleCapacity);
            this.graphMaxFps = Mathf.Max(1f, graphMaxFps);
            this.warningFps = Mathf.Clamp(warningFps, 0f, this.graphMaxFps);
            this.lineThickness = Mathf.Max(1f, lineThickness);
            this.fillAlpha = Mathf.Clamp01(fillAlpha);
            this.glowAlpha = Mathf.Clamp01(glowAlpha);
            this.glowThicknessMultiplier = Mathf.Clamp(glowThicknessMultiplier, 1f, 4f);
        }

        public void PushLiveSample(float fps, Color accentColor, Color backgroundColor, Color labelColor)
        {
            view.ConfigureGraph(sampleCapacity, lineThickness, fillAlpha, glowAlpha, glowThicknessMultiplier, accentColor, backgroundColor);
            view.AddGraphSample(Mathf.Clamp01(fps / graphMaxFps));

            Color subduedAccent = ShowcasePalette.WithAlpha(accentColor, 0.76f);
            Color lowerAccent = ShowcasePalette.WithAlpha(labelColor, 0.92f);

            view.SetChartValue(0, Mathf.RoundToInt(graphMaxFps).ToString(), accentColor);
            view.SetChartValue(1, Mathf.RoundToInt(warningFps).ToString(), subduedAccent);
            view.SetChartValue(2, "0", lowerAccent);
        }
    }
}
