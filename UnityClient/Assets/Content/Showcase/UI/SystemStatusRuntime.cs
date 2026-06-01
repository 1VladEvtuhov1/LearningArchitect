namespace LearningArchitect.UI
{
    public sealed class SystemStatusRuntime
    {
        private readonly SystemStatusSampler sampler;
        private readonly SystemStatusPresenter presenter;
        private readonly float refreshInterval;

        private float nextRefreshTime;

        public SystemStatusRuntime(
            SystemStatusSampler sampler,
            SystemStatusPresenter presenter,
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

        public void Refresh()
        {
            presenter.RefreshView();
        }

        public void Tick(float currentTime)
        {
            if (currentTime < nextRefreshTime)
                return;

            nextRefreshTime = currentTime + refreshInterval;
            presenter.SetSample(sampler.Sample());
        }

        public void Dispose()
        {
            sampler.Dispose();
        }
    }
}
