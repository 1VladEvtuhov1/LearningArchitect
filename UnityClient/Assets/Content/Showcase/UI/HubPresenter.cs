using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseStateHub))]
    [RequireComponent(typeof(HubUI))]
    [RequireComponent(typeof(ShowcaseCompositionRoot))]
    [RequireComponent(typeof(ShowcaseRuntimeController))]
    public sealed class HubPresenter : MonoBehaviour
    {
        [SerializeField] private ShowcaseCompositionRoot compositionRoot;
        [SerializeField] private ShowcaseRuntimeController runtimeController;
        [SerializeField] private ShowcaseStateHub stateHub;
        [SerializeField] private HubUI view;

        private void Awake()
        {
            compositionRoot = compositionRoot != null ? compositionRoot : GetComponent<ShowcaseCompositionRoot>();
            runtimeController = runtimeController != null ? runtimeController : GetComponent<ShowcaseRuntimeController>();
            stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();
            view = view != null ? view : GetComponent<HubUI>();

            if (compositionRoot == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCompositionRoot)} is required.");

            if (runtimeController == null)
                throw new InvalidOperationException($"{nameof(ShowcaseRuntimeController)} is required.");

            if (stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");

            if (view == null)
                throw new InvalidOperationException($"{nameof(HubUI)} is required.");
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
            view.ShowSelection(module, variant);
            view.SetSelectorContext(BuildModuleSelectorText(module), BuildVariantSelectorText(module));
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            Refresh();
        }

        private void Refresh()
        {
            view.ShowSelection(stateHub.CurrentModule, stateHub.CurrentVariant);
            view.SetSelectorContext(BuildModuleSelectorText(stateHub.CurrentModule), BuildVariantSelectorText(stateHub.CurrentModule));
        }

        private string BuildModuleSelectorText(ModuleDefinitionSO currentModule)
        {
            ModuleDefinitionSO[] modules = compositionRoot.Modules;
            if (modules == null || modules.Length == 0 || currentModule == null)
                return ShowcaseLocalization.GetText("module_group");

            int currentInCategory = 0;
            int totalInCategory = 0;
            for (int i = 0; i < modules.Length; i++)
            {
                ModuleDefinitionSO module = modules[i];
                if (module == null || module.Category != currentModule.Category)
                    continue;

                totalInCategory++;
                if (i <= runtimeController.CurrentModuleIndex)
                    currentInCategory = totalInCategory;
            }

            string category = ShowcaseLocalization.GetModuleCategory(currentModule);
            return category + "  " + Mathf.Max(1, currentInCategory) + "/" + Mathf.Max(1, totalInCategory);
        }

        private string BuildVariantSelectorText(ModuleDefinitionSO currentModule)
        {
            int variantCount = currentModule?.Variants == null ? 0 : currentModule.Variants.Length;
            if (variantCount <= 0)
                return ShowcaseLocalization.GetText("variants");

            return ShowcaseLocalization.GetText("variants") + "  " +
                   (runtimeController.CurrentVariantIndex + 1) + "/" + variantCount;
        }
    }
}
