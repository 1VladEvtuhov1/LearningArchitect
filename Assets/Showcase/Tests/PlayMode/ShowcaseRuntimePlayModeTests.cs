using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LearningArchitect.Tests.PlayMode
{
    [Category("LearningArchitect.Showcase.PlayMode")]
    public sealed class ShowcaseRuntimePlayModeTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private readonly List<UnityEngine.Object> createdObjects = new();
        private readonly Dictionary<VariantDefinitionSO, int> variantVisibleCounts = new();

        [SetUp]
        public void SetUp()
        {
            createdObjects.Clear();
            variantVisibleCounts.Clear();
            SmokeRuntimeVariant.Reset();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object createdObject = createdObjects[i];
                if (createdObject == null)
                    continue;

                if (createdObject is GameObject gameObject)
                    UnityEngine.Object.Destroy(gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
            variantVisibleCounts.Clear();
            SmokeRuntimeVariant.Reset();
            yield return null;
        }

        [UnityTest]
        public IEnumerator CompositionRoot_ActivatesInitialVariantAndPublishesStressState()
        {
            RuntimeHarness harness = CreateHarness(
                CreateModule(
                    "ModuleA",
                    CreateVariant("VariantA", 4, 10, 20, 40)));

            yield return WaitForRuntime();

            Assert.AreSame(harness.Modules[0], harness.StateHub.CurrentModule);
            Assert.AreSame(harness.Modules[0].Variants[0], harness.StateHub.CurrentVariant);
            Assert.AreEqual(10, harness.StateHub.CurrentStressLevel);
            Assert.AreEqual(4, harness.StateHub.ActiveItemCount);
            Assert.AreEqual(10, harness.StateHub.CurrentMetrics.SimulationCount);
            Assert.AreEqual(4, harness.StateHub.CurrentMetrics.VisibleCount);
            Assert.AreEqual(1, harness.ModuleRoot.childCount);
            Assert.AreEqual(1, SmokeRuntimeVariant.GetEnterCount("VariantA"));
            Assert.AreEqual(10, SmokeRuntimeVariant.GetLastStress("VariantA"));
        }

        [UnityTest]
        public IEnumerator RuntimeController_RemapsStressAndReactivatesWhenSwitchingModules()
        {
            RuntimeHarness harness = CreateHarness(
                CreateModule(
                    "ModuleA",
                    CreateVariant("VariantA", 4, 10, 20)),
                CreateModule(
                    "ModuleB",
                    CreateVariant("VariantB", 7, 100, 200)));

            yield return WaitForRuntime();

            harness.RuntimeController.SetStressLevel(20);
            yield return null;

            Assert.AreEqual(20, harness.StateHub.CurrentStressLevel);
            Assert.AreEqual(20, SmokeRuntimeVariant.GetLastStress("VariantA"));

            harness.RuntimeController.NextModule();
            yield return null;

            Assert.AreSame(harness.Modules[1], harness.StateHub.CurrentModule);
            Assert.AreSame(harness.Modules[1].Variants[0], harness.StateHub.CurrentVariant);
            Assert.AreEqual(200, harness.StateHub.CurrentStressLevel);
            Assert.AreEqual(7, harness.StateHub.ActiveItemCount);
            Assert.AreEqual(200, harness.StateHub.CurrentMetrics.SimulationCount);
            Assert.AreEqual(7, harness.StateHub.CurrentMetrics.VisibleCount);
            Assert.AreEqual(1, harness.ModuleRoot.childCount);
            Assert.AreEqual(1, SmokeRuntimeVariant.GetEnterCount("VariantA"));
            Assert.AreEqual(1, SmokeRuntimeVariant.GetExitCount("VariantA"));
            Assert.AreEqual(1, SmokeRuntimeVariant.GetEnterCount("VariantB"));
            Assert.AreEqual(200, SmokeRuntimeVariant.GetLastStress("VariantB"));
        }

        private RuntimeHarness CreateHarness(params ModuleDefinitionSO[] modules)
        {
            GameObject templateRoot = Track(new GameObject("VariantTemplates"));
            templateRoot.SetActive(false);

            GameObject root = Track(new GameObject("ShowcaseRuntimePlayModeHarness"));
            root.SetActive(false);

            Transform moduleRoot = new GameObject("ModuleRoot").transform;
            moduleRoot.SetParent(root.transform, false);

            ShowcaseStateHub stateHub = root.AddComponent<ShowcaseStateHub>();
            ShowcaseRuntimeController runtimeController = root.AddComponent<ShowcaseRuntimeController>();
            ShowcaseCommandRouter commandRouter = root.AddComponent<ShowcaseCommandRouter>();
            ShowcaseTransitionController transitionController = root.AddComponent<ShowcaseTransitionController>();
            ShowcaseCompositionRoot compositionRoot = root.AddComponent<ShowcaseCompositionRoot>();

            compositionRoot.Modules = modules;
            compositionRoot.ModuleRoot = moduleRoot;
            compositionRoot.RuntimeController = runtimeController;
            compositionRoot.CommandRouter = commandRouter;
            compositionRoot.TransitionController = transitionController;
            compositionRoot.StateHub = stateHub;

            for (int moduleIndex = 0; moduleIndex < modules.Length; moduleIndex++)
            {
                VariantDefinitionSO[] variants = modules[moduleIndex].Variants;
                for (int variantIndex = 0; variantIndex < variants.Length; variantIndex++)
                {
                    GameObject template = CreateVariantTemplate(
                        templateRoot.transform,
                        variants[variantIndex].name,
                        GetVisibleCount(variants[variantIndex]));
                    SetField(variants[variantIndex], "prefab", template);
                }
            }

            root.SetActive(true);
            return new RuntimeHarness(runtimeController, stateHub, moduleRoot, modules);
        }

        private GameObject CreateVariantTemplate(Transform parent, string variantId, int visibleCount)
        {
            GameObject template = new GameObject(variantId + "Template");
            template.transform.SetParent(parent, false);

            SmokeRuntimeVariant variant = template.AddComponent<SmokeRuntimeVariant>();
            variant.VariantId = variantId;
            variant.VisibleCount = visibleCount;

            return template;
        }

        private ModuleDefinitionSO CreateModule(string name, params VariantDefinitionSO[] variants)
        {
            ModuleDefinitionSO module = Track(ScriptableObject.CreateInstance<ModuleDefinitionSO>());
            module.name = name;
            SetField(module, "moduleName", name);
            SetField(module, "moduleNameRu", name + " RU");
            SetField(module, "thesis", "Module thesis");
            SetField(module, "thesisRu", "Module thesis RU");
            SetField(module, "description", "Module description");
            SetField(module, "descriptionRu", "Module description RU");
            SetField(module, "problemStatement", "Module problem");
            SetField(module, "problemStatementRu", "Module problem RU");
            SetField(module, "activeItemLabel", "Active items");
            SetField(module, "activeItemLabelRu", "Active items RU");
            SetField(module, "webGlPresetNote", "WebGL note");
            SetField(module, "webGlPresetNoteRu", "WebGL note RU");
            SetField(module, "variants", variants);
            return module;
        }

        private VariantDefinitionSO CreateVariant(string name, int visibleCount, params int[] stressPresets)
        {
            VariantDefinitionSO variant = Track(ScriptableObject.CreateInstance<VariantDefinitionSO>());
            variant.name = name;
            SetField(variant, "variantName", name);
            SetField(variant, "variantNameRu", name + " RU");
            SetField(variant, "prefab", null);
            SetField(variant, "stressPresets", stressPresets);
            SetField(variant, "stressPresetLabels", CreateLabels(stressPresets, "EN"));
            SetField(variant, "stressPresetLabelsRu", CreateLabels(stressPresets, "RU"));
            SetField(variant, "architectureDescription", "Architecture");
            SetField(variant, "architectureDescriptionRu", "Architecture RU");
            SetField(variant, "dataFlow", "Input -> Simulation -> View");
            SetField(variant, "dataFlowRu", "Input -> Simulation -> View RU");
            SetField(variant, "runtimeLifecycle", "Bootstrap -> Simulate -> Release");
            SetField(variant, "runtimeLifecycleRu", "Bootstrap -> Simulate -> Release RU");
            SetField(variant, "whyThisApproach", "Clear ownership");
            SetField(variant, "whyThisApproachRu", "Clear ownership RU");
            SetField(variant, "compareSummary", "Stable comparison");
            SetField(variant, "compareSummaryRu", "Stable comparison RU");
            SetField(variant, "takeaway", "Useful takeaway");
            SetField(variant, "takeawayRu", "Useful takeaway RU");
            SetField(variant, "tradeOffs", "Known trade-offs");
            SetField(variant, "tradeOffsRu", "Known trade-offs RU");
            SetField(variant, "pros", "- Predictable");
            SetField(variant, "prosRu", "- Predictable RU");
            SetField(variant, "cons", "- Synthetic runtime");
            SetField(variant, "consRu", "- Synthetic runtime RU");
            SetField(variant, "localizationKey", name.ToLowerInvariant());
            variantVisibleCounts[variant] = visibleCount;
            return variant;
        }

        private static IEnumerator WaitForRuntime()
        {
            yield return null;
            yield return null;
        }

        private static string[] CreateLabels(int[] stressPresets, string prefix)
        {
            string[] labels = new string[stressPresets.Length];
            for (int i = 0; i < stressPresets.Length; i++)
                labels[i] = prefix + " " + stressPresets[i];

            return labels;
        }

        private int GetVisibleCount(VariantDefinitionSO variant)
        {
            return variant != null && variantVisibleCounts.TryGetValue(variant, out int visibleCount) ? visibleCount : 1;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(target.GetType().FullName, fieldName);

            field.SetValue(target, value);
        }

        private T Track<T>(T createdObject) where T : UnityEngine.Object
        {
            createdObjects.Add(createdObject);
            return createdObject;
        }

        private readonly struct RuntimeHarness
        {
            public RuntimeHarness(
                ShowcaseRuntimeController runtimeController,
                ShowcaseStateHub stateHub,
                Transform moduleRoot,
                ModuleDefinitionSO[] modules)
            {
                RuntimeController = runtimeController;
                StateHub = stateHub;
                ModuleRoot = moduleRoot;
                Modules = modules;
            }

            public ShowcaseRuntimeController RuntimeController { get; }
            public ShowcaseStateHub StateHub { get; }
            public Transform ModuleRoot { get; }
            public ModuleDefinitionSO[] Modules { get; }
        }
    }

    internal sealed class SmokeRuntimeVariant : MonoBehaviour, IModule, IShowcaseStressTarget, IShowcaseMetricsSource
    {
        private static readonly Dictionary<string, int> EnterCounts = new();
        private static readonly Dictionary<string, int> ExitCounts = new();
        private static readonly Dictionary<string, int> LastStressLevels = new();

        public string VariantId = "Variant";
        public int VisibleCount = 1;

        public int ActiveCount => VisibleCount;

        public void Enter()
        {
            Increment(EnterCounts, VariantId);
        }

        public void Exit()
        {
            Increment(ExitCounts, VariantId);
        }

        public void SetStressLevel(int count)
        {
            LastStressLevels[VariantId] = count;
        }

        public ShowcaseMetricsSnapshot GetMetricsSnapshot()
        {
            return new ShowcaseMetricsSnapshot(GetLastStress(VariantId), VisibleCount, 0.05f);
        }

        public static void Reset()
        {
            EnterCounts.Clear();
            ExitCounts.Clear();
            LastStressLevels.Clear();
        }

        public static int GetEnterCount(string variantId)
        {
            return EnterCounts.TryGetValue(variantId, out int count) ? count : 0;
        }

        public static int GetExitCount(string variantId)
        {
            return ExitCounts.TryGetValue(variantId, out int count) ? count : 0;
        }

        public static int GetLastStress(string variantId)
        {
            return LastStressLevels.TryGetValue(variantId, out int count) ? count : 0;
        }

        private static void Increment(Dictionary<string, int> counters, string variantId)
        {
            counters[variantId] = counters.TryGetValue(variantId, out int count) ? count + 1 : 1;
        }
    }
}
