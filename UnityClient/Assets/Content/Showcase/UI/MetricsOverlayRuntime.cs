using LearningArchitect.Core;

namespace LearningArchitect.UI
{
    public sealed class MetricsOverlayRuntime
    {
        private readonly MetricsSampler sampler;
        private readonly MetricsOverlayPresenter presenter;
        private readonly float refreshInterval;

        private float nextRefreshTime;

        public MetricsOverlayRuntime(
            MetricsSampler sampler,
            MetricsOverlayPresenter presenter,
            float refreshInterval)
        {
            this.sampler = sampler ?? throw new System.ArgumentNullException(nameof(sampler));
            this.presenter = presenter ?? throw new System.ArgumentNullException(nameof(presenter));
            this.refreshInterval = refreshInterval;
        }

        public void Initialize()
        {
            presenter.Initialize();
        }

        public void RefreshStaticTexts()
        {
            presenter.RefreshStaticTexts();
        }

        public void SetActiveCount(int count)
        {
            presenter.SetActiveCount(count);
        }

        public void SetMetrics(ShowcaseMetricsSnapshot metrics)
        {
            presenter.SetMetrics(metrics);
        }

        public void Tick(float deltaTime, float currentTime)
        {
            MetricsSample sample = sampler.Sample(deltaTime);
            bool shouldRefreshValues = currentTime >= nextRefreshTime;
            if (shouldRefreshValues)
                nextRefreshTime = currentTime + refreshInterval;

            presenter.PresentLiveSample(sample, shouldRefreshValues);
        }
    }
}
