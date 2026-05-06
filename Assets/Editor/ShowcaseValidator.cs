using System;
using System.Collections.Generic;
using LearningArchitect.Core;
using LearningArchitect.Modules.Animation3D;
using LearningArchitect.UI;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
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
            "demo_complete",
            "fps",
            "frame_time",
            "guided_demo",
            "key_features",
            "load_selected",
            "loaded",
            "module",
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
            "realtime_preview",
            "runtime_lifecycle",
            "select_module",
            "select_variant",
            "start_demo",
            "step",
            "stop_demo",
            "strengths",
            "stress_load",
            "stress_test",
            "switching_module_next",
            "switching_module_prev",
            "switching_variant_next",
            "switching_variant_prev",
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
            ShowcaseLocalizationSnapshot localization = CaptureLocalizationSnapshot();
            ShowcaseValidationReport report = ValidateDefinitions(modules, variants, localization);
            ValidateHubPrefabLayout(AssetDatabase.LoadAssetAtPath<GameObject>(HubPrefabPath), report);

            LogReport(report, modules.Length, variants.Length);
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

            ValidateLocalizedContent(report, module, "Module name", ShowcaseLocalization.BuildModuleNameKey(module), module.ModuleName, module.ModuleNameRu, localization, true);
            ValidateLocalizedContent(report, module, "Module thesis", ShowcaseLocalization.BuildModuleThesisKey(module), module.Thesis, module.ThesisRu, localization, true);
            ValidateLocalizedContent(report, module, "Module description", ShowcaseLocalization.BuildModuleDescriptionKey(module), module.Description, module.DescriptionRu, localization, true);
            ValidateLocalizedContent(report, module, "Module problem statement", ShowcaseLocalization.BuildModuleProblemKey(module), module.ProblemStatement, module.ProblemStatementRu, localization, true);
            ValidateLocalizedContent(report, module, "Module active item label", ShowcaseLocalization.BuildModuleActiveItemLabelKey(module), module.ActiveItemLabel, module.ActiveItemLabelRu, localization, false);
            ValidateLocalizedContent(report, module, "Module WebGL preset note", ShowcaseLocalization.BuildModuleWebGlPresetKey(module), module.WebGlPresetNote, module.WebGlPresetNoteRu, localization, false);

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

            ValidateLocalizedContent(report, variant, "Variant name", ShowcaseLocalization.BuildVariantNameKey(variant), variant.VariantName, variant.VariantNameRu, localization, true);
            ValidateLocalizedContent(report, variant, "Variant architecture description", ShowcaseLocalization.BuildVariantArchitectureKey(variant), variant.ArchitectureDescription, variant.ArchitectureDescriptionRu, localization, true);
            ValidateLocalizedContent(report, variant, "Variant compare summary", ShowcaseLocalization.BuildVariantCompareKey(variant), variant.CompareSummary, variant.CompareSummaryRu, localization, true);
            ValidateLocalizedContent(report, variant, "Variant data flow", ShowcaseLocalization.BuildVariantDataFlowKey(variant), variant.DataFlow, variant.DataFlowRu, localization, true);
            ValidateLocalizedContent(report, variant, "Variant runtime lifecycle", ShowcaseLocalization.BuildVariantRuntimeLifecycleKey(variant), variant.RuntimeLifecycle, variant.RuntimeLifecycleRu, localization, true);
            ValidateLocalizedContent(report, variant, "Variant why this approach", ShowcaseLocalization.BuildVariantWhyThisApproachKey(variant), variant.WhyThisApproach, variant.WhyThisApproachRu, localization, true);
            ValidateLocalizedContent(report, variant, "Variant takeaway", ShowcaseLocalization.BuildVariantTakeawayKey(variant), variant.Takeaway, variant.TakeawayRu, localization, false);
            ValidateLocalizedContent(report, variant, "Variant trade-offs", ShowcaseLocalization.BuildVariantTradeOffsKey(variant), variant.TradeOffs, variant.TradeOffsRu, localization, true);
            ValidateLocalizedContent(report, variant, "Variant pros", ShowcaseLocalization.BuildVariantProsKey(variant), variant.Pros, variant.ProsRu, localization, true);
            ValidateLocalizedContent(report, variant, "Variant cons", ShowcaseLocalization.BuildVariantConsKey(variant), variant.Cons, variant.ConsRu, localization, true);

            ValidateStressPresets(variant, report);
            ValidateStressPresetLabels(variant, report, ShowcaseLanguage.English);
            ValidateStressPresetLabels(variant, report, ShowcaseLanguage.Russian);
            ValidatePrefabContracts(variant, report);
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
                    report.AddWarning($"UI key '{key}' is missing an English table entry.", localization.EnglishUiTable);

                if (!HasLocalizedValue(localization.RussianUiTable, key))
                    report.AddWarning($"UI key '{key}' is missing a Russian table entry.", localization.RussianUiTable);
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
            string englishFallback,
            string russianFallback,
            ShowcaseLocalizationSnapshot localization,
            bool required)
        {
            bool hasEnglishFallback = !string.IsNullOrWhiteSpace(englishFallback);
            bool hasRussianFallback = !string.IsNullOrWhiteSpace(russianFallback);
            bool hasEnglishTableValue = HasLocalizedValue(localization.EnglishContentTable, key);
            bool hasRussianTableValue = HasLocalizedValue(localization.RussianContentTable, key);

            ValidateLocaleContent(report, context, fieldLabel, "English", key, hasEnglishFallback, hasEnglishTableValue, localization.HasEnglishContentTable, required);
            ValidateLocaleContent(report, context, fieldLabel, "Russian", key, hasRussianFallback, hasRussianTableValue, localization.HasRussianContentTable, required);
        }

        private static void ValidateLocaleContent(
            ShowcaseValidationReport report,
            UnityEngine.Object context,
            string fieldLabel,
            string localeName,
            string key,
            bool hasFallback,
            bool hasTableValue,
            bool tableExists,
            bool required)
        {
            if (!hasFallback && !hasTableValue)
            {
                string message = $"{fieldLabel} is missing {localeName} content.";
                if (required)
                    report.AddError(message, context);
                else
                    report.AddWarning(message, context);

                return;
            }

            if (tableExists && !hasTableValue && hasFallback)
                report.AddWarning($"{fieldLabel} is missing a {localeName} localization table entry for key '{key}'.", context);
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

        private static void ValidateStressPresetLabels(VariantDefinitionSO variant, ShowcaseValidationReport report, ShowcaseLanguage language)
        {
            string localeName = language == ShowcaseLanguage.Russian ? "Russian" : "English";
            string[] labels = variant.GetStressPresetLabels(language);

            if (labels == null)
            {
                if (HasSerializedPresetLabels(variant, language))
                    report.AddError($"{localeName} stress preset labels do not match the preset count.", variant);

                return;
            }

            for (int i = 0; i < labels.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(labels[i]))
                    report.AddWarning($"{localeName} stress preset label at index {i} is blank.", variant);
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

                if (animationProfileProperty.objectReferenceValue is HumanoidAnimationProfileSO profile && !profile.HasActorPrefab)
                    report.AddError($"Animation profile '{profile.name}' requires an assigned actorPrefab.", profile);
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

            if (FindDeep(descriptionPanel.transform, "TabsBar") == null &&
                FindDeep(descriptionPanel.transform, "Container - DescriptionCharacters") == null)
            {
                report.AddError("DescriptionPanel is missing its tabs root.", descriptionPanel);
            }

            if (FindDeep(descriptionPanel.transform, "ActiveTabUnderline") == null)
                report.AddError("DescriptionPanel is missing ActiveTabUnderline.", descriptionPanel);

            bool hasLegacyDescriptionText = FindDeep(viewport, "DescriptionText") != null;
            bool hasCompositeLayout = FindDeep(viewport, "Container - AboutInfo") != null ||
                                      FindDeep(viewport, "Container - ArchitectureInfo") != null ||
                                      FindDeep(viewport, "Container - Trade-OffsInfo") != null ||
                                      FindDeep(viewport, "Container - ProsCons") != null;

            if (hasCompositeLayout)
            {
                ValidateRequiredSection(viewport, "Container - AboutInfo", report);
                ValidateRequiredSection(viewport, "Container - ArchitectureInfo", report);
                ValidateRequiredSection(viewport, "Container - Trade-OffsInfo", report);

                Transform prosConsRoot = FindDeep(viewport, "Container - ProsCons");
                if (prosConsRoot == null)
                {
                    report.AddError("DescriptionPanel viewport is missing Container - ProsCons.", viewport);
                    return;
                }

                ValidateRequiredSection(prosConsRoot, "Container - Pros", report);
                ValidateRequiredSection(prosConsRoot, "Container - Cons", report);
                return;
            }

            if (!hasLegacyDescriptionText)
                report.AddError("DescriptionPanel viewport must provide either composite sections or legacy DescriptionText.", viewport);
        }

        private static void ValidateRequiredSection(Transform parent, string sectionName, ShowcaseValidationReport report)
        {
            Transform section = FindDeep(parent, sectionName);
            if (section == null)
            {
                report.AddError($"DescriptionPanel is missing required section '{sectionName}'.", parent as UnityEngine.Object);
                return;
            }

            if (FindDeep(section, "Text - Header") == null)
                report.AddError($"DescriptionPanel section '{sectionName}' is missing 'Text - Header'.", section as UnityEngine.Object);

            if (FindDeep(section, "Text - Description") == null)
                report.AddError($"DescriptionPanel section '{sectionName}' is missing 'Text - Description'.", section as UnityEngine.Object);
        }

        private static bool HasSerializedPresetLabels(VariantDefinitionSO variant, ShowcaseLanguage language)
        {
            SerializedObject serializedObject = new(variant);
            string propertyName = language == ShowcaseLanguage.Russian ? "stressPresetLabelsRu" : "stressPresetLabels";
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.arraySize > 0;
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

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;

            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeep(root.GetChild(i), name);
                if (match != null)
                    return match;
            }

            return null;
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

        private static void LogReport(ShowcaseValidationReport report, int moduleCount, int variantCount)
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
