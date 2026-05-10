using System;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.EditorTools;
using LearningArchitect.Modules.Animation3D;
using LearningArchitect.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ShowcaseValidatorTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void ValidateDefinitions_ReportsErrorsAndWarnings_ForMissingPrefabContracts()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = new GameObject("VariantPrefab");
                prefab.AddComponent<ModuleOnlyRuntimeComponent>();
                variant = CreateVariant("VariantContracts", prefab, 1000, 2000, 4000);
                module = CreateModule("ModuleContracts", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("IShowcaseStressTarget")));
                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("IShowcaseMetricsSource")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenStressPresetLabelsMismatchPresetCount()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("VariantPrefab");
                variant = CreateVariant("VariantLabels", prefab, 500, 1000, 2000);
                SetField(variant, "stressPresetLabels", new[] { "Low", "Mid" });
                module = CreateModule("ModuleLabels", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("stress preset labels do not match")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenRequiredArchitectureContentIsMissing()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("VariantPrefab");
                variant = CreateVariant("VariantContent", prefab, 1000);
                SetField(variant, "architectureDescription", string.Empty);
                SetField(variant, "architectureDescriptionRu", string.Empty);
                SetField(variant, "dataFlow", string.Empty);
                SetField(variant, "dataFlowRu", string.Empty);
                module = CreateModule("ModuleContent", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("architecture description is missing English content")));
                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("data flow is missing Russian content")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsWarnings_ForDuplicateAndOrphanVariants()
        {
            VariantDefinitionSO sharedVariant = null;
            VariantDefinitionSO orphanVariant = null;
            ModuleDefinitionSO module = null;
            GameObject sharedPrefab = null;
            GameObject orphanPrefab = null;

            try
            {
                sharedPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("SharedPrefab");
                orphanPrefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("OrphanPrefab");
                sharedVariant = CreateVariant("SharedVariant", sharedPrefab, 1000, 2000, 3000);
                orphanVariant = CreateVariant("OrphanVariant", orphanPrefab, 1000, 2000, 3000);
                module = CreateModule("ModuleDuplicate", sharedVariant, sharedVariant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { sharedVariant, orphanVariant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("more than once")));
                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("not referenced by any module")));
            }
            finally
            {
                DestroyImmediateSafe(sharedPrefab);
                DestroyImmediateSafe(orphanPrefab);
                DestroyImmediateSafe(sharedVariant);
                DestroyImmediateSafe(orphanVariant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenAnimationProfileIsMissing()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = new GameObject("HumanoidVariantPrefab");
                prefab.AddComponent<ModuleOnlyRuntimeComponent>();
                prefab.AddComponent<HumanoidAnimationVariant>();
                variant = CreateVariant("HumanoidVariantMissingProfile", prefab, 1000, 2000, 4000);
                module = CreateModule("HumanoidModuleMissingProfile", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("requires an assigned animationProfile")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenAnimationProfileHasNoActorPrefab()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;
            HumanoidAnimationProfileSO profile = null;

            try
            {
                prefab = new GameObject("HumanoidVariantPrefab");
                prefab.AddComponent<ModuleOnlyRuntimeComponent>();
                HumanoidAnimationVariant animationVariant = prefab.AddComponent<HumanoidAnimationVariant>();
                profile = ScriptableObject.CreateInstance<HumanoidAnimationProfileSO>();
                profile.name = "HumanoidProfileWithoutActor";

                SerializedObject serializedVariant = new(animationVariant);
                serializedVariant.FindProperty("animationProfile").objectReferenceValue = profile;
                serializedVariant.ApplyModifiedPropertiesWithoutUndo();

                variant = CreateVariant("HumanoidVariantNoActor", prefab, 1000, 2000, 4000);
                module = CreateModule("HumanoidModuleNoActor", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("requires an assigned actorPrefab")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(profile);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenAnimationProfileHasNoAvatar()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;
            GameObject actorPrefab = null;
            HumanoidAnimationProfileSO profile = null;

            try
            {
                prefab = new GameObject("HumanoidVariantPrefab");
                prefab.AddComponent<ModuleOnlyRuntimeComponent>();
                HumanoidAnimationVariant animationVariant = prefab.AddComponent<HumanoidAnimationVariant>();
                actorPrefab = CreateActorPrefab(withAnimator: true, withCrowdActor: true);
                profile = CreateHumanoidProfile(actorPrefab, includeAvatar: false, includeAnimatorController: true);

                SerializedObject serializedVariant = new(animationVariant);
                serializedVariant.FindProperty("animationProfile").objectReferenceValue = profile;
                serializedVariant.ApplyModifiedPropertiesWithoutUndo();

                variant = CreateVariant("HumanoidVariantNoAvatar", prefab, 1000, 2000, 4000);
                module = CreateModule("HumanoidModuleNoAvatar", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("requires an assigned avatar")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(actorPrefab);
                DestroyImmediateSafe(profile);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenAnimationProfileHasNoAnimatorController()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;
            GameObject actorPrefab = null;
            HumanoidAnimationProfileSO profile = null;

            try
            {
                prefab = new GameObject("HumanoidVariantPrefab");
                prefab.AddComponent<ModuleOnlyRuntimeComponent>();
                HumanoidAnimationVariant animationVariant = prefab.AddComponent<HumanoidAnimationVariant>();
                actorPrefab = CreateActorPrefab(withAnimator: true, withCrowdActor: true);
                profile = CreateHumanoidProfile(actorPrefab, includeAvatar: true, includeAnimatorController: false);

                SerializedObject serializedVariant = new(animationVariant);
                serializedVariant.FindProperty("animationProfile").objectReferenceValue = profile;
                serializedVariant.ApplyModifiedPropertiesWithoutUndo();

                variant = CreateVariant("HumanoidVariantNoController", prefab, 1000, 2000, 4000);
                module = CreateModule("HumanoidModuleNoController", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("requires an assigned animatorController")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(actorPrefab);
                DestroyImmediateSafe(profile);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenAnimationProfileActorPrefabHasNoAnimator()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;
            GameObject actorPrefab = null;
            HumanoidAnimationProfileSO profile = null;

            try
            {
                prefab = new GameObject("HumanoidVariantPrefab");
                prefab.AddComponent<ModuleOnlyRuntimeComponent>();
                HumanoidAnimationVariant animationVariant = prefab.AddComponent<HumanoidAnimationVariant>();
                actorPrefab = CreateActorPrefab(withAnimator: false, withCrowdActor: true);
                profile = CreateHumanoidProfile(actorPrefab, includeAvatar: true, includeAnimatorController: true);

                SerializedObject serializedVariant = new(animationVariant);
                serializedVariant.FindProperty("animationProfile").objectReferenceValue = profile;
                serializedVariant.ApplyModifiedPropertiesWithoutUndo();

                variant = CreateVariant("HumanoidVariantActorNoAnimator", prefab, 1000, 2000, 4000);
                module = CreateModule("HumanoidModuleActorNoAnimator", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("requires an Animator component in children")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(actorPrefab);
                DestroyImmediateSafe(profile);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateHubPrefabLayout_AllowsCanonicalLegacyDescriptionLayout()
        {
            GameObject hubPrefab = null;

            try
            {
                hubPrefab = CreateHubPrefabForValidation(includeDescriptionText: true, includeTabsRoot: true);
                ShowcaseValidationReport report = new();

                ShowcaseValidator.ValidateHubPrefabLayout(hubPrefab, report);

                Assert.That(report.ErrorCount, Is.Zero);
            }
            finally
            {
                DestroyImmediateSafe(hubPrefab);
            }
        }

        [Test]
        public void ValidateHubPrefabLayout_ReportsError_WhenTabsRootIsMissing()
        {
            GameObject hubPrefab = null;

            try
            {
                hubPrefab = CreateHubPrefabForValidation(includeDescriptionText: true, includeTabsRoot: false);
                ShowcaseValidationReport report = new();

                ShowcaseValidator.ValidateHubPrefabLayout(hubPrefab, report);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("Layout - DescriptionTabs")));
            }
            finally
            {
                DestroyImmediateSafe(hubPrefab);
            }
        }

        [Test]
        public void ValidateHubPrefabLayout_ReportsError_WhenDescriptionTextIsMissing()
        {
            GameObject hubPrefab = null;

            try
            {
                hubPrefab = CreateHubPrefabForValidation(includeDescriptionText: false, includeTabsRoot: true);
                ShowcaseValidationReport report = new();

                ShowcaseValidator.ValidateHubPrefabLayout(hubPrefab, report);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("Text - Description")));
            }
            finally
            {
                DestroyImmediateSafe(hubPrefab);
            }
        }

        [Test]
        public void ValidateHubSceneInstanceLayout_AllowsOriginAlignedHubAndModuleRoot()
        {
            GameObject hubInstance = null;

            try
            {
                hubInstance = CreateHubSceneInstance();
                ShowcaseValidationReport report = new();

                ShowcaseValidator.ValidateHubSceneInstanceLayout(hubInstance, report);

                Assert.That(report.ErrorCount, Is.Zero);
            }
            finally
            {
                DestroyImmediateSafe(hubInstance);
            }
        }

        [Test]
        public void ValidateHubSceneInstanceLayout_ReportsError_WhenHubIsOffsetFromOrigin()
        {
            GameObject hubInstance = null;

            try
            {
                hubInstance = CreateHubSceneInstance();
                hubInstance.transform.localPosition = new Vector3(-75.55f, -20.84f, 0f);
                ShowcaseValidationReport report = new();

                ShowcaseValidator.ValidateHubSceneInstanceLayout(hubInstance, report);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("Scene hub 'ArchitectureShowcaseHub' must stay at the world origin")));
            }
            finally
            {
                DestroyImmediateSafe(hubInstance);
            }
        }

        [Test]
        public void ValidateHubSceneInstanceLayout_ReportsError_WhenModuleRootIsOffset()
        {
            GameObject hubInstance = null;

            try
            {
                hubInstance = CreateHubSceneInstance();
                Transform moduleRoot = hubInstance.transform.Find("ModuleRoot");
                moduleRoot.localPosition = new Vector3(2f, 0f, 0f);
                ShowcaseValidationReport report = new();

                ShowcaseValidator.ValidateHubSceneInstanceLayout(hubInstance, report);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("Scene ModuleRoot must stay aligned with the hub origin")));
            }
            finally
            {
                DestroyImmediateSafe(hubInstance);
            }
        }

        private static ModuleDefinitionSO CreateModule(string assetName, params VariantDefinitionSO[] variants)
        {
            ModuleDefinitionSO module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();
            module.name = assetName;
            SetField(module, "moduleName", assetName);
            SetField(module, "moduleNameRu", assetName + " RU");
            SetField(module, "thesis", "Module thesis");
            SetField(module, "thesisRu", "Тезис модуля");
            SetField(module, "description", "Module description");
            SetField(module, "descriptionRu", "Описание модуля");
            SetField(module, "problemStatement", "Module problem");
            SetField(module, "problemStatementRu", "Проблема модуля");
            SetField(module, "activeItemLabel", "Active agents");
            SetField(module, "activeItemLabelRu", "Активные агенты");
            SetField(module, "webGlPresetNote", "WebGL note");
            SetField(module, "webGlPresetNoteRu", "Заметка WebGL");
            SetField(module, "variants", variants);
            return module;
        }

        private static VariantDefinitionSO CreateVariant(string assetName, GameObject prefab, params int[] stressPresets)
        {
            VariantDefinitionSO variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();
            variant.name = assetName;
            SetField(variant, "variantName", assetName);
            SetField(variant, "variantNameRu", assetName + " RU");
            SetField(variant, "prefab", prefab);
            SetField(variant, "stressPresets", stressPresets);
            SetField(variant, "architectureDescription", "Architecture body");
            SetField(variant, "architectureDescriptionRu", "Архитектура");
            SetField(variant, "dataFlow", "Input -> Simulation -> View");
            SetField(variant, "dataFlowRu", "Ввод -> Симуляция -> Представление");
            SetField(variant, "runtimeLifecycle", "Bootstrap, simulate, release");
            SetField(variant, "runtimeLifecycleRu", "Запуск, симуляция, освобождение");
            SetField(variant, "whyThisApproach", "Clear ownership");
            SetField(variant, "whyThisApproachRu", "Явное владение");
            SetField(variant, "compareSummary", "Compares well against the naive baseline.");
            SetField(variant, "compareSummaryRu", "Хорошо сравнивается с наивной базой.");
            SetField(variant, "takeaway", "Use when coordination matters.");
            SetField(variant, "takeawayRu", "Использовать, когда важна координация.");
            SetField(variant, "tradeOffs", "More setup, less per-frame duplication.");
            SetField(variant, "tradeOffsRu", "Больше настройки, меньше дублирования в кадре.");
            SetField(variant, "pros", "- Stable");
            SetField(variant, "prosRu", "- Стабильно");
            SetField(variant, "cons", "- More authoring discipline");
            SetField(variant, "consRu", "- Требует дисциплины при наполнении");
            return variant;
        }

        private static HumanoidAnimationProfileSO CreateHumanoidProfile(GameObject actorPrefab, bool includeAvatar, bool includeAnimatorController)
        {
            HumanoidAnimationProfileSO template = AssetDatabase.LoadAssetAtPath<HumanoidAnimationProfileSO>(
                "Assets/Modules/LayeredCharacterAnimation/Data/Animation_PaladinCombatProfile.asset");
            Assert.IsNotNull(template, "Could not load Animation_PaladinCombatProfile.asset for validator tests.");

            HumanoidAnimationProfileSO profile = ScriptableObject.CreateInstance<HumanoidAnimationProfileSO>();
            profile.name = "HumanoidProfileForValidation";
            SetField(profile, "actorPrefab", actorPrefab);
            SetField(profile, "avatar", includeAvatar ? template.Avatar : null);
            SetField(profile, "animatorController", includeAnimatorController ? template.AnimatorController : null);
            return profile;
        }

        private static GameObject CreateActorPrefab(bool withAnimator, bool withCrowdActor)
        {
            GameObject actorPrefab = new("HumanoidActor");
            if (withAnimator)
                actorPrefab.AddComponent<Animator>();

            if (withCrowdActor)
                actorPrefab.AddComponent<HumanoidCrowdActor>();

            return actorPrefab;
        }


        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(target.GetType().FullName, fieldName);

            field.SetValue(target, value);
        }

        private static void DestroyImmediateSafe(UnityEngine.Object target)
        {
            if (target != null)
                UnityEngine.Object.DestroyImmediate(target);
        }

        private static GameObject CreateHubPrefabForValidation(bool includeDescriptionText, bool includeTabsRoot)
        {
            GameObject root = new("ArchitectureShowcaseHub", typeof(RectTransform), typeof(DescriptionPanel), typeof(ScrollRect));
            DescriptionPanel panel = root.GetComponent<DescriptionPanel>();
            ScrollRect scrollRect = root.GetComponent<ScrollRect>();

            if (includeTabsRoot)
            {
                GameObject tabsBar = CreateNode("Layout - DescriptionTabs", root.transform);
                CreateNode("Image - ActiveTabUnderline", tabsBar.transform);
            }

            GameObject viewportObject = CreateNode("Container - Viewport", root.transform, typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            scrollRect.viewport = viewport;
            panel.ScrollRect = scrollRect;

            if (includeDescriptionText)
                CreateSection("Text - Description", viewport.transform, includeTextChildren: false);

            return root;
        }

        private static GameObject CreateHubSceneInstance()
        {
            GameObject hub = new(ShowcaseValidator.HubRootName);
            CreateNode("ModuleRoot", hub.transform);
            return hub;
        }

        private static GameObject CreateSection(string sectionName, Transform parent, bool includeTextChildren)
        {
            GameObject section = CreateNode(sectionName, parent);
            if (!includeTextChildren)
                return section;

            CreateNode("Text - PerformanceHeader", section.transform);
            CreateNode("Text - Description", section.transform);
            return section;
        }

        private static GameObject CreateNode(string name, Transform parent, params Type[] components)
        {
            Type[] nodeComponents = components == null || components.Length == 0
                ? new[] { typeof(RectTransform) }
                : components;

            GameObject node = new(name, nodeComponents);
            node.transform.SetParent(parent, false);
            return node;
        }
    }

    internal sealed class ModuleOnlyRuntimeComponent : MonoBehaviour, IModule
    {
        public void Enter()
        {
        }

        public void Exit()
        {
        }
    }
}
