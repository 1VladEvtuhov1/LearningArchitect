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
        private const string RootFolder = "Assets/Showcase/Localization";
        private const string LocalesFolder = "Assets/Showcase/Localization/Locales";
        private const string TablesFolder = "Assets/Showcase/Localization/Tables";
        private const string SettingsPath = "Assets/Showcase/Localization/ShowcaseLocalizationSettings.asset";
        private static readonly string[] DefinitionSearchFolders =
        {
            "Assets/Showcase/Data",
            "Assets/Modules"
        };

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
                { "module_category_architecture", "Architecture Pattern Module" },
                { "module_category_simulation", "Simulation Module" },
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
                { "tooltip_next_variant", "Next variant" },
                { "interview_arena_launch", "Interview Arena" },
                { "interview_arena_title", "Interview Arena" },
                { "interview_arena_subtitle", "Soldiers: melee, ranged crossbow, respawn. Top-left i-frame bar; blue flash on the hero." },
                { "interview_arena_back", "Back to architecture demos" }
            };

            Dictionary<string, string> russianEntries = new Dictionary<string, string>
            {
                { "breadcrumb", "СИСТЕМЫ ДВИЖКА / <color=#" + ShowcasePalette.AccentHex + ">ВИЗУАЛИЗАТОР</color>" },
                { "module", "МОДУЛЬ" },
                { "module_category_architecture", "Архитектурный паттерн" },
                { "module_category_simulation", "Симуляционный модуль" },
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
                { "tooltip_next_variant", "Следующий вариант" },
                { "interview_arena_launch", "Interview Arena" },
                { "interview_arena_title", "Interview Arena" },
                { "interview_arena_subtitle", "Солдаты: melee, арбалет на дистанции, респавн. Слева — индикатор i-frames (голубая вспышка на герое)." },
                { "interview_arena_back", "Назад к архитектурным демо" }
            };

            WriteEntries(englishTable, englishEntries);
            WriteEntries(russianTable, russianEntries);

            AddMissingUiEntries(englishTable, russianTable);
        }

        private static void PopulateContentCollection(StringTableCollection collection, Locale english, Locale russian)
        {
            GetStringTableOrThrow(collection, english);
            GetStringTableOrThrow(collection, russian);
            // ShowcaseContent rows are authored in the string tables only; module/variant SOs hold keys and wiring.
        }

        private static void WriteEntries(StringTable table, Dictionary<string, string> entries)
        {
            foreach (KeyValuePair<string, string> pair in entries)
                SetEntry(table, pair.Key, pair.Value);
        }

        private static void AddMissingUiEntries(StringTable englishTable, StringTable russianTable)
        {
            SetEntry(englishTable, "module_group", "GROUP");
            SetEntry(russianTable, "module_group", "ГРУППА");
            SetEntry(englishTable, "module_type", "MODULE TYPE");
            SetEntry(russianTable, "module_type", "ТИП МОДУЛЯ");
            SetEntry(englishTable, "variants", "VARIANTS");
            SetEntry(russianTable, "variants", "ВАРИАНТЫ");
            SetEntry(englishTable, "start_demo", "START DEMO");
            SetEntry(russianTable, "start_demo", "ЗАПУСК ДЕМО");
            SetEntry(englishTable, "stop_demo", "STOP DEMO");
            SetEntry(russianTable, "stop_demo", "СТОП ДЕМО");
            SetEntry(englishTable, "problem", "PROBLEM");
            SetEntry(russianTable, "problem", "ПРОБЛЕМА");
            SetEntry(englishTable, "compare", "COMPARE");
            SetEntry(russianTable, "compare", "СРАВНЕНИЕ");
            SetEntry(englishTable, "takeaway", "TAKEAWAY");
            SetEntry(russianTable, "takeaway", "ВЫВОД");
            SetEntry(englishTable, "webgl_preset", "WEBGL PRESET");
            SetEntry(russianTable, "webgl_preset", "WEBGL ПРЕСЕТ");
            SetEntry(englishTable, "core_idea", "Core Idea");
            SetEntry(russianTable, "core_idea", "Ключевая идея");
            SetEntry(englishTable, "data_flow", "Data Flow");
            SetEntry(russianTable, "data_flow", "Поток данных");
            SetEntry(englishTable, "runtime_lifecycle", "Runtime Lifecycle");
            SetEntry(russianTable, "runtime_lifecycle", "Жизненный цикл runtime");
            SetEntry(englishTable, "why_this_approach", "Why This Approach");
            SetEntry(russianTable, "why_this_approach", "Почему этот подход");
            SetEntry(englishTable, "active_items", "Active items");
            SetEntry(russianTable, "active_items", "Активные элементы");
            SetEntry(englishTable, "no_problem_statement", "No problem statement provided.");
            SetEntry(russianTable, "no_problem_statement", "Постановка проблемы не указана.");
            SetEntry(englishTable, "no_compare_summary", "No comparison summary provided.");
            SetEntry(russianTable, "no_compare_summary", "Сравнение не описано.");
            SetEntry(englishTable, "no_takeaway", "No takeaway provided.");
            SetEntry(russianTable, "no_takeaway", "Ключевой вывод не указан.");
            SetEntry(englishTable, "no_data_flow", "No data flow notes provided.");
            SetEntry(russianTable, "no_data_flow", "Поток данных не описан.");
            SetEntry(englishTable, "no_runtime_lifecycle", "No runtime lifecycle notes provided.");
            SetEntry(russianTable, "no_runtime_lifecycle", "Жизненный цикл runtime не описан.");
            SetEntry(englishTable, "no_why_this_approach", "No rationale for this approach provided.");
            SetEntry(russianTable, "no_why_this_approach", "Обоснование выбора подхода не указано.");
            SetEntry(englishTable, "no_module_type", "No module type specified.");
            SetEntry(russianTable, "no_module_type", "Тип модуля не указан.");
            SetEntry(englishTable, "no_webgl_note", "No WebGL note provided.");
            SetEntry(russianTable, "no_webgl_note", "WebGL-пресет не описан.");
            SetEntry(englishTable, "module_category_architecture", "Architecture Pattern Module");
            SetEntry(russianTable, "module_category_architecture", "Архитектурный паттерн");
            SetEntry(englishTable, "module_category_simulation", "Simulation Module");
            SetEntry(russianTable, "module_category_simulation", "Симуляционный модуль");
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
