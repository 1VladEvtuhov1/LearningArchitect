using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseStateHub))]
    [RequireComponent(typeof(MetricsOverlayHost))]
    public sealed class MetricsPresenter : MonoBehaviour
    {
        [SerializeField] private ShowcaseStateHub stateHub;
        [SerializeField] private MetricsOverlayHost view;

        private void Awake()
        {
            stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();
            view = view != null ? view : GetComponent<MetricsOverlayHost>();

            if (stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");

            if (view == null)
                throw new InvalidOperationException($"{nameof(MetricsOverlayHost)} is required.");
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
            view.ShowActiveCount(stateHub.ActiveItemCount);
        }

        private void HandleStressStateChanged(int level, int activeCount)
        {
            view.ShowActiveCount(activeCount);
        }

        private void HandleMetricsChanged(ShowcaseMetricsSnapshot metrics)
        {
            view.ShowMetrics(metrics);
        }

        private void Refresh()
        {
            view.ShowActiveCount(stateHub.ActiveItemCount);
            view.ShowMetrics(stateHub.CurrentMetrics);
        }
    }
}
