using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseStateHub))]
    [RequireComponent(typeof(DescriptionPanel))]
    public sealed class DescriptionPresenter : MonoBehaviour
    {
        [SerializeField] private ShowcaseStateHub stateHub;
        [SerializeField] private DescriptionPanel view;

        private void Awake()
        {
            stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();
            view = view != null ? view : GetComponent<DescriptionPanel>();

            if (stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");

            if (view == null)
                throw new InvalidOperationException($"{nameof(DescriptionPanel)} is required.");
        }

        private void OnEnable()
        {
            stateHub.SelectionChanged += HandleSelectionChanged;
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
            Refresh();
        }

        private void OnDisable()
        {
            stateHub.SelectionChanged -= HandleSelectionChanged;
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
        }

        private void HandleSelectionChanged(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            view.SetContent(module, variant);
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            view.RefreshLocalizedContent();
        }

        private void Refresh()
        {
            view.SetContent(stateHub.CurrentModule, stateHub.CurrentVariant);
        }
    }
}
