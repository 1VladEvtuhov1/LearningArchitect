using System;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.EditorTools;
using LearningArchitect.Modules.Animation3D;
using LearningArchitect.UI;
using NUnit.Framework;
using TMPro;
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
        public void ValidateDefinitions_ReportsErrors_WhenShowcaseContentTablesUnavailable()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("VariantPrefab");
                variant = CreateVariant("VariantContent", prefab, 1000);
                module = CreateModule("ModuleContent", variant);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue =>
                    issue.Message.Contains("missing English string table for 'ShowcaseContent'")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsErrors_WhenSyntheticKeysMissingFromShowcaseContent()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("VariantPrefab");
                variant = CreateVariant("VariantMissingTableEntry", prefab, 1000, 2000, 3000);
                module = CreateModule("ModuleMissingTableEntry", variant);
                SetField(module, "localizationKey", "validator.synthetic_missing_module");
                SetField(variant, "localizationKey", "validator.synthetic_missing_variant");

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseValidator.CaptureLocalizationSnapshot());

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue =>
                    issue.Message.Contains("Module description is missing English ShowcaseContent entry")));
                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue =>
                    issue.Message.Contains("Variant architecture description is missing Russian ShowcaseContent entry")));
            }
            finally
            {
                DestroyImmediateSafe(prefab);
                DestroyImmediateSafe(variant);
                DestroyImmediateSafe(module);
            }
        }

        [Test]
        public void ValidateDefinitions_ReportsError_WhenLocalizationKeysAreMissing()
        {
            VariantDefinitionSO variant = null;
            ModuleDefinitionSO module = null;
            GameObject prefab = null;

            try
            {
                prefab = ShowcaseCoreTestFactory.CreateRuntimePrefab("VariantPrefab");
                variant = CreateVariant("VariantWithoutLocalizationKey", prefab, 1000, 2000, 3000);
                module = CreateModule("ModuleWithoutLocalizationKey", variant);
                SetField(module, "localizationKey", string.Empty);
                SetField(variant, "localizationKey", string.Empty);

                ShowcaseValidationReport report = ShowcaseValidator.ValidateDefinitions(
                    new[] { module },
                    new[] { variant },
                    ShowcaseLocalizationSnapshot.Empty);

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(
                    issue => issue.Message.Contains("ModuleDefinitionSO requires an explicit localizationKey.")));
                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(
                    issue => issue.Message.Contains("VariantDefinitionSO requires an explicit localizationKey.")));
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

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("tabsBar")));
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

                Assert.That(report.Issues, Has.Some.Matches<ShowcaseValidationIssue>(issue => issue.Message.Contains("legacyDescriptionText")));
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
            SetField(module, "localizationKey", assetName.ToLowerInvariant());
            SetField(module, "variants", variants);
            return module;
        }

        private static VariantDefinitionSO CreateVariant(string assetName, GameObject prefab, params int[] stressPresets)
        {
            VariantDefinitionSO variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();
            variant.name = assetName;
            SetField(variant, "localizationKey", assetName.ToLowerInvariant());
            SetField(variant, "prefab", prefab);
            SetField(variant, "stressPresets", stressPresets);
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
            GameObject root = new(
                "ArchitectureShowcaseHub",
                typeof(RectTransform),
                typeof(DescriptionPanel),
                typeof(ScrollRect),
                typeof(HubUI),
                typeof(ModuleNavigationControls),
                typeof(StressTestControlsView),
                typeof(StressTestControls),
                typeof(ShowcaseLocalization));

            DescriptionPanel panel = root.GetComponent<DescriptionPanel>();
            ScrollRect scrollRect = root.GetComponent<ScrollRect>();
            HubUI hubUi = root.GetComponent<HubUI>();
            ModuleNavigationControls navigation = root.GetComponent<ModuleNavigationControls>();
            StressTestControlsView stressView = root.GetComponent<StressTestControlsView>();
            StressTestControls stressControls = root.GetComponent<StressTestControls>();
            ShowcaseLocalization localization = root.GetComponent<ShowcaseLocalization>();

            GameObject tabsBarObject = includeTabsRoot
                ? CreateNode("Layout - DescriptionTabs", root.transform)
                : CreateNode("Layout - DescriptionTabs Missing", root.transform);
            RectTransform tabsBar = includeTabsRoot ? tabsBarObject.GetComponent<RectTransform>() : null;
            GameObject activeUnderlineObject = CreateNode("Image - ActiveTabUnderline", tabsBarObject.transform, typeof(RectTransform), typeof(Image));
            RectTransform activeUnderline = activeUnderlineObject.GetComponent<RectTransform>();

            TextMeshProUGUI overviewTabLabel = CreateTextNode("Text - OverviewTab", tabsBarObject.transform);
            TextMeshProUGUI architectureTabLabel = CreateTextNode("Text - ArchitectureTab", tabsBarObject.transform);
            TextMeshProUGUI tradeOffsTabLabel = CreateTextNode("Text - TradeOffsTab", tabsBarObject.transform);

            GameObject viewportObject = CreateNode("Container - Viewport", root.transform, typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            scrollRect.viewport = viewport;
            panel.ScrollRect = scrollRect;

            TextMeshProUGUI descriptionText = includeDescriptionText
                ? CreateTextNode("Text - Description", viewport.transform)
                : CreateTextNode("Text - Description Missing", viewport.transform);

            SetField(panel, "tabsBar", tabsBar);
            SetField(panel, "activeTabUnderline", activeUnderline);
            SetField(panel, "legacyDescriptionText", includeDescriptionText ? descriptionText : null);
            SetField(panel, "overviewTabLabel", overviewTabLabel);
            SetField(panel, "architectureTabLabel", architectureTabLabel);
            SetField(panel, "tradeOffsTabLabel", tradeOffsTabLabel);

            TextMeshProUGUI moduleName = CreateTextNode("Text - ModuleName", root.transform);
            TextMeshProUGUI variantName = CreateTextNode("Text - VariantName", root.transform);
            TextMeshProUGUI moduleSelectorName = CreateTextNode("Text - ModuleSelectorName", root.transform);
            TextMeshProUGUI variantSelectorName = CreateTextNode("Text - VariantSelectorName", root.transform);
            RectTransform moduleStatsContainer = CreateNode("Container - ModuleStats", root.transform).GetComponent<RectTransform>();
            SetField(hubUi, "moduleName", moduleName);
            SetField(hubUi, "variantName", variantName);
            SetField(hubUi, "moduleSelectorName", moduleSelectorName);
            SetField(hubUi, "variantSelectorName", variantSelectorName);
            SetField(hubUi, "moduleStatsContainer", moduleStatsContainer);
            SetField(hubUi, "moduleStats", CreateModuleStatBindings(moduleStatsContainer));

            Button previousModuleButton = CreateButtonNode("Button - PrevModule", root.transform);
            Button nextModuleButton = CreateButtonNode("Button - NextModule", root.transform);
            Button previousVariantButton = CreateButtonNode("Button - PrevVariant", root.transform);
            Button nextVariantButton = CreateButtonNode("Button - NextVariant", root.transform);
            Button moduleSelectorButton = CreateButtonNode("Button - ModuleSelector", root.transform);
            Button variantSelectorButton = CreateButtonNode("Button - VariantSelector", root.transform);
            SetField(navigation, "_previousModuleButton", previousModuleButton);
            SetField(navigation, "_previousModuleLabel", CreateButtonLabel(previousModuleButton.transform, "‹"));
            SetField(navigation, "_nextModuleButton", nextModuleButton);
            SetField(navigation, "_nextModuleLabel", CreateButtonLabel(nextModuleButton.transform, "›"));
            SetField(navigation, "_previousVariantButton", previousVariantButton);
            SetField(navigation, "_previousVariantLabel", CreateButtonLabel(previousVariantButton.transform, "‹"));
            SetField(navigation, "_nextVariantButton", nextVariantButton);
            SetField(navigation, "_nextVariantLabel", CreateButtonLabel(nextVariantButton.transform, "›"));
            SetField(navigation, "_moduleSelectorButton", moduleSelectorButton);
            SetField(navigation, "_variantSelectorButton", variantSelectorButton);

            SetField(stressView, "_presetButtons", CreateStressPresetBindings(root.transform));
            SetField(stressControls, "_view", stressView);

            Button languageToggleButton = CreateButtonNode("Button - LanguageToggle", root.transform);
            SetField(localization, "languageToggleButton", languageToggleButton);
            SetField(localization, "languageToggleLabel", CreateButtonLabel(languageToggleButton.transform, "EN"));
            SetField(localization, "breadcrumbText", CreateTextNode("Text - Breadcrumb", root.transform));
            SetField(localization, "moduleHeaderText", CreateTextNode("Text - ModuleHeader", root.transform));
            SetField(localization, "variantHeaderText", CreateTextNode("Text - VariantHeader", root.transform));
            SetField(localization, "stressHeaderText", CreateTextNode("Text - StressHeader", root.transform));
            SetField(localization, "statusTitleText", CreateTextNode("Text - StatusTitle", root.transform));
            SetField(localization, "overviewTabText", overviewTabLabel);
            SetField(localization, "architectureTabText", architectureTabLabel);
            SetField(localization, "tradeOffsTabText", tradeOffsTabLabel);

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

        private static TextMeshProUGUI CreateTextNode(string name, Transform parent)
        {
            GameObject node = CreateNode(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            return node.GetComponent<TextMeshProUGUI>();
        }

        private static Button CreateButtonNode(string name, Transform parent)
        {
            GameObject node = CreateNode(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            return node.GetComponent<Button>();
        }

        private static TextMeshProUGUI CreateButtonLabel(Transform buttonTransform, string text)
        {
            TextMeshProUGUI label = CreateTextNode("Text - Label", buttonTransform);
            label.text = text;
            return label;
        }

        private static Array CreateModuleStatBindings(Transform moduleStatsRoot)
        {
            Type bindingType = typeof(HubUI).GetNestedType("ModuleStatBinding", BindingFlags.NonPublic);
            Assert.IsNotNull(bindingType, "HubUI.ModuleStatBinding was not found.");

            Array bindings = Array.CreateInstance(bindingType, 5);
            for (int i = 0; i < 5; i++)
            {
                GameObject statRootObject = CreateNode("Container - ModuleStat (" + i + ")", moduleStatsRoot);
                RectTransform statRoot = statRootObject.GetComponent<RectTransform>();
                TextMeshProUGUI label = CreateTextNode("Text - ModuleStatLabel", statRootObject.transform);
                TextMeshProUGUI value = CreateTextNode("Text - ModuleStatValue", statRootObject.transform);
                object binding = Activator.CreateInstance(bindingType);
                bindingType.GetField("root").SetValue(binding, statRoot);
                bindingType.GetField("label").SetValue(binding, label);
                bindingType.GetField("value").SetValue(binding, value);
                bindings.SetValue(binding, i);
            }

            return bindings;
        }

        private static Array CreateStressPresetBindings(Transform parent)
        {
            Type bindingType = typeof(StressTestControlsView).GetNestedType("PresetButtonView", BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(bindingType, "StressTestControlsView.PresetButtonView was not found.");

            Array bindings = Array.CreateInstance(bindingType, 3);
            for (int i = 0; i < 3; i++)
            {
                Button button = CreateButtonNode("Button - StressPreset (" + i + ")", parent);
                TextMeshProUGUI label = CreateButtonLabel(button.transform, "Preset " + i);
                object binding = Activator.CreateInstance(bindingType);
                bindingType.GetField("_button", PrivateInstance).SetValue(binding, button);
                bindingType.GetField("_label", PrivateInstance).SetValue(binding, label);
                bindings.SetValue(binding, i);
            }

            return bindings;
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
