using LearningArchitect.Core;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ShowcaseCoordinatorTests
    {
        [Test]
        public void MoveToNextCategory_SkipsSameCategoryModules_AndResetsVariantIndex()
        {
            GameObject moduleRoot = new("Coordinator Root");
            GameObject sharedPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("Shared Runtime Prefab");

            ModuleDefinitionSO moduleA = null;
            ModuleDefinitionSO moduleB = null;
            ModuleDefinitionSO moduleC = null;

            try
            {
                moduleA = ShowcaseCoreTestFactory.CreateModule(
                    "Simulation A",
                    ShowcaseModuleCategory.SimulationModule,
                    ShowcaseCoreTestFactory.CreateVariant("Simulation A 0", sharedPrefab, 10, 20),
                    ShowcaseCoreTestFactory.CreateVariant("Simulation A 1", sharedPrefab, 10, 20));
                moduleB = ShowcaseCoreTestFactory.CreateModule(
                    "Simulation B",
                    ShowcaseModuleCategory.SimulationModule,
                    ShowcaseCoreTestFactory.CreateVariant("Simulation B 0", sharedPrefab, 10, 20));
                moduleC = ShowcaseCoreTestFactory.CreateModule(
                    "Architecture C",
                    ShowcaseModuleCategory.ArchitecturePatternModule,
                    ShowcaseCoreTestFactory.CreateVariant("Architecture C 0", sharedPrefab, 10, 20));

                ModuleDefinitionSO[] modules = { moduleA, moduleB, moduleC };
                ShowcaseSelectionState selectionState = new();
                selectionState.SetSelection(0, 1, modules);

                ShowcaseCoordinator coordinator = new(
                    modules,
                    selectionState,
                    new ShowcaseStressState(),
                    new VariantSpawner(moduleRoot.transform),
                    new ModuleRuntimeHost());

                coordinator.MoveToNextCategory();

                Assert.AreEqual(2, coordinator.CurrentModuleIndex);
                Assert.AreEqual(0, coordinator.CurrentVariantIndex);
                Assert.AreSame(moduleC, coordinator.CurrentModule);
            }
            finally
            {
                DestroyModuleGraph(moduleA);
                DestroyModuleGraph(moduleB);
                DestroyModuleGraph(moduleC);
                DestroyTestObjects(moduleRoot, sharedPrefab);
            }
        }

        [Test]
        public void ActivateCurrentVariant_ReplacesRuntimeInstance_AndNormalizesStressForNewVariant()
        {
            GameObject moduleRoot = new("Coordinator Root");
            GameObject pooledPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("Pooled Runtime Prefab");
            GameObject centralizedPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("Centralized Runtime Prefab");

            ModuleDefinitionSO module = null;

            try
            {
                VariantDefinitionSO pooledVariant = ShowcaseCoreTestFactory.CreateVariant("Pooled Variant", pooledPrefab, 20, 40);
                VariantDefinitionSO centralizedVariant = ShowcaseCoreTestFactory.CreateVariant("Centralized Variant", centralizedPrefab, 12, 24);
                module = ShowcaseCoreTestFactory.CreateModule(
                    "Performance",
                    ShowcaseModuleCategory.SimulationModule,
                    pooledVariant,
                    centralizedVariant);

                ModuleDefinitionSO[] modules = { module };
                ShowcaseCoordinator coordinator = new(
                    modules,
                    new ShowcaseSelectionState(),
                    new ShowcaseStressState(),
                    new VariantSpawner(moduleRoot.transform),
                    new ModuleRuntimeHost());

                coordinator.ActivateCurrentVariant();

                Assert.AreEqual(1, moduleRoot.transform.childCount);
                Assert.AreEqual(20, coordinator.CurrentStressLevel);
                Assert.AreEqual(20, coordinator.ActiveItemCount);
                Assert.AreEqual(20, coordinator.CurrentMetrics.VisibleCount);

                ShowcaseTestRuntimeModule firstInstance = moduleRoot.transform.GetChild(0).GetComponent<ShowcaseTestRuntimeModule>();
                Assert.NotNull(firstInstance);
                Assert.AreEqual(1, firstInstance.EnterCallCount);

                coordinator.MoveToNextVariant();
                coordinator.ActivateCurrentVariant();

                Assert.AreEqual(1, moduleRoot.transform.childCount);
                Assert.IsTrue(firstInstance == null);
                Assert.AreEqual("Centralized Runtime Prefab", moduleRoot.transform.GetChild(0).name);
                Assert.AreEqual(12, coordinator.CurrentStressLevel);
                Assert.AreEqual(12, coordinator.ActiveItemCount);
                Assert.AreEqual(24, coordinator.CurrentMetrics.OperationsPerFrame);
            }
            finally
            {
                DestroyModuleGraph(module);
                DestroyTestObjects(moduleRoot, pooledPrefab, centralizedPrefab);
            }
        }

        [Test]
        public void ActivateCurrentVariant_PreservesSelectedStressPresetIndex_WhenSwitchingVariants()
        {
            GameObject moduleRoot = new("Coordinator Root");
            GameObject firstPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("First Runtime Prefab");
            GameObject secondPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("Second Runtime Prefab");

            ModuleDefinitionSO module = null;

            try
            {
                VariantDefinitionSO firstVariant = ShowcaseCoreTestFactory.CreateVariant("First Variant", firstPrefab, 10, 20, 30);
                VariantDefinitionSO secondVariant = ShowcaseCoreTestFactory.CreateVariant("Second Variant", secondPrefab, 100, 200, 300);
                module = ShowcaseCoreTestFactory.CreateModule(
                    "Performance",
                    ShowcaseModuleCategory.SimulationModule,
                    firstVariant,
                    secondVariant);

                ShowcaseCoordinator coordinator = new(
                    new[] { module },
                    new ShowcaseSelectionState(),
                    new ShowcaseStressState(),
                    new VariantSpawner(moduleRoot.transform),
                    new ModuleRuntimeHost());

                coordinator.ActivateCurrentVariant();
                coordinator.SetStressLevel(30);

                Assert.AreEqual(30, coordinator.CurrentStressLevel);

                coordinator.MoveToNextVariant();
                coordinator.ActivateCurrentVariant();

                Assert.AreEqual(300, coordinator.CurrentStressLevel);
                Assert.AreEqual(300, coordinator.ActiveItemCount);
                Assert.AreEqual(300, coordinator.CurrentMetrics.VisibleCount);
            }
            finally
            {
                DestroyModuleGraph(module);
                DestroyTestObjects(moduleRoot, firstPrefab, secondPrefab);
            }
        }

        private static void DestroyTestObjects(params Object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    Object.DestroyImmediate(objects[i]);
            }
        }

        private static void DestroyModuleGraph(ModuleDefinitionSO module)
        {
            if (module == null)
                return;

            VariantDefinitionSO[] variants = module.Variants;
            if (variants != null)
            {
                for (int i = 0; i < variants.Length; i++)
                {
                    if (variants[i]?.Prefab != null)
                        Object.DestroyImmediate(variants[i].Prefab);

                    if (variants[i] != null)
                        Object.DestroyImmediate(variants[i]);
                }
            }

            Object.DestroyImmediate(module);
        }
    }
}
