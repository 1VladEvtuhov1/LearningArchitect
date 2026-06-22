using System;
using LearningArchitect.Core;
using LearningArchitect.Shared.Settings;
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

        [SerializeField] private ShowcaseLanguage currentLanguage = ShowcaseLanguage.English;
        [SerializeField] private Button languageToggleButton;
        [SerializeField] private TextMeshProUGUI languageToggleLabel;
        [SerializeField] private TextMeshProUGUI breadcrumbText;
        [SerializeField] private TextMeshProUGUI moduleHeaderText;
        [SerializeField] private TextMeshProUGUI variantHeaderText;
        [SerializeField] private TextMeshProUGUI stressHeaderText;
        [SerializeField] private TextMeshProUGUI statusTitleText;
        [SerializeField] private TextMeshProUGUI overviewTabText;
        [SerializeField] private TextMeshProUGUI architectureTabText;
        [SerializeField] private TextMeshProUGUI tradeOffsTabText;

        private bool initialized;
        private static bool localizationBackendAvailable = true;

        public static ShowcaseLocalization Instance { get; private set; }
        public static event Action<ShowcaseLanguage> LanguageChanged;

        public static ShowcaseLanguage CurrentLanguage
        {
            get
            {
                if (Instance == null)
                    return ShowcaseLanguage.English;

                return Instance.currentLanguage;
            }
        }

        internal static bool IsLocalizationBackendAvailable => localizationBackendAvailable;

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
            SetLanguage(currentLanguage == ShowcaseLanguage.English
                ? ShowcaseLanguage.Russian
                : ShowcaseLanguage.English);
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
            return ShowcaseLocalizationContent.GetText(language, key);
        }

        public static string GetModuleName(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.GetModuleName(module);
        }

        public static string GetModuleDescription(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.GetModuleDescription(module);
        }

        public static string GetModuleCategory(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.GetModuleCategory(module);
        }

        public static string GetModuleProblemStatement(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.GetModuleProblemStatement(module);
        }

        public static string GetModuleWebGlPresetNote(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.GetModuleWebGlPresetNote(module);
        }

        public static string GetModuleActiveItemLabel(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.GetModuleActiveItemLabel(module);
        }

        public static string GetVariantName(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantName(variant);
        }

        public static string GetVariantArchitectureDescription(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantArchitectureDescription(variant);
        }

        public static string GetVariantDataFlow(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantDataFlow(variant);
        }

        public static string GetVariantRuntimeLifecycle(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantRuntimeLifecycle(variant);
        }

        public static string GetVariantWhyThisApproach(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantWhyThisApproach(variant);
        }

        public static string GetVariantCompareSummary(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantCompareSummary(variant);
        }

        public static string GetVariantTakeaway(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantTakeaway(variant);
        }

        public static string GetVariantTradeOffs(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantTradeOffs(variant);
        }

        public static string GetVariantPros(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantPros(variant);
        }

        public static string GetVariantCons(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.GetVariantCons(variant);
        }

        public static string BuildModuleNameKey(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.BuildModuleNameKey(module);
        }

        public static string BuildModuleDescriptionKey(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.BuildModuleDescriptionKey(module);
        }

        public static string BuildModuleThesisKey(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.BuildModuleThesisKey(module);
        }

        public static string BuildModuleProblemKey(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.BuildModuleProblemKey(module);
        }

        public static string BuildModuleWebGlPresetKey(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.BuildModuleWebGlPresetKey(module);
        }

        public static string BuildModuleActiveItemLabelKey(ModuleDefinitionSO module)
        {
            return ShowcaseLocalizationContent.BuildModuleActiveItemLabelKey(module);
        }

        public static string BuildVariantNameKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantNameKey(variant);
        }

        public static string BuildVariantArchitectureKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantArchitectureKey(variant);
        }

        public static string BuildVariantCompareKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantCompareKey(variant);
        }

        public static string BuildVariantDataFlowKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantDataFlowKey(variant);
        }

        public static string BuildVariantRuntimeLifecycleKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantRuntimeLifecycleKey(variant);
        }

        public static string BuildVariantWhyThisApproachKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantWhyThisApproachKey(variant);
        }

        public static string BuildVariantTakeawayKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantTakeawayKey(variant);
        }

        public static string BuildVariantTradeOffsKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantTradeOffsKey(variant);
        }

        public static string BuildVariantProsKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantProsKey(variant);
        }

        public static string BuildVariantConsKey(VariantDefinitionSO variant)
        {
            return ShowcaseLocalizationContent.BuildVariantConsKey(variant);
        }

        public static string BuildContentKey(string assetName, string suffix)
        {
            return ShowcaseLocalizationContent.BuildContentKey(assetName, suffix);
        }

        internal static void DisableLocalizationBackend()
        {
            localizationBackendAvailable = false;
        }

        private void Initialize()
        {
            if (Instance != null && Instance != this)
                return;

            Instance = this;
            if (!initialized)
            {
                GameSettings.EnsureLoaded();
                currentLanguage = (ShowcaseLanguage)GameSettings.Current.language;
                if (!JsonFileGameSettingsStore.Exists())
                    currentLanguage = (ShowcaseLanguage)PlayerPrefs.GetInt(PlayerPrefsKey, (int)currentLanguage);

                ValidateReferences();
                initialized = true;
            }

            BindLanguageToggleButton();
            TrySetSelectedLocale(currentLanguage);
            ApplyChromeTexts();
        }

        private void ValidateReferences()
        {
            if (languageToggleButton == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(languageToggleButton)}.");

            if (languageToggleLabel == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(languageToggleLabel)}.");

            if (breadcrumbText == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(breadcrumbText)}.");

            if (moduleHeaderText == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(moduleHeaderText)}.");

            if (variantHeaderText == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(variantHeaderText)}.");

            if (stressHeaderText == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(stressHeaderText)}.");

            if (statusTitleText == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(statusTitleText)}.");

            if (overviewTabText == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(overviewTabText)}.");

            if (architectureTabText == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(architectureTabText)}.");

            if (tradeOffsTabText == null)
                throw new InvalidOperationException($"{nameof(ShowcaseLocalization)} requires {nameof(tradeOffsTabText)}.");
        }

        private void BindLanguageToggleButton()
        {
            languageToggleButton.onClick.RemoveListener(ToggleLanguage);
            languageToggleButton.onClick.AddListener(ToggleLanguage);
        }

        private void ApplyChromeTexts()
        {
            breadcrumbText.richText = true;
            breadcrumbText.text = GetText("breadcrumb");
            moduleHeaderText.text = GetText("select_module");
            variantHeaderText.text = GetText("select_variant");
            stressHeaderText.text = GetText("stress_test");
            statusTitleText.text = GetText("system_status");
            overviewTabText.text = GetText("overview");
            architectureTabText.text = GetText("architecture");
            tradeOffsTabText.text = GetText("trade_offs");
            languageToggleLabel.text = currentLanguage == ShowcaseLanguage.English ? "RU" : "EN";
        }

        private bool TrySetSelectedLocale(ShowcaseLanguage language)
        {
            if (!localizationBackendAvailable || !LocalizationSettings.HasSettings)
                return false;

            try
            {
                var operation = LocalizationSettings.InitializationOperation;
                if (!operation.IsDone)
                    return false;

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
            GameSettings.SetLanguage((int)currentLanguage);
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
                if (candidate?.Identifier.Code == null)
                    continue;

                if (candidate.Identifier.Code.StartsWith(code, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            return null;
        }

        private static ShowcaseLanguage MapLocaleToLanguage(Locale locale)
        {
            if (locale != null &&
                !string.IsNullOrEmpty(locale.Identifier.Code) &&
                locale.Identifier.Code.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            {
                return ShowcaseLanguage.Russian;
            }

            return ShowcaseLanguage.English;
        }

        private static void TrySubscribeToLocaleChanges()
        {
            if (!localizationBackendAvailable || Instance == null)
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
    }
}
