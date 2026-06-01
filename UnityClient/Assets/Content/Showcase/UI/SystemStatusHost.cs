using System;
using LearningArchitect.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SystemStatusView))]
    public sealed class SystemStatusHost : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SystemStatusView _view;

        [Header("Settings")]
        [FormerlySerializedAs("refreshInterval")]
        [SerializeField] private SystemStatusRefreshSettings _refreshSettings = new(1f);
        [FormerlySerializedAs("memoryBarMaxGb")]
        [FormerlySerializedAs("smoothingSamples")]
        [SerializeField] private SystemStatusSamplingSettings _samplingSettings = new(8f, 2);
        [FormerlySerializedAs("healthyDotColor")]
        [FormerlySerializedAs("warningDotColor")]
        [FormerlySerializedAs("criticalDotColor")]
        [SerializeField] private SystemStatusThemeSettings _themeSettings = new(default, default, default, 70f, 90f);

        private SystemStatusRuntime _runtime;

        private void Awake()
        {
            ValidateConfiguration();

            SystemStatusTheme theme = SystemStatusTheme.FromSettings(_themeSettings);
            SystemStatusPresenter presenter = new(_view, theme, _samplingSettings.MemoryBarMaxGb);
            SystemStatusSampler sampler = new(_samplingSettings.SmoothingSamples);
            _runtime = new SystemStatusRuntime(sampler, presenter, _refreshSettings.IntervalSeconds);
            _runtime.Initialize();
        }

        private void OnEnable()
        {
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
            _runtime?.Refresh();
        }

        private void OnDisable()
        {
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
        }

        private void Update()
        {
            _runtime?.Tick(Time.unscaledTime);
        }

        private void OnDestroy()
        {
            _runtime?.Dispose();
        }

        private void ValidateConfiguration()
        {
            if (_view == null)
                throw new InvalidOperationException($"{nameof(SystemStatusView)} reference is required.");

            _view.ValidateReferences();
            _refreshSettings.Validate();
            _samplingSettings.Validate();
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            _runtime?.Refresh();
        }
    }
}
