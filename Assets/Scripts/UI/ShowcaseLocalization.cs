using System;
using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    public sealed class ShowcaseLocalization : MonoBehaviour
    {
        public const string PlayerPrefsKey = "ShowcaseLanguage";
        public const string UiTableName = "ShowcaseUI";
        public const string ContentTableName = "ShowcaseContent";

        public static ShowcaseLocalization Instance { get; private set; }
        public static event Action<ShowcaseLanguage> LanguageChanged;

        [SerializeField] private ShowcaseLanguage currentLanguage = ShowcaseLanguage.English;

        private Button languageToggleButton;
        private TextMeshProUGUI languageToggleLabel;
        private bool initialized;
        private static bool localizationBackendAvailable = true;

        public static ShowcaseLanguage CurrentLanguage
        {
            get
            {
                if (Instance == null)
                    return ShowcaseLanguage.English;

                return Instance.currentLanguage;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            LanguageChanged += HandleLanguageChangedInternal;
            TrySubscribeToLocaleChanges();
        }

        private void OnDisable()
        {
            TryUnsubscribeFromLocaleChanges();
            LanguageChanged -= HandleLanguageChangedInternal;
            if (Instance == this)
                Instance = null;
        }

        public void ToggleLanguage()
        {
            SetLanguage(currentLanguage == ShowcaseLanguage.English ? ShowcaseLanguage.Russian : ShowcaseLanguage.English);
        }

        public void SetLanguage(ShowcaseLanguage language)
        {
            if (TrySetSelectedLocale(language))
                return;

            ApplyLanguage(language, true);
        }

        public static string GetText(string key)
        {
            return GetText(CurrentLanguage, key);
        }

        public static string GetText(ShowcaseLanguage language, string key)
        {
            return GetLocalizedString(UiTableName, key, GetUiFallbackText(language, key), language);
        }

        public static string GetModuleName(ModuleDefinitionSO module)
        {
            if (module == null)
                return GetText("no_module");

            return GetContentText(BuildModuleNameKey(module), module.moduleName, module.moduleNameRu);
        }

        public static string GetModuleDescription(ModuleDefinitionSO module)
        {
            if (module == null)
                return string.Empty;

            return GetContentText(BuildModuleDescriptionKey(module), module.description, module.descriptionRu);
        }

        public static string GetVariantName(VariantDefinitionSO variant)
        {
            if (variant == null)
                return GetText("no_variant");

            return GetContentText(BuildVariantNameKey(variant), variant.variantName, variant.variantNameRu);
        }

        public static string GetVariantArchitectureDescription(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetContentText(BuildVariantArchitectureKey(variant), variant.architectureDescription, variant.architectureDescriptionRu);
        }

        public static string GetVariantTradeOffs(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetContentText(BuildVariantTradeOffsKey(variant), variant.tradeOffs, variant.tradeOffsRu);
        }

        public static string GetVariantPros(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetContentText(BuildVariantProsKey(variant), variant.pros, variant.prosRu);
        }

        public static string GetVariantCons(VariantDefinitionSO variant)
        {
            if (variant == null)
                return string.Empty;

            return GetContentText(BuildVariantConsKey(variant), variant.cons, variant.consRu);
        }

        public static string BuildModuleNameKey(ModuleDefinitionSO module)
        {
            return BuildContentKey(module == null ? string.Empty : module.name, "module_name");
        }

        public static string BuildModuleDescriptionKey(ModuleDefinitionSO module)
        {
            return BuildContentKey(module == null ? string.Empty : module.name, "description");
        }

        public static string BuildVariantNameKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.name, "variant_name");
        }

        public static string BuildVariantArchitectureKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.name, "architecture");
        }

        public static string BuildVariantTradeOffsKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.name, "trade_offs");
        }

        public static string BuildVariantProsKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.name, "pros");
        }

        public static string BuildVariantConsKey(VariantDefinitionSO variant)
        {
            return BuildContentKey(variant == null ? string.Empty : variant.name, "cons");
        }

        public static string BuildContentKey(string assetName, string suffix)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return suffix;

            char[] buffer = assetName.ToLowerInvariant().ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {
                char current = buffer[i];
                if ((current >= 'a' && current <= 'z') || (current >= '0' && current <= '9'))
                    continue;

                buffer[i] = '_';
            }

            string sanitized = new string(buffer).Trim('_');
            return sanitized + "." + suffix;
        }

        private void Initialize()
        {
            if (Instance != null && Instance != this)
                return;

            Instance = this;
            if (!initialized)
            {
                currentLanguage = (ShowcaseLanguage)PlayerPrefs.GetInt(PlayerPrefsKey, (int)currentLanguage);
                initialized = true;
            }

            ResolveButton();
            TrySetSelectedLocale(currentLanguage);
            ApplyChromeTexts();
        }

        private void ResolveButton()
        {
            Canvas canvas = FindCanvasByChild(transform, "RootFrame");
            if (canvas == null)
                return;

            Transform buttonTransform = FindDeep(canvas.transform, "LanguageToggleButton");
            if (buttonTransform == null)
                return;

            languageToggleButton = buttonTransform.GetComponent<Button>();
            Transform labelTransform = buttonTransform.Find("Label");
            languageToggleLabel = labelTransform == null ? null : labelTransform.GetComponent<TextMeshProUGUI>();

            if (languageToggleButton != null)
            {
                languageToggleButton.onClick.RemoveAllListeners();
                languageToggleButton.onClick.AddListener(ToggleLanguage);
            }
        }

        private void ApplyChromeTexts()
        {
            Canvas canvas = FindCanvasByChild(transform, "RootFrame");
            if (canvas == null)
                return;

            SetText(canvas.transform, "Breadcrumb", GetText("breadcrumb"), true);
            SetText(canvas.transform, "ModuleSectionTitle", GetText("select_module"));
            SetText(canvas.transform, "VariantSectionTitle", GetText("select_variant"));
            SetText(canvas.transform, "StressSectionTitle", GetText("stress_test"));
            SetText(canvas.transform, "StatusTitle", GetText("system_status"));
            SetText(canvas.transform, "Tab_0", GetText("overview"));
            SetText(canvas.transform, "Tab_1", GetText("architecture"));
            SetText(canvas.transform, "Tab_2", GetText("trade_offs"));

            string[] statKeys = { "module", "variant", "loaded", "orbit", "zoom" };
            for (int i = 0; i < statKeys.Length; i++)
                SetText(canvas.transform, "StatLabel_" + i, GetText(statKeys[i]));

            if (languageToggleLabel != null)
                languageToggleLabel.text = currentLanguage == ShowcaseLanguage.English ? "RU" : "EN";
        }

        private bool TrySetSelectedLocale(ShowcaseLanguage language)
        {
            if (!localizationBackendAvailable)
                return false;

            if (!LocalizationSettings.HasSettings)
                return false;

            try
            {
                var operation = LocalizationSettings.InitializationOperation;
                if (!operation.IsDone)
                    operation.WaitForCompletion();

                Locale locale = GetLocaleForLanguage(language);
                if (locale == null)
                    return false;

                if (LocalizationSettings.SelectedLocale != locale)
                {
                    LocalizationSettings.SelectedLocale = locale;
                    return true;
                }

                ApplyLanguage(MapLocaleToLanguage(locale), false);
                return true;
            }
            catch
            {
                localizationBackendAvailable = false;
                return false;
            }
        }

        private void HandleSelectedLocaleChanged(Locale locale)
        {
            ApplyLanguage(MapLocaleToLanguage(locale), true);
        }

        private void ApplyLanguage(ShowcaseLanguage language, bool raiseEvent)
        {
            currentLanguage = language;
            PlayerPrefs.SetInt(PlayerPrefsKey, (int)currentLanguage);
            PlayerPrefs.Save();
            ApplyChromeTexts();

            if (raiseEvent)
                LanguageChanged?.Invoke(currentLanguage);
        }

        private void HandleLanguageChangedInternal(ShowcaseLanguage language)
        {
            if (Instance != this)
                return;

            ApplyChromeTexts();
        }

        private static Locale GetLocaleForLanguage(ShowcaseLanguage language)
        {
            var provider = LocalizationSettings.AvailableLocales;
            if (provider == null)
                return null;

            string code = language == ShowcaseLanguage.Russian ? "ru" : "en";
            Locale locale = provider.GetLocale(code);
            if (locale != null)
                return locale;

            for (int i = 0; i < provider.Locales.Count; i++)
            {
                Locale candidate = provider.Locales[i];
                if (candidate == null || candidate.Identifier.Code == null)
                    continue;

                if (candidate.Identifier.Code.StartsWith(code, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            return null;
        }

        private static ShowcaseLanguage MapLocaleToLanguage(Locale locale)
        {
            if (locale != null && !string.IsNullOrEmpty(locale.Identifier.Code) &&
                locale.Identifier.Code.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            {
                return ShowcaseLanguage.Russian;
            }

            return ShowcaseLanguage.English;
        }

        private static string GetContentText(string entryKey, string englishFallback, string russianFallback)
        {
            return GetLocalizedString(ContentTableName, entryKey, GetPreferredFallback(CurrentLanguage, englishFallback, russianFallback), CurrentLanguage);
        }

        private static string GetLocalizedString(string tableName, string entryKey, string fallback, ShowcaseLanguage language)
        {
            if (localizationBackendAvailable &&
                !string.IsNullOrWhiteSpace(entryKey) &&
                LocalizationSettings.HasSettings)
            {
                try
                {
                    if (LocalizationSettings.SelectedLocale == null)
                        return !string.IsNullOrWhiteSpace(fallback) ? fallback : GetUiFallbackText(language, entryKey);

                    string localized = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, entryKey);
                    if (!string.IsNullOrWhiteSpace(localized) && !string.Equals(localized, entryKey, StringComparison.Ordinal))
                        return localized;
                }
                catch
                {
                    localizationBackendAvailable = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback;

            return GetUiFallbackText(language, entryKey);
        }

        private static string GetPreferredFallback(ShowcaseLanguage language, string englishFallback, string russianFallback)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(russianFallback))
                return russianFallback;

            if (!string.IsNullOrWhiteSpace(englishFallback))
                return englishFallback;

            return russianFallback;
        }

        private static string GetUiFallbackText(ShowcaseLanguage language, string key)
        {
            switch (key)
            {
                case "breadcrumb":
                    return "ENGINE SYSTEMS / <color=#" + ShowcasePalette.AccentHex + ">VISUALIZER</color>";
                case "module":
                    return "MODULE";
                case "variant":
                    return "VARIANT";
                case "loaded":
                    return "LOADED";
                case "orbit":
                    return "ORBIT";
                case "zoom":
                    return "ZOOM";
                case "performance":
                    return "PERFORMANCE";
                case "system_status":
                    return "SYSTEM STATUS";
                case "select_module":
                    return "SELECT MODULE";
                case "select_variant":
                    return "SELECT VARIANT";
                case "stress_test":
                    return "STRESS TEST";
                case "overview":
                    return language == ShowcaseLanguage.Russian ? "Обзор" : "Overview";
                case "architecture":
                    return language == ShowcaseLanguage.Russian ? "Архитектура" : "Architecture";
                case "trade_offs":
                    return language == ShowcaseLanguage.Russian ? "Компромиссы" : "Trade-offs";
                case "about":
                    return language == ShowcaseLanguage.Russian ? "ОПИСАНИЕ" : "ABOUT";
                case "key_features":
                    return language == ShowcaseLanguage.Russian ? "КЛЮЧЕВЫЕ ПЛЮСЫ" : "KEY FEATURES";
                case "watch_out":
                    return language == ShowcaseLanguage.Russian ? "ОГРАНИЧЕНИЯ" : "WATCH OUT";
                case "strengths":
                    return language == ShowcaseLanguage.Russian ? "СИЛЬНЫЕ СТОРОНЫ" : "STRENGTHS";
                case "pros":
                    return language == ShowcaseLanguage.Russian ? "ПЛЮСЫ" : "PROS";
                case "cons":
                    return language == ShowcaseLanguage.Russian ? "МИНУСЫ" : "CONS";
                case "no_module":
                    return language == ShowcaseLanguage.Russian ? "Модуль не выбран." : "No module selected.";
                case "no_variant":
                    return language == ShowcaseLanguage.Russian ? "Вариант не выбран." : "No variant selected.";
                case "no_architecture_notes":
                    return language == ShowcaseLanguage.Russian ? "Описание архитектуры отсутствует." : "No architecture notes provided.";
                case "no_tradeoffs":
                    return language == ShowcaseLanguage.Russian ? "Описание компромиссов отсутствует." : "No trade-off notes provided.";
                case "no_key_features":
                    return language == ShowcaseLanguage.Russian ? "Ключевые преимущества не указаны." : "No key features listed.";
                case "no_strengths":
                    return language == ShowcaseLanguage.Russian ? "Сильные стороны не указаны." : "No strengths listed.";
                case "no_pros":
                    return language == ShowcaseLanguage.Russian ? "Плюсы не указаны." : "No pros listed.";
                case "no_cons":
                    return language == ShowcaseLanguage.Russian ? "Минусы не указаны." : "No cons listed.";
                case "no_constraints":
                    return language == ShowcaseLanguage.Russian ? "Ограничения не указаны." : "No constraints listed.";
                case "realtime_preview":
                    return language == ShowcaseLanguage.Russian ? "Интерактивный просмотр архитектуры" : "Realtime architecture preview";
                case "switching_module_prev":
                    return language == ShowcaseLanguage.Russian ? "Переключение модуля  ‹" : "Switching module  ‹";
                case "switching_module_next":
                    return language == ShowcaseLanguage.Russian ? "Переключение модуля  ›" : "Switching module  ›";
                case "switching_variant_prev":
                    return language == ShowcaseLanguage.Russian ? "Переключение варианта  ‹" : "Switching variant  ‹";
                case "switching_variant_next":
                    return language == ShowcaseLanguage.Russian ? "Переключение варианта  ›" : "Switching variant  ›";
                case "load_selected":
                    return language == ShowcaseLanguage.Russian ? "Нагрузка выбрана" : "Load selected";
                case "stress_load":
                    return language == ShowcaseLanguage.Russian ? "Нагрузка" : "Stress load";
                case "active":
                    return language == ShowcaseLanguage.Russian ? "Активно" : "Active";
                case "fps":
                    return "FPS";
                case "frame_time":
                    return language == ShowcaseLanguage.Russian ? "Время кадра" : "Frame Time";
                case "particles":
                    return language == ShowcaseLanguage.Russian ? "Частицы" : "Particles";
                case "na":
                    return language == ShowcaseLanguage.Russian ? "Н/Д" : "N/A";
                case "tooltip_prev_module":
                    return language == ShowcaseLanguage.Russian ? "Предыдущий модуль" : "Previous module";
                case "tooltip_next_module":
                    return language == ShowcaseLanguage.Russian ? "Следующий модуль" : "Next module";
                case "tooltip_prev_variant":
                    return language == ShowcaseLanguage.Russian ? "Предыдущий вариант" : "Previous variant";
                case "tooltip_next_variant":
                    return language == ShowcaseLanguage.Russian ? "Следующий вариант" : "Next variant";
                default:
                    return key;
            }
        }

        private static void TrySubscribeToLocaleChanges()
        {
            if (!localizationBackendAvailable)
                return;

            try
            {
                if (LocalizationSettings.HasSettings)
                    LocalizationSettings.SelectedLocaleChanged += Instance.HandleSelectedLocaleChanged;
            }
            catch
            {
                localizationBackendAvailable = false;
            }
        }

        private static void TryUnsubscribeFromLocaleChanges()
        {
            if (!localizationBackendAvailable || Instance == null)
                return;

            try
            {
                if (LocalizationSettings.HasSettings)
                    LocalizationSettings.SelectedLocaleChanged -= Instance.HandleSelectedLocaleChanged;
            }
            catch
            {
                localizationBackendAvailable = false;
            }
        }

        private static void SetText(Transform root, string name, string value, bool richText = false)
        {
            Transform target = FindDeep(root, name);
            if (target == null)
                return;

            TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();
            if (text == null)
                return;

            text.richText = richText;
            text.text = value;
        }

        private static Canvas FindCanvasByChild(Transform root, string childName)
        {
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (FindDeep(canvases[i].transform, childName) != null)
                    return canvases[i];
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
    }
}
