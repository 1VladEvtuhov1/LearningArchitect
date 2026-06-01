using System;
using LearningArchitect.Core;
using UnityEngine;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseCommandRouter))]
    [RequireComponent(typeof(ShowcaseRuntimeController))]
    [RequireComponent(typeof(ShowcaseStateHub))]
    [RequireComponent(typeof(ModuleNavigationControls))]
    [RequireComponent(typeof(HubUI))]
    public sealed class NavigationPresenter : MonoBehaviour
    {
        [SerializeField] private ShowcaseCommandRouter commands;
        [SerializeField] private ShowcaseRuntimeController runtimeController;
        [SerializeField] private ShowcaseStateHub stateHub;
        [SerializeField] private ModuleNavigationControls view;
        [SerializeField] private HubUI hubUI;

        private void Awake()
        {
            commands = commands != null ? commands : GetComponent<ShowcaseCommandRouter>();
            runtimeController = runtimeController != null ? runtimeController : GetComponent<ShowcaseRuntimeController>();
            stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();
            view = view != null ? view : GetComponent<ModuleNavigationControls>();
            hubUI = hubUI != null ? hubUI : GetComponent<HubUI>();

            if (commands == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCommandRouter)} is required.");

            if (stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");

            if (runtimeController == null)
                throw new InvalidOperationException($"{nameof(ShowcaseRuntimeController)} is required.");

            if (view == null)
                throw new InvalidOperationException($"{nameof(ModuleNavigationControls)} is required.");

            if (hubUI == null)
                throw new InvalidOperationException($"{nameof(HubUI)} is required.");
        }

        private void OnEnable()
        {
            stateHub.SelectionChanged += HandleSelectionChanged;
            view.PreviousModuleRequested += HandlePreviousModuleRequested;
            view.NextModuleRequested += HandleNextModuleRequested;
            view.NextCategoryRequested += HandleNextCategoryRequested;
            view.PreviousVariantRequested += HandlePreviousVariantRequested;
            view.NextVariantRequested += HandleNextVariantRequested;
            Refresh();
        }

        private void OnDisable()
        {
            stateHub.SelectionChanged -= HandleSelectionChanged;
            view.PreviousModuleRequested -= HandlePreviousModuleRequested;
            view.NextModuleRequested -= HandleNextModuleRequested;
            view.NextCategoryRequested -= HandleNextCategoryRequested;
            view.PreviousVariantRequested -= HandlePreviousVariantRequested;
            view.NextVariantRequested -= HandleNextVariantRequested;
        }

        private void HandleSelectionChanged(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            Refresh();
        }

        private void HandlePreviousModuleRequested()
        {
            hubUI.PlayModuleSwitchFeedback(-1);
            commands.PreviousModule();
        }

        private void HandleNextModuleRequested()
        {
            hubUI.PlayModuleSwitchFeedback(1);
            commands.NextModule();
        }

        private void HandleNextCategoryRequested()
        {
            hubUI.PlayCategorySwitchFeedback();
            commands.NextCategory();
        }

        private void HandlePreviousVariantRequested()
        {
            hubUI.PlayVariantSwitchFeedback(-1);
            commands.PreviousVariant();
        }

        private void HandleNextVariantRequested()
        {
            hubUI.PlayVariantSwitchFeedback(1);
            commands.NextVariant();
        }

        private void Refresh()
        {
            view.ShowNavigationState(
                stateHub.CanSwitchModules,
                runtimeController.CanSwitchCategories,
                stateHub.CanSwitchVariants);
        }
    }
}
