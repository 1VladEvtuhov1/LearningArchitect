using System;
using LearningArchitect.Core;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LearningArchitect.UI
{
    /// <summary>
    /// Resolves showcase copy from Unity Localization <c>ShowcaseContent</c> and <c>ShowcaseUI</c> tables only.
    /// Missing entries surface explicit markers; module/variant ScriptableObjects carry ids and wiring, not copy.
    /// </summary>
    public static class ShowcaseLocalizationContent
    {
        public static string GetText(ShowcaseLanguage language, string key)
        {
            return GetLocalizedString(
                ShowcaseLocalization.UiTableName,
                key,
                GetUiFallbackText(language, key),
                language,
                useUiFallbackWhenMissing: true);
        }

        public static string GetModuleName(ModuleDefinitionSO module)
        {
            if (module == null)
                return GetText(ShowcaseLocalization.CurrentLanguage, "no_module");

            return GetRequiredContentText(BuildModuleNameKey(module), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetModuleDescription(ModuleDefinitionSO module)
        {
            if (module == null)
                return string.Empty;

            return GetRequiredContentText(BuildModuleDescriptionKey(module), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetModuleCategory(ModuleDefinitionSO module)
        {
            if (module == null)
                return string.Empty;

            string uiKey = module.Category == ShowcaseModuleCategory.ArchitecturePatternModule
                ? "module_category_architecture"
                : "module_category_simulation";

            return GetText(ShowcaseLocalization.CurrentLanguage, uiKey);
        }

        public static string GetModuleProblemStatement(ModuleDefinitionSO module)
        {
            if (module == null)
                return string.Empty;

            return GetRequiredContentText(BuildModuleProblemKey(module), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetModuleWebGlPresetNote(ModuleDefinitionSO module)
        {
            if (module == null)
                return string.Empty;

            return GetOptionalContentText(BuildModuleWebGlPresetKey(module), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetModuleActiveItemLabel(ModuleDefinitionSO module)
        {
            if (module == null)
                return GetText(ShowcaseLocalization.CurrentLanguage, "active_items");

            string localized = GetOptionalContentText(BuildModuleActiveItemLabelKey(module), ShowcaseLocalization.CurrentLanguage);
            return string.IsNullOrWhiteSpace(localized)
                ? GetText(ShowcaseLocalization.CurrentLanguage, "active_items")
                : localized;
        }

        public static string GetVariantName(VariantDefinitionSO variant)
        {
            if (variant == null)
                return GetText(ShowcaseLocalization.CurrentLanguage, "no_variant");

            return GetRequiredContentText(BuildVariantNameKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantArchitectureDescription(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetRequiredContentText(BuildVariantArchitectureKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantDataFlow(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetRequiredContentText(BuildVariantDataFlowKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantRuntimeLifecycle(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetRequiredContentText(BuildVariantRuntimeLifecycleKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantWhyThisApproach(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetRequiredContentText(BuildVariantWhyThisApproachKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantCompareSummary(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetRequiredContentText(BuildVariantCompareKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantTakeaway(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetOptionalContentText(BuildVariantTakeawayKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantTradeOffs(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetRequiredContentText(BuildVariantTradeOffsKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantPros(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetRequiredContentText(BuildVariantProsKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetVariantCons(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetRequiredContentText(BuildVariantConsKey(variant), ShowcaseLocalization.CurrentLanguage);
        }

        public static string GetRequiredContentText(string entryKey, ShowcaseLanguage language)
        {
            if (TryGetLocalizedString(ShowcaseLocalization.ContentTableName, entryKey, out string localized))
                return localized;

            return BuildMissingValue(language, entryKey, useUiFallbackWhenMissing: false);
        }

        public static string GetOptionalContentText(string entryKey, ShowcaseLanguage language)
        {
            if (TryGetLocalizedString(ShowcaseLocalization.ContentTableName, entryKey, out string localized))
                return localized;

            return string.Empty;
        }

        public static string BuildModuleNameKey(ModuleDefinitionSO module)
        {
            return BuildContentKey(module == null ? string.Empty : module.GetLocalizationKey(), "module_name");
        }

        public static string BuildModuleDescriptionKey(ModuleDefinitionSO module)
        {
            return BuildContentKey(module == null ? string.Empty : module.GetLocalizationKey(), "description");
        }

        public static string BuildModuleThesisKey(ModuleDefinitionSO module)
        {
            return BuildContentKey(module == null ? string.Empty : module.GetLocalizationKey(), "thesis");
        }

        public static string BuildModuleProblemKey(ModuleDefinitionSO module)
        {
            return BuildContentKey(module == null ? string.Empty : module.GetLocalizationKey(), "problem");
        }

        public static string BuildModuleWebGlPresetKey(ModuleDefinitionSO module)
        {
            return BuildContentKey(module == null ? string.Empty : module.GetLocalizationKey(), "webgl_preset");
        }

        public static string BuildModuleActiveItemLabelKey(ModuleDefinitionSO module)
        {
            return BuildContentKey(module == null ? string.Empty : module.GetLocalizationKey(), "active_item_label");
        }

        public static string BuildVariantNameKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "variant_name");
        }

        public static string BuildVariantArchitectureKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "architecture");
        }

        public static string BuildVariantCompareKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "compare");
        }

        public static string BuildVariantDataFlowKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "data_flow");
        }

        public static string BuildVariantRuntimeLifecycleKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "runtime_lifecycle");
        }

        public static string BuildVariantWhyThisApproachKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "why_this_approach");
        }

        public static string BuildVariantTakeawayKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "takeaway");
        }

        public static string BuildVariantTradeOffsKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "trade_offs");
        }

        public static string BuildVariantProsKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "pros");
        }

        public static string BuildVariantConsKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.GetLocalizationKey(), "cons");
        }

        public static string BuildContentKey(string localizationKey, string suffix)
        {
            if (string.IsNullOrWhiteSpace(localizationKey))
                return suffix;

            char[] buffer = localizationKey.ToLowerInvariant().ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {
                char current = buffer[i];
                if ((current >= 'a' && current <= 'z') ||
                    (current >= '0' && current <= '9') ||
                    current == '_' ||
                    current == '.')
                    continue;

                buffer[i] = '_';
            }

            string sanitized = new(buffer);
            return sanitized.Trim('_') + "." + suffix;
        }

        public static string BuildMissingMarker(string key)
        {
            return "[MISSING: " + key + "]";
        }

        private static bool TryGetLocalizedString(string tableName, string entryKey, out string localized)
        {
            localized = null;
            if (ShowcaseLocalization.IsLocalizationBackendAvailable &&
                !string.IsNullOrWhiteSpace(entryKey) &&
                LocalizationSettings.HasSettings)
            {
                try
                {
                    if (LocalizationSettings.SelectedLocale == null)
                        return false;

                    string candidate = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, entryKey);
                    if (IsUsableLocalizedValue(candidate, entryKey))
                    {
                        localized = candidate;
                        return true;
                    }
                }
                catch
                {
                    ShowcaseLocalization.DisableLocalizationBackend();
                }
            }

            return false;
        }

        private static string GetLocalizedString(
            string tableName,
            string entryKey,
            string fallback,
            ShowcaseLanguage language,
            bool useUiFallbackWhenMissing)
        {
            if (TryGetLocalizedString(tableName, entryKey, out string localized))
                return localized;

            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback;

            return BuildMissingValue(language, entryKey, useUiFallbackWhenMissing);
        }

        private static string BuildMissingValue(ShowcaseLanguage language, string entryKey, bool useUiFallbackWhenMissing)
        {
            if (useUiFallbackWhenMissing)
                return GetUiFallbackText(language, entryKey);

            return BuildMissingMarker(
                string.IsNullOrWhiteSpace(entryKey)
                    ? ShowcaseLocalization.ContentTableName
                    : ShowcaseLocalization.ContentTableName + "." + entryKey);
        }

        private static bool IsUsableLocalizedValue(string localized, string entryKey)
        {
            if (string.IsNullOrWhiteSpace(localized))
                return false;

            if (string.Equals(localized, entryKey, StringComparison.Ordinal))
                return false;

            if (localized.IndexOf("No translation found", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            if (localized.IndexOf("Missing translation", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            return true;
        }

        private static string GetUiFallbackText(ShowcaseLanguage language, string key)
        {
            if (TryGetInterviewArenaUiFallback(language, key, out string fallback))
                return fallback;

            string qualifiedKey = string.IsNullOrWhiteSpace(key)
                ? ShowcaseLocalization.UiTableName
                : ShowcaseLocalization.UiTableName + "." + key;

            return BuildMissingMarker(qualifiedKey);
        }

        private static bool TryGetInterviewArenaUiFallback(ShowcaseLanguage language, string key, out string text)
        {
            text = null;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            bool russian = language == ShowcaseLanguage.Russian;
            switch (key)
            {
                case "interview_arena_launch":
                    text = russian ? "Interview Arena" : "Interview Arena";
                    return true;
                case "interview_arena_title":
                    text = russian ? "Interview Arena" : "Interview Arena";
                    return true;
                case "interview_arena_subtitle":
                    text = russian
                        ? "Солдаты: melee, арбалет на дистанции, респавн. Слева — индикатор i-frames (голубая вспышка на герое)."
                        : "Soldiers: melee, ranged crossbow, respawn. Top-left i-frame bar; blue flash on the hero.";
                    return true;
                case "interview_arena_back":
                    text = russian ? "Назад к архитектурным демо" : "Back to architecture demos";
                    return true;
                default:
                    return false;
            }
        }
    }
}
