using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseCommandRouter))]
    [RequireComponent(typeof(ShowcaseStateHub))]
    [RequireComponent(typeof(StressTestControls))]
    public sealed class StressPresenter : MonoBehaviour
    {
        [SerializeField] private ShowcaseCommandRouter commands;
        [SerializeField] private ShowcaseStateHub stateHub;
        [SerializeField] private StressTestControls view;

        private void Awake()
        {
            commands = commands != null ? commands : GetComponent<ShowcaseCommandRouter>();
            stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();
            view = view != null ? view : GetComponent<StressTestControls>();

            if (commands == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCommandRouter)} is required.");

            if (stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");

            if (view == null)
                throw new InvalidOperationException($"{nameof(StressTestControls)} is required.");
        }

        private void OnEnable()
        {
            stateHub.SelectionChanged += HandleSelectionChanged;
            stateHub.StressStateChanged += HandleStressStateChanged;
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
            view.StressRequested += HandleStressRequested;
            Refresh();
        }

        private void OnDisable()
        {
            stateHub.SelectionChanged -= HandleSelectionChanged;
            stateHub.StressStateChanged -= HandleStressStateChanged;
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
            view.StressRequested -= HandleStressRequested;
        }

        private void HandleSelectionChanged(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            view.ConfigureModule(module);
            view.ConfigurePresets(
                variant != null ? variant.GetStressPresets() : null,
                variant != null ? variant.GetStressPresetLabels(ShowcaseLocalization.CurrentLanguage) : null);
            view.ShowStressState(stateHub.CurrentStressLevel, stateHub.ActiveItemCount);
        }

        private void HandleStressRequested(int level)
        {
            commands.SetStressLevel(level);
        }

        private void HandleStressStateChanged(int level, int activeCount)
        {
            view.ShowStressState(level, activeCount);
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            Refresh();
        }

        private void Refresh()
        {
            view.ConfigureModule(stateHub.CurrentModule);
            view.ConfigurePresets(
                stateHub.CurrentVariant != null ? stateHub.CurrentVariant.GetStressPresets() : null,
                stateHub.CurrentVariant != null ? stateHub.CurrentVariant.GetStressPresetLabels(ShowcaseLocalization.CurrentLanguage) : null);
            view.ShowStressState(stateHub.CurrentStressLevel, stateHub.ActiveItemCount);
        }
    }
}
