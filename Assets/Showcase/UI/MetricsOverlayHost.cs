using LearningArchitect.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MetricsOverlayView))]
    public sealed class MetricsOverlayHost : MonoBehaviour
    {
        [Header("References")]
        [FormerlySerializedAs("view")]
        [SerializeField] private MetricsOverlayView _view;

        [Header("Settings")]
        [SerializeField] private MetricsRefreshSettings _refreshSettings = new(0.25f);
        [SerializeField] private MetricsGraphSettings _graphSettings = new(64, 180f, 45f, 30f, 3.6f, 0.16f, 0.26f, 2.9f);
        [SerializeField] private MetricsThemeSettings _themeSettings = new(default, default, default, default, default, default, default);

        private MetricsOverlayRuntime _runtime;

        public void ShowActiveCount(int count)
        {
            _runtime?.SetActiveCount(count);
        }

        public void ShowMetrics(ShowcaseMetricsSnapshot metrics)
        {
            _runtime?.SetMetrics(metrics);
        }

        private void Awake()
        {
            BootstrapRuntime();
        }

        /// <summary>EditMode tests: runs the same setup as <see cref="Awake"/> without relying on reflection.</summary>
        internal void RunBootstrapForEditModeTests()
        {
            BootstrapRuntime();
        }

        private void BootstrapRuntime()
        {
            ValidateConfiguration();

            MetricsOverlayTheme theme = MetricsOverlayTheme.FromSettings(_themeSettings, _graphSettings);

            MetricsGraphController graphController = new(
                _view,
                _graphSettings.SampleCount,
                _graphSettings.MaxFps,
                _graphSettings.WarningFps,
                _graphSettings.LineThickness,
                _graphSettings.FillAlpha,
                _graphSettings.GlowAlpha,
                _graphSettings.GlowThicknessMultiplier);

            MetricsOverlayPresenter presenter = new(_view, graphController, theme);
            _runtime = new MetricsOverlayRuntime(new MetricsSampler(), presenter, _refreshSettings.IntervalSeconds);
            _runtime.Initialize();
        }

        private void OnEnable()
        {
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
            _runtime?.RefreshStaticTexts();
        }

        private void OnDisable()
        {
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
        }

        private void Update()
        {
            _runtime?.Tick(Time.unscaledDeltaTime, Time.unscaledTime);
        }

        private void ValidateConfiguration()
        {
            if (_view == null)
                throw new System.InvalidOperationException($"{nameof(MetricsOverlayView)} reference is required.");

            _view.ValidateReferences();
            _refreshSettings.Validate();
            _graphSettings.Validate();
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            _runtime?.RefreshStaticTexts();
        }
    }
}
