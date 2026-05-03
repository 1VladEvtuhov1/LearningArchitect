using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseStateHub))]
    [RequireComponent(typeof(MetricsOverlay))]
    public sealed class MetricsPresenter : MonoBehaviour
    {
        [SerializeField] private ShowcaseStateHub stateHub;
        [SerializeField] private MetricsOverlay view;

        private void Awake()
        {
            stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();
            view = view != null ? view : GetComponent<MetricsOverlay>();

            if (stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");

            if (view == null)
                throw new InvalidOperationException($"{nameof(MetricsOverlay)} is required.");
        }

        private void OnEnable()
        {
            stateHub.SelectionChanged += HandleSelectionChanged;
            stateHub.StressStateChanged += HandleStressStateChanged;
            stateHub.MetricsChanged += HandleMetricsChanged;
            Refresh();
        }

        private void OnDisable()
        {
            stateHub.SelectionChanged -= HandleSelectionChanged;
            stateHub.StressStateChanged -= HandleStressStateChanged;
            stateHub.MetricsChanged -= HandleMetricsChanged;
        }

        private void HandleSelectionChanged(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            view.ConfigureModule(module);
            view.ShowActiveCount(stateHub.ActiveItemCount);
            RefreshReferenceProfile();
        }

        private void HandleStressStateChanged(int level, int activeCount)
        {
            view.ShowActiveCount(activeCount);
            RefreshReferenceProfile();
        }

        private void HandleMetricsChanged(ShowcaseMetricsSnapshot metrics)
        {
            view.ShowMetrics(metrics);
        }

        private void Refresh()
        {
            view.ConfigureModule(stateHub.CurrentModule);
            view.ShowActiveCount(stateHub.ActiveItemCount);
            view.ShowMetrics(stateHub.CurrentMetrics);
            RefreshReferenceProfile();
        }

        private void RefreshReferenceProfile()
        {
            MetricsOverlay.ReferenceMetricsProfile profile = null;
            if (ShouldUseReferenceMetrics())
                MetricsOverlay.TryResolveWebDemoProfile(stateHub.CurrentVariant, stateHub.CurrentStressLevel, out profile);

            view.SetReferenceProfile(profile);
        }

        private static bool ShouldUseReferenceMetrics()
        {
            return Application.platform == RuntimePlatform.WebGLPlayer;
        }
    }
}
