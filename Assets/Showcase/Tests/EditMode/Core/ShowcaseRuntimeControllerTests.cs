using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ShowcaseRuntimeControllerTests
    {
        [Test]
        public void SetCoordinator_UpdatesModuleCountInStateHub()
        {
            RuntimeControllerHarness harness = RuntimeControllerHarness.Create(
                CreateModule("Simulation", 16, 32),
                CreateModule("Architecture", 24, 48));

            try
            {
                harness.Controller.SetCoordinator(harness.Coordinator);

                Assert.AreEqual(2, harness.StateHub.ModuleCount);
                Assert.IsTrue(harness.StateHub.CanSwitchModules);
            }
            finally
            {
                harness.Dispose();
            }
        }

        [Test]
        public void LoadSelection_PublishesSelectionAndStressState()
        {
            ModuleDefinitionSO simulationModule = CreateModule("Simulation", 16, 32);
            ModuleDefinitionSO architectureModule = CreateModule("Architecture", 24, 48);
            RuntimeControllerHarness harness = RuntimeControllerHarness.Create(simulationModule, architectureModule);

            try
            {
                harness.Controller.SetCoordinator(harness.Coordinator);
                harness.Controller.LoadSelection(1, 0);

                Assert.AreSame(architectureModule, harness.StateHub.CurrentModule);
                Assert.AreSame(architectureModule.Variants[0], harness.StateHub.CurrentVariant);
                Assert.AreEqual(24, harness.StateHub.CurrentStressLevel);
                Assert.AreEqual(24, harness.StateHub.ActiveItemCount);
                Assert.AreEqual(24, harness.StateHub.CurrentMetrics.SimulationCount);
            }
            finally
            {
                harness.Dispose();
            }
        }

        [Test]
        public void SetStressLevel_PublishesNormalizedStressState()
        {
            ModuleDefinitionSO module = CreateModule("Simulation", 16, 32);
            RuntimeControllerHarness harness = RuntimeControllerHarness.Create(module);

            try
            {
                harness.Controller.SetCoordinator(harness.Coordinator);
                harness.Controller.LoadSelection(0, 0);

                harness.Controller.SetStressLevel(32);
                Assert.AreEqual(32, harness.StateHub.CurrentStressLevel);
                Assert.AreEqual(32, harness.StateHub.ActiveItemCount);

                harness.Controller.SetStressLevel(999);
                Assert.AreEqual(16, harness.StateHub.CurrentStressLevel);
                Assert.AreEqual(16, harness.StateHub.ActiveItemCount);
                Assert.AreEqual(16, harness.Controller.CurrentStressLevel);
            }
            finally
            {
                harness.Dispose();
            }
        }

        private static ModuleDefinitionSO CreateModule(string name, params int[] stressPresets)
        {
            GameObject prefab = ShowcaseCoreTestFactory.CreateRuntimePrefab(name + " Runtime Prefab");
            VariantDefinitionSO variant = ShowcaseCoreTestFactory.CreateVariant(name + " Variant", prefab, stressPresets);
            return ShowcaseCoreTestFactory.CreateModule(name, ShowcaseModuleCategory.SimulationModule, variant);
        }

        private sealed class RuntimeControllerHarness
        {
            private readonly ModuleDefinitionSO[] modules;

            private RuntimeControllerHarness(
                GameObject controllerRoot,
                GameObject moduleRoot,
                ShowcaseStateHub stateHub,
                ShowcaseRuntimeController controller,
                ShowcaseCoordinator coordinator,
                ModuleDefinitionSO[] modules)
            {
                ControllerRoot = controllerRoot;
                ModuleRoot = moduleRoot;
                StateHub = stateHub;
                Controller = controller;
                Coordinator = coordinator;
                this.modules = modules;
            }

            public GameObject ControllerRoot { get; }
            public GameObject ModuleRoot { get; }
            public ShowcaseStateHub StateHub { get; }
            public ShowcaseRuntimeController Controller { get; }
            public ShowcaseCoordinator Coordinator { get; }

            public static RuntimeControllerHarness Create(params ModuleDefinitionSO[] modules)
            {
                GameObject controllerRoot = new("Runtime Controller Root");
                GameObject moduleRoot = new("Runtime Module Root");
                ShowcaseStateHub stateHub = controllerRoot.AddComponent<ShowcaseStateHub>();
                ShowcaseTransitionController transitionController = controllerRoot.AddComponent<ShowcaseTransitionController>();
                ShowcaseRuntimeController controller = controllerRoot.AddComponent<ShowcaseRuntimeController>();
                controller.Configure(transitionController, stateHub);

                ShowcaseCoordinator coordinator = new(
                    modules,
                    new ShowcaseSelectionState(),
                    new ShowcaseStressState(),
                    new VariantSpawner(moduleRoot.transform),
                    new ModuleRuntimeHost());

                return new RuntimeControllerHarness(controllerRoot, moduleRoot, stateHub, controller, coordinator, modules);
            }

            public void Dispose()
            {
                if (ControllerRoot != null)
                    Object.DestroyImmediate(ControllerRoot);

                if (ModuleRoot != null)
                    Object.DestroyImmediate(ModuleRoot);

                for (int i = 0; i < modules.Length; i++)
                {
                    if (modules[i] == null)
                        continue;

                    VariantDefinitionSO[] variants = modules[i].Variants;
                    if (variants != null)
                    {
                        for (int j = 0; j < variants.Length; j++)
                        {
                            if (variants[j]?.Prefab != null)
                                Object.DestroyImmediate(variants[j].Prefab);

                            if (variants[j] != null)
                                Object.DestroyImmediate(variants[j]);
                        }
                    }

                    Object.DestroyImmediate(modules[i]);
                }
            }
        }
    }
}
