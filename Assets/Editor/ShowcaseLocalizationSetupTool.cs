using System;
using System.Collections.Generic;
using LearningArchitect.Core;
using LearningArchitect.UI;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace LearningArchitect.EditorTools
{
    public static class ShowcaseLocalizationSetupTool
    {
        private const string RootFolder = "Assets/Localization";
        private const string LocalesFolder = "Assets/Localization/Locales";
        private const string TablesFolder = "Assets/Localization/Tables";
        private const string SettingsPath = "Assets/Localization/ShowcaseLocalizationSettings.asset";

        [MenuItem("Learning Architect/Localization/Setup Showcase Localization")]
        public static void Setup()
        {
            EnsureFolder("Assets", "Localization");
            EnsureFolder(RootFolder, "Locales");
            EnsureFolder(RootFolder, "Tables");

            LocalizationSettings settings = EnsureSettings();
            Locale english = EnsureLocale("en", "English");
            Locale russian = EnsureLocale("ru", "Russian");

            LocalizationEditorSettings.AddLocale(english);
            LocalizationEditorSettings.AddLocale(russian);

            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            LocalizationSettings.InitializeSynchronously = true;

            List<Locale> locales = new List<Locale> { english, russian };
            StringTableCollection uiCollection = EnsureCollection(ShowcaseLocalization.UiTableName, locales);
            StringTableCollection contentCollection = EnsureCollection(ShowcaseLocalization.ContentTableName, locales);

            PopulateUiCollection(uiCollection, english, russian);
            PopulateContentCollection(contentCollection, english, russian);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Showcase localization tables are ready.");
        }

        private static LocalizationSettings EnsureSettings()
        {
            LocalizationSettings settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                settings.name = "Showcase Localization Settings";
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            return settings;
        }

        private static Locale EnsureLocale(string code, string fileName)
        {
            string assetPath = LocalesFolder + "/" + fileName + ".asset";
            Locale locale = AssetDatabase.LoadAssetAtPath<Locale>(assetPath);
            if (locale != null)
                return locale;

            locale = Locale.CreateLocale(code);
            locale.name = fileName;
            AssetDatabase.CreateAsset(locale, assetPath);
            return locale;
        }

        private static StringTableCollection EnsureCollection(string tableName, IList<Locale> locales)
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
                collection = LocalizationEditorSettings.CreateStringTableCollection(tableName, TablesFolder, locales);

            foreach (Locale locale in locales)
            {
                if (collection.GetTable(locale.Identifier) == null)
                    collection.AddNewTable(locale.Identifier);
            }

            foreach (StringTable table in collection.StringTables)
                LocalizationEditorSettings.SetPreloadTableFlag(table, true);

            return collection;
        }

        private static void PopulateUiCollection(StringTableCollection collection, Locale english, Locale russian)
        {
            StringTable englishTable = GetStringTableOrThrow(collection, english);
            StringTable russianTable = GetStringTableOrThrow(collection, russian);

            Dictionary<string, string> englishEntries = new Dictionary<string, string>
            {
                { "breadcrumb", "ENGINE SYSTEMS / <color=#" + ShowcasePalette.AccentHex + ">VISUALIZER</color>" },
                { "module", "MODULE" },
                { "variant", "VARIANT" },
                { "loaded", "LOADED" },
                { "orbit", "ORBIT" },
                { "zoom", "ZOOM" },
                { "performance", "PERFORMANCE" },
                { "system_status", "SYSTEM STATUS" },
                { "select_module", "SELECT MODULE" },
                { "select_variant", "SELECT VARIANT" },
                { "stress_test", "STRESS TEST" },
                { "overview", "Overview" },
                { "architecture", "Architecture" },
                { "trade_offs", "Trade-offs" },
                { "about", "ABOUT" },
                { "key_features", "KEY FEATURES" },
                { "watch_out", "WATCH OUT" },
                { "strengths", "STRENGTHS" },
                { "pros", "PROS" },
                { "cons", "CONS" },
                { "no_module", "No module selected." },
                { "no_variant", "No variant selected." },
                { "no_architecture_notes", "No architecture notes provided." },
                { "no_tradeoffs", "No trade-off notes provided." },
                { "no_key_features", "No key features listed." },
                { "no_strengths", "No strengths listed." },
                { "no_pros", "No pros listed." },
                { "no_cons", "No cons listed." },
                { "no_constraints", "No constraints listed." },
                { "realtime_preview", "Realtime architecture preview" },
                { "switching_module_prev", "Switching module  ‹" },
                { "switching_module_next", "Switching module  ›" },
                { "switching_variant_prev", "Switching variant  ‹" },
                { "switching_variant_next", "Switching variant  ›" },
                { "load_selected", "Load selected" },
                { "stress_load", "Stress load" },
                { "active", "Active" },
                { "fps", "FPS" },
                { "frame_time", "Frame Time" },
                { "particles", "Particles" },
                { "na", "N/A" },
                { "tooltip_prev_module", "Previous module" },
                { "tooltip_next_module", "Next module" },
                { "tooltip_prev_variant", "Previous variant" },
                { "tooltip_next_variant", "Next variant" }
            };

            Dictionary<string, string> russianEntries = new Dictionary<string, string>
            {
                { "breadcrumb", "СИСТЕМЫ ДВИЖКА / <color=#" + ShowcasePalette.AccentHex + ">ВИЗУАЛИЗАТОР</color>" },
                { "module", "МОДУЛЬ" },
                { "variant", "ВАРИАНТ" },
                { "loaded", "ЗАГРУЖЕНО" },
                { "orbit", "ОРБИТА" },
                { "zoom", "МАСШТАБ" },
                { "performance", "ПРОИЗВОДИТЕЛЬНОСТЬ" },
                { "system_status", "СОСТОЯНИЕ СИСТЕМЫ" },
                { "select_module", "ВЫБОР МОДУЛЯ" },
                { "select_variant", "ВЫБОР ВАРИАНТА" },
                { "stress_test", "СТРЕСС-ТЕСТ" },
                { "overview", "Обзор" },
                { "architecture", "Архитектура" },
                { "trade_offs", "Компромиссы" },
                { "about", "ОПИСАНИЕ" },
                { "key_features", "КЛЮЧЕВЫЕ ПЛЮСЫ" },
                { "watch_out", "ОГРАНИЧЕНИЯ" },
                { "strengths", "СИЛЬНЫЕ СТОРОНЫ" },
                { "pros", "ПЛЮСЫ" },
                { "cons", "МИНУСЫ" },
                { "no_module", "Модуль не выбран." },
                { "no_variant", "Вариант не выбран." },
                { "no_architecture_notes", "Описание архитектуры отсутствует." },
                { "no_tradeoffs", "Описание компромиссов отсутствует." },
                { "no_key_features", "Ключевые преимущества не указаны." },
                { "no_strengths", "Сильные стороны не указаны." },
                { "no_pros", "Плюсы не указаны." },
                { "no_cons", "Минусы не указаны." },
                { "no_constraints", "Ограничения не указаны." },
                { "realtime_preview", "Интерактивный просмотр архитектуры" },
                { "switching_module_prev", "Переключение модуля  ‹" },
                { "switching_module_next", "Переключение модуля  ›" },
                { "switching_variant_prev", "Переключение варианта  ‹" },
                { "switching_variant_next", "Переключение варианта  ›" },
                { "load_selected", "Нагрузка выбрана" },
                { "stress_load", "Нагрузка" },
                { "active", "Активно" },
                { "fps", "FPS" },
                { "frame_time", "Время кадра" },
                { "particles", "Частицы" },
                { "na", "Н/Д" },
                { "tooltip_prev_module", "Предыдущий модуль" },
                { "tooltip_next_module", "Следующий модуль" },
                { "tooltip_prev_variant", "Предыдущий вариант" },
                { "tooltip_next_variant", "Следующий вариант" }
            };

            WriteEntries(englishTable, englishEntries);
            WriteEntries(russianTable, russianEntries);
        }

        private static void PopulateContentCollection(StringTableCollection collection, Locale english, Locale russian)
        {
            StringTable englishTable = GetStringTableOrThrow(collection, english);
            StringTable russianTable = GetStringTableOrThrow(collection, russian);

            string[] moduleGuids = AssetDatabase.FindAssets("t:ModuleDefinitionSO", new[] { "Assets/Prefabs/Showcase" });
            for (int i = 0; i < moduleGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(moduleGuids[i]);
                ModuleDefinitionSO module = LoadAssetOrThrow<ModuleDefinitionSO>(path);

                SetEntry(englishTable, ShowcaseLocalization.BuildModuleNameKey(module), module.ModuleName);
                SetEntry(russianTable, ShowcaseLocalization.BuildModuleNameKey(module), module.ModuleNameRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildModuleDescriptionKey(module), module.Description);
                SetEntry(russianTable, ShowcaseLocalization.BuildModuleDescriptionKey(module), module.DescriptionRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildModuleThesisKey(module), module.Thesis);
                SetEntry(russianTable, ShowcaseLocalization.BuildModuleThesisKey(module), module.ThesisRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildModuleProblemKey(module), module.ProblemStatement);
                SetEntry(russianTable, ShowcaseLocalization.BuildModuleProblemKey(module), module.ProblemStatementRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildModuleWebGlPresetKey(module), module.WebGlPresetNote);
                SetEntry(russianTable, ShowcaseLocalization.BuildModuleWebGlPresetKey(module), module.WebGlPresetNoteRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildModuleActiveItemLabelKey(module), module.ActiveItemLabel);
                SetEntry(russianTable, ShowcaseLocalization.BuildModuleActiveItemLabelKey(module), module.ActiveItemLabelRu);
            }

            string[] variantGuids = AssetDatabase.FindAssets("t:VariantDefinitionSO", new[] { "Assets/Prefabs/Showcase" });
            for (int i = 0; i < variantGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(variantGuids[i]);
                VariantDefinitionSO variant = LoadAssetOrThrow<VariantDefinitionSO>(path);

                SetEntry(englishTable, ShowcaseLocalization.BuildVariantNameKey(variant), variant.VariantName);
                SetEntry(russianTable, ShowcaseLocalization.BuildVariantNameKey(variant), variant.VariantNameRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildVariantArchitectureKey(variant), variant.ArchitectureDescription);
                SetEntry(russianTable, ShowcaseLocalization.BuildVariantArchitectureKey(variant), variant.ArchitectureDescriptionRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildVariantCompareKey(variant), variant.CompareSummary);
                SetEntry(russianTable, ShowcaseLocalization.BuildVariantCompareKey(variant), variant.CompareSummaryRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildVariantTakeawayKey(variant), variant.Takeaway);
                SetEntry(russianTable, ShowcaseLocalization.BuildVariantTakeawayKey(variant), variant.TakeawayRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildVariantTradeOffsKey(variant), variant.TradeOffs);
                SetEntry(russianTable, ShowcaseLocalization.BuildVariantTradeOffsKey(variant), variant.TradeOffsRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildVariantProsKey(variant), variant.Pros);
                SetEntry(russianTable, ShowcaseLocalization.BuildVariantProsKey(variant), variant.ProsRu);
                SetEntry(englishTable, ShowcaseLocalization.BuildVariantConsKey(variant), variant.Cons);
                SetEntry(russianTable, ShowcaseLocalization.BuildVariantConsKey(variant), variant.ConsRu);
            }
        }

        private static void WriteEntries(StringTable table, Dictionary<string, string> entries)
        {
            foreach (KeyValuePair<string, string> pair in entries)
                SetEntry(table, pair.Key, pair.Value);
        }

        private static void SetEntry(StringTable table, string key, string value)
        {
            if (table == null)
                throw new InvalidOperationException("String table is required.");

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Localization key is required.", nameof(key));

            StringTableEntry entry = table.GetEntry(key);
            if (entry == null)
                entry = table.AddEntry(key, value ?? string.Empty);
            else
                entry.Value = value ?? string.Empty;

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);
        }

        private static void EnsureFolder(string parent, string folderName)
        {
            string target = parent + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(target))
                AssetDatabase.CreateFolder(parent, folderName);
        }

        private static StringTable GetStringTableOrThrow(StringTableCollection collection, Locale locale)
        {
            StringTable table = collection.GetTable(locale.Identifier) as StringTable;
            if (table == null)
                throw new InvalidOperationException($"Missing string table for locale '{locale.Identifier.Code}'.");

            return table;
        }

        private static T LoadAssetOrThrow<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                throw new InvalidOperationException($"Failed to load asset at path '{path}'.");

            return asset;
        }
    }
}
