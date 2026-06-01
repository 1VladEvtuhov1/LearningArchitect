using System;
using LearningArchitect.UI;
using UnityEngine;

namespace LearningArchitect.Core
{
    [DisallowMultipleComponent]
    public sealed class ShowcaseRuntimeController : MonoBehaviour
    {
        private ShowcaseCoordinator coordinator;
        private ShowcaseTransitionController transitionController;
        private ShowcaseStateHub stateHub;
        private int lastPublishedStressLevel = int.MinValue;
        private int lastPublishedActiveItemCount = int.MinValue;
        private ShowcaseMetricsSnapshot lastPublishedMetrics = ShowcaseMetricsSnapshot.Empty;

        public int CurrentModuleIndex => coordinator == null ? 0 : coordinator.CurrentModuleIndex;

        public int CurrentVariantIndex => coordinator == null ? 0 : coordinator.CurrentVariantIndex;

        public ModuleDefinitionSO CurrentModule => coordinator == null ? null : coordinator.CurrentModule;

        public VariantDefinitionSO CurrentVariant => coordinator == null ? null : coordinator.CurrentVariant;

        public int CurrentStressLevel => coordinator == null ? 1000 : coordinator.CurrentStressLevel;

        public int ActiveItemCount => coordinator == null ? 0 : coordinator.ActiveItemCount;

        public int ModuleCount => coordinator == null ? 0 : coordinator.ModuleCount;

        public bool CanSwitchModules => coordinator != null && coordinator.CanSwitchModules();

        public bool CanSwitchCategories => coordinator != null && coordinator.CanSwitchCategories();

        public bool CanSwitchVariants => coordinator != null && coordinator.CanSwitchVariants();

        private void Start()
        {
            if (coordinator == null)
                throw new InvalidOperationException($"{nameof(ShowcaseRuntimeController)} requires {nameof(ShowcaseCoordinator)} configuration.");

            LoadCurrentImmediate();
        }

        private void Update()
        {
            if (coordinator == null)
                return;

            coordinator.SyncRuntimeState();
            PublishStressStateIfChanged();
        }

        private void OnDestroy()
        {
            if (coordinator != null)
                coordinator.DeactivateCurrentVariant();
        }

        public void Configure(ShowcaseTransitionController transitionController, ShowcaseStateHub stateHub)
        {
            this.transitionController = transitionController != null
                ? transitionController
                : GetComponent<ShowcaseTransitionController>();
            this.stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();

            if (this.transitionController == null)
                throw new InvalidOperationException($"{nameof(ShowcaseTransitionController)} is required.");

            if (this.stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");

            this.transitionController.InitializeOverlay();
        }

        public void SetCoordinator(ShowcaseCoordinator coordinator)
        {
            if (this.coordinator == coordinator)
                return;

            if (Application.isPlaying && this.coordinator != null)
                this.coordinator.DeactivateCurrentVariant();

            this.coordinator = coordinator;
            if (stateHub != null)
                stateHub.SetModuleCount(coordinator == null ? 0 : coordinator.ModuleCount);
            lastPublishedStressLevel = int.MinValue;
            lastPublishedActiveItemCount = int.MinValue;
            lastPublishedMetrics = ShowcaseMetricsSnapshot.Empty;
        }

        public void PreviousModule()
        {
            if (coordinator == null || !coordinator.HasModules())
                return;

            coordinator.MoveToPreviousModule();
            LoadCurrent();
        }

        public void NextModule()
        {
            if (coordinator == null || !coordinator.HasModules())
                return;

            coordinator.MoveToNextModule();
            LoadCurrent();
        }

        public void NextCategory()
        {
            if (coordinator == null || !coordinator.CanSwitchCategories())
                return;

            coordinator.MoveToNextCategory();
            LoadCurrent();
        }

        public void PreviousVariant()
        {
            if (coordinator == null || !coordinator.CanSwitchVariants())
                return;

            coordinator.MoveToPreviousVariant();
            LoadCurrent();
        }

        public void NextVariant()
        {
            if (coordinator == null || !coordinator.CanSwitchVariants())
                return;

            coordinator.MoveToNextVariant();
            LoadCurrent();
        }

        public void SetStressLevel(int count)
        {
            if (coordinator != null)
                coordinator.SetStressLevel(count);

            PublishStressStateIfChanged();
        }

        public void LoadSelection(int moduleIndex, int variantIndex)
        {
            if (coordinator == null || !coordinator.HasModules())
                return;

            coordinator.Select(moduleIndex, variantIndex);
            LoadCurrent();
        }

        public void LoadCurrent()
        {
            if (!isActiveAndEnabled || transitionController == null || !transitionController.CanAnimate)
            {
                LoadCurrentImmediate();
                return;
            }

            PublishSelection();

            if (Application.isPlaying && Time.frameCount > 1)
            {
                transitionController.Play(LoadCurrentImmediate);
                return;
            }

            LoadCurrentImmediate();
        }

        private void LoadCurrentImmediate()
        {
            if (coordinator == null)
                return;

            coordinator.ActivateCurrentVariant();
            PublishSelection();
            PublishStressStateIfChanged();
        }

        private void PublishSelection()
        {
            stateHub.PublishSelection(CurrentModule, CurrentVariant);
        }

        private void PublishStressStateIfChanged()
        {
            int stressLevel = CurrentStressLevel;
            int activeItemCount = ActiveItemCount;
            ShowcaseMetricsSnapshot metrics = coordinator == null ? ShowcaseMetricsSnapshot.Empty : coordinator.CurrentMetrics;
            if (stressLevel == lastPublishedStressLevel &&
                activeItemCount == lastPublishedActiveItemCount &&
                metrics == lastPublishedMetrics)
                return;

            lastPublishedStressLevel = stressLevel;
            lastPublishedActiveItemCount = activeItemCount;
            lastPublishedMetrics = metrics;
            stateHub.PublishStress(stressLevel, activeItemCount, metrics);
        }
    }
}
