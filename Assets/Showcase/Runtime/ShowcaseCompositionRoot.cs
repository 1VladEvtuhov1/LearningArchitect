using System;
using LearningArchitect.UI;
using UnityEngine;

namespace LearningArchitect.Core
{
    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseRuntimeController))]
    [RequireComponent(typeof(ShowcaseCommandRouter))]
    [RequireComponent(typeof(ShowcaseStateHub))]
    public sealed class ShowcaseCompositionRoot : MonoBehaviour
    {
        [Header("Modules")]
        [SerializeField] private ModuleDefinitionSO[] modules;
        [SerializeField] private Transform moduleRoot;

        [Header("Runtime")]
        [SerializeField] private ShowcaseRuntimeController runtimeController;
        [SerializeField] private ShowcaseCommandRouter commandRouter;
        [SerializeField] private ShowcaseTransitionController transitionController;
        [SerializeField] private ShowcaseStateHub stateHub;

        public ModuleDefinitionSO[] Modules
        {
            get { return modules; }
            set { modules = value; }
        }

        public Transform ModuleRoot
        {
            get { return moduleRoot; }
            set { moduleRoot = value; }
        }

        public ShowcaseRuntimeController RuntimeController
        {
            get { return runtimeController; }
            set { runtimeController = value; }
        }

        public ShowcaseCommandRouter CommandRouter
        {
            get { return commandRouter; }
            set { commandRouter = value; }
        }

        public ShowcaseTransitionController TransitionController
        {
            get { return transitionController; }
            set { transitionController = value; }
        }

        public ShowcaseStateHub StateHub
        {
            get { return stateHub; }
            set { stateHub = value; }
        }

        private void Awake()
        {
            ResolveDependencies();
            stateHub.SetModuleCount(modules.Length);
            runtimeController.Configure(transitionController, stateHub);
            commandRouter.Configure(runtimeController);
            runtimeController.SetCoordinator(CreateCoordinator());
        }

        private ShowcaseCoordinator CreateCoordinator()
        {
            var selectionState = new ShowcaseSelectionState();
            var stressState = new ShowcaseStressState();
            var spawner = new VariantSpawner(moduleRoot);
            var runtimeHost = new ModuleRuntimeHost();
            return new ShowcaseCoordinator(modules, selectionState, stressState, spawner, runtimeHost);
        }

        private void ResolveDependencies()
        {
            runtimeController = runtimeController != null
                ? runtimeController
                : GetComponent<ShowcaseRuntimeController>();
            commandRouter = commandRouter != null
                ? commandRouter
                : GetComponent<ShowcaseCommandRouter>();
            transitionController = transitionController != null
                ? transitionController
                : GetComponent<ShowcaseTransitionController>();
            stateHub = stateHub != null
                ? stateHub
                : GetComponent<ShowcaseStateHub>();

            if (moduleRoot == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCompositionRoot)} requires {nameof(moduleRoot)}.");

            if (modules == null || modules.Length == 0)
                throw new InvalidOperationException($"{nameof(ShowcaseCompositionRoot)} requires at least one module definition.");

            if (runtimeController == null)
                throw new InvalidOperationException($"{nameof(ShowcaseRuntimeController)} is required.");

            if (commandRouter == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCommandRouter)} is required.");

            if (transitionController == null)
                throw new InvalidOperationException($"{nameof(ShowcaseTransitionController)} is required.");

            if (stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");
        }
    }
}
