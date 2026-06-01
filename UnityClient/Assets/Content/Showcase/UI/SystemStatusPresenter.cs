using System;
using UnityEngine;

namespace LearningArchitect.UI
{
    public sealed class SystemStatusPresenter
    {
        private readonly SystemStatusView _view;
        private readonly SystemStatusTheme _theme;
        private readonly float _memoryBarMaxGb;

        private SystemStatusSample _currentSample;

        public SystemStatusPresenter(
            SystemStatusView view,
            SystemStatusTheme theme,
            float memoryBarMaxGb)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _theme = theme;
            _memoryBarMaxGb = Mathf.Max(0.1f, memoryBarMaxGb);
        }

        public void Initialize()
        {
            _view.ValidateReferences();
            RefreshView();
        }

        public void SetSample(SystemStatusSample sample)
        {
            _currentSample = sample;
            RefreshView();
        }

        public void RefreshView()
        {
            float cpuNormalized = _currentSample.HasCpuPercent ? Mathf.Clamp01(_currentSample.CpuPercent / 100f) : 0f;
            float gpuNormalized = _currentSample.HasGpuPercent ? Mathf.Clamp01(_currentSample.GpuPercent / 100f) : 0f;
            float memoryNormalized = Mathf.Clamp01(_currentSample.MemoryGb / _memoryBarMaxGb);

            _view.SetMetric(0, SystemStatusFormatter.FormatCpu(_currentSample), cpuNormalized);
            _view.SetMetric(1, SystemStatusFormatter.FormatGpu(_currentSample), gpuNormalized);
            _view.SetMetric(2, SystemStatusFormatter.FormatMemory(_currentSample), memoryNormalized);

            float loadPercent = Mathf.Max(
                _currentSample.HasCpuPercent ? _currentSample.CpuPercent : 0f,
                _currentSample.HasGpuPercent ? _currentSample.GpuPercent : 0f);

            if (loadPercent <= 0f)
                loadPercent = memoryNormalized * 100f;

            _view.SetStatusColor(_theme.GetStatusColor(loadPercent));
        }
    }
}
