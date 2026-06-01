using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.UI
{
    public sealed class MetricsOverlayPresenter
    {
        private readonly MetricsOverlayView view;
        private readonly MetricsGraphController graphController;
        private readonly MetricsOverlayTheme theme;

        private int activeCount;
        private ShowcaseMetricsSnapshot currentMetrics = ShowcaseMetricsSnapshot.Empty;
        private MetricsSample currentSample;

        public MetricsOverlayPresenter(
            MetricsOverlayView view,
            MetricsGraphController graphController,
            MetricsOverlayTheme theme)
        {
            this.view = view ?? throw new System.ArgumentNullException(nameof(view));
            this.graphController = graphController ?? throw new System.ArgumentNullException(nameof(graphController));
            this.theme = theme;
        }

        public void Initialize()
        {
            view.ValidateReferences();
            RefreshStaticTexts();
            RefreshValues();
        }

        public void RefreshStaticTexts()
        {
            view.SetHeader(MetricsOverlayFormatter.GetHeaderText(), theme.TitleColor);
            view.SetMetricLabel(0, MetricsOverlayFormatter.GetFpsLabel(), theme.LabelColor);
            view.SetMetricLabel(1, MetricsOverlayFormatter.GetFrameTimeLabel(), theme.LabelColor);
            view.SetMetricLabel(2, MetricsOverlayFormatter.GetActiveItemsLabel(), theme.LabelColor);
        }

        public void SetActiveCount(int count)
        {
            activeCount = Mathf.Max(0, count);
            RefreshValues();
        }

        public void SetMetrics(ShowcaseMetricsSnapshot metrics)
        {
            currentMetrics = metrics;
            RefreshValues();
        }

        public void PresentLiveSample(MetricsSample sample, bool refreshValues)
        {
            currentSample = sample;

            Color accentColor = theme.GetAccentColor(sample.Fps);
            graphController.PushLiveSample(
                sample.Fps,
                accentColor,
                theme.GraphBackgroundColor,
                theme.LabelColor);

            if (refreshValues)
                RefreshValues();
        }

        private void RefreshValues()
        {
            Color accentColor = theme.GetAccentColor(currentSample.Fps);

            view.SetMetricValue(0, MetricsOverlayFormatter.FormatFps(currentSample.Fps), accentColor, true);
            view.SetMetricValue(1, MetricsOverlayFormatter.FormatFrameTime(currentSample.FrameTimeMs), theme.ValueColor, true);
            view.SetMetricValue(2, MetricsOverlayFormatter.FormatCount(GetDisplayedItemCount()), theme.ValueColor, true);
        }

        private int GetDisplayedItemCount()
        {
            return currentMetrics.HasVisibleCount ? currentMetrics.VisibleCount : activeCount;
        }
    }
}
