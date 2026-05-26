using System;
using System.Collections.Generic;
using LearningArchitect.Core;
using LearningArchitect.Modules.Animation3D;
using LearningArchitect.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace LearningArchitect.EditorTools
{
    internal enum ShowcaseValidationSeverity
    {
        Warning = 0,
        Error = 1
    }

    internal readonly struct ShowcaseValidationIssue
    {
        public ShowcaseValidationIssue(ShowcaseValidationSeverity severity, string message, UnityEngine.Object context)
        {
            Severity = severity;
            Message = message;
            Context = context;
        }

        public ShowcaseValidationSeverity Severity { get; }
        public string Message { get; }
        public UnityEngine.Object Context { get; }
    }

    internal sealed class ShowcaseValidationReport
    {
        private readonly List<ShowcaseValidationIssue> issues = new();

        public IReadOnlyList<ShowcaseValidationIssue> Issues => issues;
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public bool IsValid => ErrorCount == 0;

        public void AddError(string message, UnityEngine.Object context)
        {
            issues.Add(new ShowcaseValidationIssue(ShowcaseValidationSeverity.Error, message, context));
            ErrorCount++;
        }

        public void AddWarning(string message, UnityEngine.Object context)
        {
            issues.Add(new ShowcaseValidationIssue(ShowcaseValidationSeverity.Warning, message, context));
            WarningCount++;
        }
    }

    internal readonly struct ShowcaseLocalizationSnapshot
    {
        public ShowcaseLocalizationSnapshot(
            StringTable englishUiTable,
            StringTable russianUiTable,
            StringTable englishContentTable,
            StringTable russianContentTable,
            bool hasUiCollection,
            bool hasContentCollection)
        {
            EnglishUiTable = englishUiTable;
            RussianUiTable = russianUiTable;
            EnglishContentTable = englishContentTable;
            RussianContentTable = russianContentTable;
            HasUiCollection = hasUiCollection;
            HasContentCollection = hasContentCollection;
        }

        public static ShowcaseLocalizationSnapshot Empty =>
            new(null, null, null, null, false, false);

        public StringTable EnglishUiTable { get; }
        public StringTable RussianUiTable { get; }
        public StringTable EnglishContentTable { get; }
        public StringTable RussianContentTable { get; }
        public bool HasUiCollection { get; }
        public bool HasContentCollection { get; }
        public bool HasEnglishUiTable => EnglishUiTable != null;
        public bool HasRussianUiTable => RussianUiTable != null;
        public bool HasEnglishContentTable => EnglishContentTable != null;
        public bool HasRussianContentTable => RussianContentTable != null;
    }

    public static class ShowcaseValidator
    {
        internal const string HubPrefabPath = "Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab";
        internal const string ShowcaseScenePath = "Assets/Showcase/Scenes/ArchitectureShowcase.unity";
        internal const string HubRootName = "ArchitectureShowcaseHub";
        private const float TransformTolerance = 0.001f;

        private static readonly string[] DefinitionSearchFolders =
        {
            "Assets/Showcase/Data",
            "Assets/Modules"
        };

        private static readonly string[] RequiredUiKeys =
        {
            "about",
            "active",
            "active_items",
            "architecture",
            "breadcrumb",
            "compare",
            "cons",
            "core_idea",
            "data_flow",
            "fps",
            "frame_time",
            "key_features",
            "loaded",
            "module",
            "module_category_architecture",
            "module_category_simulation",
            "module_group",
            "module_type",
            "na",
            "no_architecture_notes",
            "no_compare_summary",
            "no_cons",
            "no_constraints",
            "no_data_flow",
            "no_key_features",
            "no_module",
            "no_module_type",
            "no_problem_statement",
            "no_pros",
            "no_runtime_lifecycle",
            "no_strengths",
            "no_takeaway",
            "no_tradeoffs",
            "no_variant",
            "no_webgl_note",
            "no_why_this_approach",
            "orbit",
            "overview",
            "particles",
            "performance",
            "problem",
            "pros",
            "runtime_lifecycle",
            "select_module",
            "select_variant",
            "start_demo",
            "stop_demo",
            "strengths",
            "stress_test",
            "system_status",
            "takeaway",
            "tooltip_next_module",
            "tooltip_next_variant",
            "tooltip_prev_module",
            "tooltip_prev_variant",
            "trade_offs",
            "variant",
            "variants",
            "watch_out",
            "webgl_preset",
            "why_this_approach",
            "zoom"
        };

        [MenuItem("Tools/LearningArchitect/Validate Showcase Configuration")]
        public static void ValidateShowcaseConfiguration()
        {
            ModuleDefinitionSO[] modules = LoadAssets<ModuleDefinitionSO>();
            VariantDefinitionSO[] variants = LoadAssets<VariantDefinitionSO>();
            RecruiterDemoScenarioSO[] recruiterScenarios = LoadAssets<RecruiterDemoScenarioSO>();
            ShowcaseValidationReport report = ValidateShowcaseConfigurationInternal(modules, variants, recruiterScenarios);
            LogReport(report, modules.Length, variants.Length);
        }

        internal static ShowcaseValidationReport ValidateShowcaseConfigurationInternal(
            IReadOnlyList<ModuleDefinitionSO> modules,
            IReadOnlyList<VariantDefinitionSO> variants,
            IReadOnlyList<RecruiterDemoScenarioSO> recruiterScenarios)
        {
            ShowcaseLocalizationSnapshot localization = CaptureLocalizationSnapshot();
            ShowcaseValidationReport report = ValidateDefinitions(modules, variants, localization);
            ValidateRecruiterDemoScenarios(recruiterScenarios, modules, localization, report);
            ValidateHubPrefabLayout(AssetDatabase.LoadAssetAtPath<GameObject>(HubPrefabPath), report);
            ValidateHubSceneLayout(report);
            ValidateInterviewArenaBuildScenes(report);
            ValidateInterviewArenaScenePlayer(report);
            return report;
        }

        private static MonoBehaviour FindInterviewArenaBootstrapInScene()
        {
            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == "InterviewArenaRuntimeContext")
                    return behaviour;
            }

            return null;
        }

        internal static void ValidateInterviewArenaBuildScenes(ShowcaseValidationReport report)
        {
            const string architectureScenePath = "Assets/Showcase/Scenes/ArchitectureShowcase.unity";
            const string interviewArenaScenePath = "Assets/Modules/InterviewArena/Scenes/InterviewArena.unity";

            bool hasShowcase = false;
            bool hasArena = false;

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;

                if (scene.path == architectureScenePath)
                    hasShowcase = true;

                if (scene.path == interviewArenaScenePath)
                    hasArena = true;
            }

            if (!hasShowcase)
            {
                report.AddError(
                    $"Build settings must include enabled scene '{architectureScenePath}'.",
                    null);
            }

            if (!hasArena)
            {
                report.AddWarning(
                    $"Build settings do not include '{interviewArenaScenePath}'. " +
                    "Run Learning Architect/Interview Arena/Setup Interview Arena Scenes.",
                    null);
            }
        }

        internal static void ValidateInterviewArenaScenePlayer(ShowcaseValidationReport report)
        {
            const string arenaScenePath = "Assets/Modules/InterviewArena/Scenes/InterviewArena.unity";
            if (!System.IO.File.Exists(arenaScenePath))
                return;

            Scene existingScene = SceneManager.GetSceneByPath(arenaScenePath);
            Scene scene = existingScene.IsValid() && existingScene.isLoaded
                ? existingScene
                : EditorSceneManager.OpenScene(arenaScenePath, OpenSceneMode.Additive);

            bool openedForValidation = !existingScene.IsValid() || !existingScene.isLoaded;
            try
            {
                MonoBehaviour bootstrap = FindInterviewArenaBootstrapInScene();
                if (bootstrap == null)
                    return;

                SerializedObject serialized = new SerializedObject(bootstrap);
                if (serialized.FindProperty("player").objectReferenceValue == null)
                {
                    report.AddWarning(
                        "InterviewArenaRuntimeContext has no scene player assigned. Place a prefab instance and wire the Player field (no runtime spawn).",
                        bootstrap);
                }
            }
            finally
            {
                if (openedForValidation)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        internal static ShowcaseValidationReport ValidateDefinitions(
            IReadOnlyList<ModuleDefinitionSO> modules,
            IReadOnlyList<VariantDefinitionSO> variants,
            ShowcaseLocalizationSnapshot localization)
        {
            ShowcaseValidationReport report = new();

            if (modules == null || modules.Count == 0)
                report.AddError("No module definitions were found under Assets/Showcase/Data or Assets/Modules.", null);

            if (variants == null || variants.Count == 0)
                report.AddError("No variant definitions were found under Assets/Showcase/Data or Assets/Modules.", null);

            ValidateUiLocalization(localization, report);
            ValidateContentLocalizationAvailability(localization, report);

            HashSet<VariantDefinitionSO> referencedVariants = new();
            Dictionary<VariantDefinitionSO, ModuleDefinitionSO> ownership = new();

            if (modules != null)
            {
                for (int i = 0; i < modules.Count; i++)
                    ValidateModule(modules[i], localization, report, referencedVariants, ownership);
            }

            if (variants != null)
            {
                for (int i = 0; i < variants.Count; i++)
                {
                    VariantDefinitionSO variant = variants[i];
                    ValidateVariant(variant, localization, report);

                    if (variant != null && !referencedVariants.Contains(variant))
                        report.AddWarning("Variant is not referenced by any module definition.", variant);
                }
            }

            return report;
        }

        internal static ShowcaseLocalizationSnapshot CaptureLocalizationSnapshot()
        {
            StringTableCollection uiCollection = LocalizationEditorSettings.GetStringTableCollection(ShowcaseLocalization.UiTableName);
            StringTableCollection contentCollection = LocalizationEditorSettings.GetStringTableCollection(ShowcaseLocalization.ContentTableName);

            return new ShowcaseLocalizationSnapshot(
                FindTableByCode(uiCollection, "en"),
                FindTableByCode(uiCollection, "ru"),
                FindTableByCode(contentCollection, "en"),
                FindTableByCode(contentCollection, "ru"),
                uiCollection != null,
                contentCollection != null);
        }

        private static void ValidateModule(
            ModuleDefinitionSO module,
            ShowcaseLocalizationSnapshot localization,
            ShowcaseValidationReport report,
            HashSet<VariantDefinitionSO> referencedVariants,
            Dictionary<VariantDefinitionSO, ModuleDefinitionSO> ownership)
        {
            if (module == null)
            {
                report.AddError("Encountered a null module definition reference.", null);
                return;
            }

            bool hasLocalizationKey = ValidateLocalizationKey(module, module.LocalizationKey, report);
            if (hasLocalizationKey)
            {
                ValidateLocalizedContent(report, module, "Module name", ShowcaseLocalization.BuildModuleNameKey(module), localization, true);
                ValidateLocalizedContent(report, module, "Module thesis", ShowcaseLocalization.BuildModuleThesisKey(module), localization, true);
                ValidateLocalizedContent(report, module, "Module description", ShowcaseLocalization.BuildModuleDescriptionKey(module), localization, true);
                ValidateLocalizedContent(report, module, "Module problem statement", ShowcaseLocalization.BuildModuleProblemKey(module), localization, true);
                ValidateLocalizedContent(report, module, "Module active item label", ShowcaseLocalization.BuildModuleActiveItemLabelKey(module), localization, false);
                ValidateLocalizedContent(report, module, "Module WebGL preset note", ShowcaseLocalization.BuildModuleWebGlPresetKey(module), localization, false);
            }

            VariantDefinitionSO[] variants = module.Variants;
            if (variants == null || variants.Length == 0)
            {
                report.AddError("Module does not reference any variants.", module);
                return;
            }

            HashSet<VariantDefinitionSO> moduleVariants = new();
            for (int i = 0; i < variants.Length; i++)
            {
                VariantDefinitionSO variant = variants[i];
                if (variant == null)
                {
                    report.AddError($"Module contains a null variant reference at index {i}.", module);
                    continue;
                }

                if (!moduleVariants.Add(variant))
                    report.AddWarning($"Module references variant '{variant.name}' more than once.", module);

                referencedVariants.Add(variant);

                if (ownership.TryGetValue(variant, out ModuleDefinitionSO owner) && owner != module)
                {
                    report.AddWarning(
                        $"Variant '{variant.name}' is shared by multiple modules ('{owner.name}' and '{module.name}').",
                        variant);
                }
                else if (!ownership.ContainsKey(variant))
                {
                    ownership.Add(variant, module);
                }
            }
        }

        private static void ValidateVariant(
            VariantDefinitionSO variant,
            ShowcaseLocalizationSnapshot localization,
            ShowcaseValidationReport report)
        {
            if (variant == null)
            {
                report.AddError("Encountered a null variant definition reference.", null);
                return;
            }

            bool hasLocalizationKey = ValidateLocalizationKey(variant, variant.LocalizationKey, report);
            if (hasLocalizationKey)
            {
                ValidateLocalizedContent(report, variant, "Variant name", ShowcaseLocalization.BuildVariantNameKey(variant), localization, true);
                ValidateLocalizedContent(report, variant, "Variant architecture description", ShowcaseLocalization.BuildVariantArchitectureKey(variant), localization, true);
                ValidateLocalizedContent(report, variant, "Variant compare summary", ShowcaseLocalization.BuildVariantCompareKey(variant), localization, true);
                ValidateLocalizedContent(report, variant, "Variant data flow", ShowcaseLocalization.BuildVariantDataFlowKey(variant), localization, true);
                ValidateLocalizedContent(report, variant, "Variant runtime lifecycle", ShowcaseLocalization.BuildVariantRuntimeLifecycleKey(variant), localization, true);
                ValidateLocalizedContent(report, variant, "Variant why this approach", ShowcaseLocalization.BuildVariantWhyThisApproachKey(variant), localization, true);
                ValidateLocalizedContent(report, variant, "Variant takeaway", ShowcaseLocalization.BuildVariantTakeawayKey(variant), localization, false);
                ValidateLocalizedContent(report, variant, "Variant trade-offs", ShowcaseLocalization.BuildVariantTradeOffsKey(variant), localization, true);
                ValidateLocalizedContent(report, variant, "Variant pros", ShowcaseLocalization.BuildVariantProsKey(variant), localization, true);
                ValidateLocalizedContent(report, variant, "Variant cons", ShowcaseLocalization.BuildVariantConsKey(variant), localization, true);
            }

            ValidateStressPresets(variant, report);
            ValidatePrefabContracts(variant, report);
        }

        private static void ValidateRecruiterDemoScenarios(
            IReadOnlyList<RecruiterDemoScenarioSO> scenarios,
            IReadOnlyList<ModuleDefinitionSO> modules,
            ShowcaseLocalizationSnapshot localization,
            ShowcaseValidationReport report)
        {
            if (scenarios == null || scenarios.Count == 0)
            {
                report.AddWarning("No recruiter demo scenario assets were found.", null);
                return;
            }

            for (int i = 0; i < scenarios.Count; i++)
                ValidateRecruiterDemoScenario(scenarios[i], modules, localization, report);
        }

        private static void ValidateRecruiterDemoScenario(
            RecruiterDemoScenarioSO scenario,
            IReadOnlyList<ModuleDefinitionSO> modules,
            ShowcaseLocalizationSnapshot localization,
            ShowcaseValidationReport report)
        {
            if (scenario == null)
            {
                report.AddError("Encountered a null recruiter demo scenario asset.", null);
                return;
            }

            RecruiterDemoScenarioSO.StepDefinition[] steps = scenario.Steps;
            if (steps == null || steps.Length == 0)
            {
                report.AddError("Recruiter demo scenario does not define any steps.", scenario);
                return;
            }

            HashSet<VariantDefinitionSO> selectionCueVariants = new();
            RecruiterDemoScenarioSO.SelectionCueDefinition[] selectionCues = scenario.SelectionCues;
            for (int i = 0; i < selectionCues.Length; i++)
            {
                VariantDefinitionSO variant = selectionCues[i].Variant;
                if (variant == null)
                {
                    report.AddError($"Recruiter demo selection cue {i} is missing a variant reference.", scenario);
                    continue;
                }

                if (!selectionCueVariants.Add(variant))
                    report.AddError($"Recruiter demo selection cue {i} duplicates variant '{variant.name}'.", scenario);
            }

            for (int i = 0; i < steps.Length; i++)
                ValidateRecruiterDemoStep(scenario, steps[i], i, modules, localization, report);
        }

        private static void ValidateRecruiterDemoStep(
            RecruiterDemoScenarioSO scenario,
            RecruiterDemoScenarioSO.StepDefinition step,
            int index,
            IReadOnlyList<ModuleDefinitionSO> modules,
            ShowcaseLocalizationSnapshot localization,
            ShowcaseValidationReport report)
        {
            if (step.Module == null)
            {
                report.AddError($"Recruiter demo step {index} is missing a module reference.", scenario);
                return;
            }

            if (step.Variant == null)
            {
                report.AddError($"Recruiter demo step {index} is missing a variant reference.", scenario);
                return;
            }

            if (!ContainsReference(modules, step.Module))
            {
                report.AddError(
                    $"Recruiter demo step {index} references module '{step.Module.name}', which is not part of the showcase module list.",
                    scenario);
            }

            if (!ContainsReference(step.Module.Variants, step.Variant))
            {
                report.AddError(
                    $"Recruiter demo step {index} references variant '{step.Variant.name}', which is not part of module '{step.Module.name}'.",
                    scenario);
            }

            if (!step.HasNoteLocalizationKey)
            {
                report.AddError($"Recruiter demo step {index} requires an explicit note localization key.", scenario);
            }
            else
            {
                ValidateLocalizedContent(
                    report,
                    scenario,
                    $"Recruiter demo step {index} note",
                    step.NoteLocalizationKey,
                    localization,
                    true);
            }

            if (step.StressMode == RecruiterDemoStressMode.ExplicitValue && step.ExplicitStressLevel <= 0)
            {
                report.AddError($"Recruiter demo step {index} requires a positive explicit stress level.", scenario);
            }
        }

        private static bool ValidateLocalizationKey(
            UnityEngine.Object target,
            string localizationKey,
            ShowcaseValidationReport report)
        {
            if (!string.IsNullOrWhiteSpace(localizationKey))
                return true;

            report.AddError($"{target.GetType().Name} requires an explicit localizationKey.", target);
            return false;
        }

        private static void ValidateUiLocalization(ShowcaseLocalizationSnapshot localization, ShowcaseValidationReport report)
        {
            if (!localization.HasUiCollection)
            {
                report.AddError($"Missing localization collection '{ShowcaseLocalization.UiTableName}'.", null);
                return;
            }

            if (!localization.HasEnglishUiTable)
                report.AddError($"Missing English table for '{ShowcaseLocalization.UiTableName}'.", null);

            if (!localization.HasRussianUiTable)
                report.AddError($"Missing Russian table for '{ShowcaseLocalization.UiTableName}'.", null);

            if (!localization.HasEnglishUiTable || !localization.HasRussianUiTable)
                return;

            for (int i = 0; i < RequiredUiKeys.Length; i++)
            {
                string key = RequiredUiKeys[i];
                if (!HasLocalizedValue(localization.EnglishUiTable, key))
                    report.AddError($"UI key '{key}' is missing an English table entry.", localization.EnglishUiTable);

                if (!HasLocalizedValue(localization.RussianUiTable, key))
                    report.AddError($"UI key '{key}' is missing a Russian table entry.", localization.RussianUiTable);
            }
        }

        private static void ValidateContentLocalizationAvailability(ShowcaseLocalizationSnapshot localization, ShowcaseValidationReport report)
        {
            if (!localization.HasContentCollection)
            {
                report.AddError($"Missing localization collection '{ShowcaseLocalization.ContentTableName}'.", null);
                return;
            }

            if (!localization.HasEnglishContentTable)
                report.AddError($"Missing English table for '{ShowcaseLocalization.ContentTableName}'.", null);

            if (!localization.HasRussianContentTable)
                report.AddError($"Missing Russian table for '{ShowcaseLocalization.ContentTableName}'.", null);
        }

        private static void ValidateLocalizedContent(
            ShowcaseValidationReport report,
            UnityEngine.Object context,
            string fieldLabel,
            string key,
            ShowcaseLocalizationSnapshot localization,
            bool required)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                report.AddError($"{fieldLabel}: localization key is empty.", context);
                return;
            }

            bool hasEnglishTableValue = HasLocalizedValue(localization.EnglishContentTable, key);
            bool hasRussianTableValue = HasLocalizedValue(localization.RussianContentTable, key);

            ValidateLocaleTableOnly(report, context, fieldLabel, "English", key, hasEnglishTableValue, localization.HasEnglishContentTable, required);
            ValidateLocaleTableOnly(report, context, fieldLabel, "Russian", key, hasRussianTableValue, localization.HasRussianContentTable, required);
        }

        private static void ValidateLocaleTableOnly(
            ShowcaseValidationReport report,
            UnityEngine.Object context,
            string fieldLabel,
            string localeName,
            string key,
            bool hasTableValue,
            bool tableExists,
            bool required)
        {
            if (!tableExists)
            {
                if (required)
                    report.AddError($"{fieldLabel}: missing {localeName} string table for '{ShowcaseLocalization.ContentTableName}'.", context);

                return;
            }

            if (!hasTableValue)
            {
                string message = $"{fieldLabel} is missing {localeName} ShowcaseContent entry for key '{key}'.";
                if (required)
                    report.AddError(message, context);
                else
                    report.AddWarning(message, context);
            }
        }

        private static void ValidateStressPresets(VariantDefinitionSO variant, ShowcaseValidationReport report)
        {
            int[] presets = variant.GetStressPresets();
            if (presets == null || presets.Length == 0)
            {
                report.AddError("Variant does not define any stress presets.", variant);
                return;
            }

            int previous = int.MinValue;
            HashSet<int> uniquePresets = new();
            for (int i = 0; i < presets.Length; i++)
            {
                int value = presets[i];
                if (value <= 0)
                    report.AddError($"Stress preset at index {i} must be greater than zero, but was {value}.", variant);

                if (!uniquePresets.Add(value))
                    report.AddWarning($"Stress preset value {value} is duplicated.", variant);

                if (value < previous)
                    report.AddWarning("Stress presets are not ordered from low to high.", variant);

                previous = value;
            }
        }

        private static void ValidatePrefabContracts(VariantDefinitionSO variant, ShowcaseValidationReport report)
        {
            GameObject prefab = variant.Prefab;
            if (prefab == null)
            {
                report.AddError("Variant does not reference a prefab.", variant);
                return;
            }

            if (prefab.GetComponent<IModule>() == null)
                report.AddError("Variant prefab root must implement IModule.", prefab);

            if (prefab.GetComponent<IShowcaseStressTarget>() == null)
                report.AddError("Variant prefab root must implement IShowcaseStressTarget.", prefab);

            if (prefab.GetComponent<IShowcaseMetricsSource>() == null)
                report.AddWarning("Variant prefab root does not implement IShowcaseMetricsSource.", prefab);

            ValidateRequiredVisualPrefabs(prefab, report);
            ValidateRequiredAnimationProfiles(prefab, report);
        }

        private static void ValidateRequiredVisualPrefabs(GameObject prefab, ShowcaseValidationReport report)
        {
            MonoBehaviour[] behaviours = prefab.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                SerializedObject serializedObject = new(behaviour);
                SerializedProperty visualPrefabProperty = serializedObject.FindProperty("visualPrefab");
                if (visualPrefabProperty == null)
                    continue;

                if (visualPrefabProperty.propertyType != SerializedPropertyType.ObjectReference)
                {
                    report.AddError($"Component '{behaviour.GetType().Name}' has a non-object 'visualPrefab' field.", prefab);
                    continue;
                }

                if (visualPrefabProperty.objectReferenceValue == null)
                    report.AddError($"Component '{behaviour.GetType().Name}' requires an assigned visualPrefab.", prefab);
            }
        }

        private static void ValidateRequiredAnimationProfiles(GameObject prefab, ShowcaseValidationReport report)
        {
            MonoBehaviour[] behaviours = prefab.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                SerializedObject serializedObject = new(behaviour);
                SerializedProperty animationProfileProperty = serializedObject.FindProperty("animationProfile");
                if (animationProfileProperty == null)
                    continue;

                if (animationProfileProperty.propertyType != SerializedPropertyType.ObjectReference)
                {
                    report.AddError($"Component '{behaviour.GetType().Name}' has a non-object 'animationProfile' field.", prefab);
                    continue;
                }

                if (animationProfileProperty.objectReferenceValue == null)
                {
                    report.AddError($"Component '{behaviour.GetType().Name}' requires an assigned animationProfile.", prefab);
                    continue;
                }

                if (animationProfileProperty.objectReferenceValue is not HumanoidAnimationProfileSO profile)
                    continue;

                if (!profile.HasActorPrefab)
                {
                    report.AddError($"Animation profile '{profile.name}' requires an assigned actorPrefab.", profile);
                    continue;
                }

                if (!profile.HasAvatar)
                    report.AddError($"Animation profile '{profile.name}' requires an assigned avatar.", profile);

                if (!profile.HasAnimatorController)
                    report.AddError($"Animation profile '{profile.name}' requires an assigned animatorController.", profile);

                if (!profile.ActorPrefabHasAnimator)
                    report.AddError(
                        $"Animation profile '{profile.name}' actor prefab '{profile.ActorPrefab.name}' requires an Animator component in children.",
                        profile.ActorPrefab);

                if (!profile.ActorPrefabHasCrowdActor)
                    report.AddWarning(
                        $"Animation profile '{profile.name}' actor prefab '{profile.ActorPrefab.name}' should preconfigure HumanoidCrowdActor for a self-describing showcase contract.",
                        profile.ActorPrefab);
            }
        }

        internal static void ValidateHubPrefabLayout(GameObject hubPrefab, ShowcaseValidationReport report)
        {
            if (hubPrefab == null)
            {
                report.AddError($"Could not load showcase hub prefab at '{HubPrefabPath}'.", null);
                return;
            }

            DescriptionPanel descriptionPanel = hubPrefab.GetComponent<DescriptionPanel>();
            if (descriptionPanel == null)
            {
                report.AddError("Showcase hub prefab is missing DescriptionPanel on the root object.", hubPrefab);
                return;
            }

            ScrollRect scrollRect = descriptionPanel.ScrollRect != null
                ? descriptionPanel.ScrollRect
                : descriptionPanel.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                report.AddError("DescriptionPanel requires a ScrollRect component.", descriptionPanel);
                return;
            }

            RectTransform viewport = scrollRect.viewport;
            if (viewport == null)
            {
                report.AddError("DescriptionPanel ScrollRect is missing its viewport reference.", scrollRect);
                return;
            }

            ValidateRequiredReference(descriptionPanel, "tabsBar", "DescriptionPanel requires tabsBar.", report);
            ValidateRequiredReference(descriptionPanel, "activeTabUnderline", "DescriptionPanel requires activeTabUnderline.", report);
            ValidateRequiredReference(descriptionPanel, "legacyDescriptionText", "DescriptionPanel requires legacyDescriptionText.", report);
            ValidateRequiredReference(descriptionPanel, "overviewTabLabel", "DescriptionPanel requires overviewTabLabel.", report);
            ValidateRequiredReference(descriptionPanel, "architectureTabLabel", "DescriptionPanel requires architectureTabLabel.", report);
            ValidateRequiredReference(descriptionPanel, "tradeOffsTabLabel", "DescriptionPanel requires tradeOffsTabLabel.", report);

            HubUI hubUi = hubPrefab.GetComponent<HubUI>();
            if (hubUi == null)
            {
                report.AddError("Showcase hub prefab is missing HubUI on the root object.", hubPrefab);
            }
            else
            {
                ValidateRequiredReference(hubUi, "moduleName", "HubUI requires moduleName.", report);
                ValidateRequiredReference(hubUi, "variantName", "HubUI requires variantName.", report);
                ValidateRequiredReference(hubUi, "moduleSelectorName", "HubUI requires moduleSelectorName.", report);
                ValidateRequiredReference(hubUi, "variantSelectorName", "HubUI requires variantSelectorName.", report);
                ValidateArrayPropertyHasElements(hubUi, "moduleStats", "HubUI requires moduleStats bindings.", report);
                ValidateStructArrayObjectReference(hubUi, "moduleStats", "root", report);
                ValidateStructArrayObjectReference(hubUi, "moduleStats", "label", report);
                ValidateStructArrayObjectReference(hubUi, "moduleStats", "value", report);
            }

            ModuleNavigationControls navigationControls = hubPrefab.GetComponent<ModuleNavigationControls>();
            if (navigationControls != null)
            {
                ValidateRequiredReference(navigationControls, "_previousModuleButton", "ModuleNavigationControls requires _previousModuleButton.", report);
                ValidateRequiredReference(navigationControls, "_nextModuleButton", "ModuleNavigationControls requires _nextModuleButton.", report);
                ValidateRequiredReference(navigationControls, "_previousVariantButton", "ModuleNavigationControls requires _previousVariantButton.", report);
                ValidateRequiredReference(navigationControls, "_previousVariantLabel", "ModuleNavigationControls requires _previousVariantLabel.", report);
                ValidateRequiredReference(navigationControls, "_nextVariantButton", "ModuleNavigationControls requires _nextVariantButton.", report);
                ValidateRequiredReference(navigationControls, "_nextVariantLabel", "ModuleNavigationControls requires _nextVariantLabel.", report);
                ValidateRequiredReference(navigationControls, "_moduleSelectorButton", "ModuleNavigationControls requires _moduleSelectorButton.", report);
                ValidateRequiredReference(navigationControls, "_variantSelectorButton", "ModuleNavigationControls requires _variantSelectorButton.", report);
            }

            StressTestControls stressControls = hubPrefab.GetComponent<StressTestControls>();
            if (stressControls != null)
                ValidateRequiredReference(stressControls, "_view", "StressTestControls requires _view.", report);

            StressTestControlsView stressView = hubPrefab.GetComponent<StressTestControlsView>();
            if (stressView != null)
            {
                ValidateArrayPropertyHasElements(stressView, "_presetButtons", "StressTestControlsView requires preset button bindings.", report);
                ValidateStructArrayObjectReference(stressView, "_presetButtons", "_button", report);
                ValidateStructArrayObjectReference(stressView, "_presetButtons", "_label", report);
            }

            ShowcaseLocalization localization = hubPrefab.GetComponent<ShowcaseLocalization>();
            if (localization != null)
            {
                ValidateRequiredReference(localization, "languageToggleButton", "ShowcaseLocalization requires languageToggleButton.", report);
                ValidateRequiredReference(localization, "languageToggleLabel", "ShowcaseLocalization requires languageToggleLabel.", report);
                ValidateRequiredReference(localization, "breadcrumbText", "ShowcaseLocalization requires breadcrumbText.", report);
                ValidateRequiredReference(localization, "moduleHeaderText", "ShowcaseLocalization requires moduleHeaderText.", report);
                ValidateRequiredReference(localization, "variantHeaderText", "ShowcaseLocalization requires variantHeaderText.", report);
                ValidateRequiredReference(localization, "stressHeaderText", "ShowcaseLocalization requires stressHeaderText.", report);
                ValidateRequiredReference(localization, "statusTitleText", "ShowcaseLocalization requires statusTitleText.", report);
                ValidateRequiredReference(localization, "overviewTabText", "ShowcaseLocalization requires overviewTabText.", report);
                ValidateRequiredReference(localization, "architectureTabText", "ShowcaseLocalization requires architectureTabText.", report);
                ValidateRequiredReference(localization, "tradeOffsTabText", "ShowcaseLocalization requires tradeOffsTabText.", report);
            }
        }

        internal static void ValidateHubSceneLayout(ShowcaseValidationReport report)
        {
            Scene existingScene = SceneManager.GetSceneByPath(ShowcaseScenePath);
            bool wasLoaded = existingScene.IsValid() && existingScene.isLoaded;
            Scene scene = wasLoaded
                ? existingScene
                : EditorSceneManager.OpenScene(ShowcaseScenePath, OpenSceneMode.Additive);

            try
            {
                GameObject hubInstance = FindSceneHubRoot(scene);
                ValidateHubSceneInstanceLayout(hubInstance, report);
            }
            finally
            {
                if (!wasLoaded && scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        internal static void ValidateHubSceneInstanceLayout(GameObject hubInstance, ShowcaseValidationReport report)
        {
            if (hubInstance == null)
            {
                report.AddError($"Could not find scene instance '{HubRootName}' in '{ShowcaseScenePath}'.", null);
                return;
            }

            ValidateTransformIsIdentity(
                hubInstance.transform,
                $"Scene hub '{HubRootName}' must stay at the world origin",
                report);

            Transform moduleRoot = hubInstance.transform.Find("ModuleRoot");
            if (moduleRoot == null)
            {
                report.AddError("Scene hub instance is missing ModuleRoot.", hubInstance);
                return;
            }

            ValidateTransformIsIdentity(
                moduleRoot,
                "Scene ModuleRoot must stay aligned with the hub origin",
                report);
        }

        private static bool HasLocalizedValue(StringTable table, string key)
        {
            if (table == null || string.IsNullOrWhiteSpace(key))
                return false;

            StringTableEntry entry = table.GetEntry(key);
            return entry != null && !string.IsNullOrWhiteSpace(entry.Value);
        }

        private static StringTable FindTableByCode(StringTableCollection collection, string localeCode)
        {
            if (collection == null)
                return null;

            for (int i = 0; i < collection.StringTables.Count; i++)
            {
                StringTable table = collection.StringTables[i];
                if (table == null || table.LocaleIdentifier.Code == null)
                    continue;

                if (table.LocaleIdentifier.Code.StartsWith(localeCode, StringComparison.OrdinalIgnoreCase))
                    return table;
            }

            return null;
        }

        private static GameObject FindSceneHubRoot(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root != null && root.name == HubRootName)
                    return root;
            }

            return null;
        }

        private static void ValidateTransformIsIdentity(Transform target, string label, ShowcaseValidationReport report)
        {
            if (target == null)
                return;

            if (!Approximately(target.localPosition, Vector3.zero))
            {
                report.AddError(
                    $"{label}: localPosition must be (0, 0, 0), but was {target.localPosition}.",
                    target);
            }

            if (!Approximately(target.localEulerAngles, Vector3.zero))
            {
                report.AddError(
                    $"{label}: localRotation must be (0, 0, 0), but was {target.localEulerAngles}.",
                    target);
            }

            if (!Approximately(target.localScale, Vector3.one))
            {
                report.AddError(
                    $"{label}: localScale must be (1, 1, 1), but was {target.localScale}.",
                    target);
            }
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return Mathf.Abs(left.x - right.x) <= TransformTolerance &&
                   Mathf.Abs(left.y - right.y) <= TransformTolerance &&
                   Mathf.Abs(left.z - right.z) <= TransformTolerance;
        }

        private static bool ContainsReference<T>(IReadOnlyList<T> values, T candidate)
            where T : UnityEngine.Object
        {
            if (values == null || candidate == null)
                return false;

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] == candidate)
                    return true;
            }

            return false;
        }

        private static bool ContainsReference<T>(T[] values, T candidate)
            where T : UnityEngine.Object
        {
            if (values == null || candidate == null)
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == candidate)
                    return true;
            }

            return false;
        }

        private static void ValidateRequiredReference(
            UnityEngine.Object target,
            string propertyName,
            string message,
            ShowcaseValidationReport report)
        {
            SerializedProperty property = new SerializedObject(target).FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue == null)
                report.AddError(message, target);
        }

        private static void ValidateArrayPropertyHasElements(
            UnityEngine.Object target,
            string propertyName,
            string message,
            ShowcaseValidationReport report)
        {
            SerializedProperty property = new SerializedObject(target).FindProperty(propertyName);
            if (property == null || !property.isArray || property.arraySize == 0)
                report.AddError(message, target);
        }

        private static void ValidateStructArrayObjectReference(
            UnityEngine.Object target,
            string arrayPropertyName,
            string childPropertyName,
            ShowcaseValidationReport report)
        {
            SerializedProperty property = new SerializedObject(target).FindProperty(arrayPropertyName);
            if (property == null || !property.isArray)
                return;

            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                SerializedProperty child = element.FindPropertyRelative(childPropertyName);
                if (child == null || child.propertyType != SerializedPropertyType.ObjectReference || child.objectReferenceValue == null)
                {
                    report.AddError(
                        $"{target.GetType().Name} requires '{arrayPropertyName}[{i}].{childPropertyName}'.",
                        target);
                }
            }
        }

        private static T[] LoadAssets<T>()
            where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, DefinitionSearchFolders);
            List<T> assets = new(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            return assets.ToArray();
        }

        internal static void LogReport(ShowcaseValidationReport report, int moduleCount, int variantCount)
        {
            foreach (ShowcaseValidationIssue issue in report.Issues)
            {
                string message = "[ShowcaseValidator] " + issue.Message;
                if (issue.Severity == ShowcaseValidationSeverity.Error)
                    Debug.LogError(message, issue.Context);
                else
                    Debug.LogWarning(message, issue.Context);
            }

            string summary = $"[ShowcaseValidator] Checked {moduleCount} modules and {variantCount} variants. {report.ErrorCount} errors, {report.WarningCount} warnings.";
            if (report.IsValid)
                Debug.Log(summary);
            else
                Debug.LogError(summary);
        }
    }
}
