using System;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace LearningArchitect.Tests.UI
{
    [Category("LearningArchitect.UI.Edit")]
    public sealed class ShowcaseLocalizationTableTests
    {
        private const string EnglishTablePath = "Assets/Showcase/Localization/Tables/ShowcaseContent_en.asset";
        private const string RussianTablePath = "Assets/Showcase/Localization/Tables/ShowcaseContent_ru.asset";
        private const string EffectsModulePath = "Assets/Modules/EffectsSystem/Data/EffectsModule.asset";
        private const string ChunkVariantPath = "Assets/Modules/EffectsSystem/Data/Effects_ChunkVariant.asset";
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly FieldInfo InstanceField =
            typeof(ShowcaseLocalization).GetField("<Instance>k__BackingField", PrivateStatic);
        private static readonly FieldInfo CurrentLanguageField =
            typeof(ShowcaseLocalization).GetField("currentLanguage", PrivateInstance);
        private static readonly FieldInfo InitializedField =
            typeof(ShowcaseLocalization).GetField("initialized", PrivateInstance);
        private static readonly FieldInfo LocalizationBackendAvailableField =
            typeof(ShowcaseLocalization).GetField("localizationBackendAvailable", PrivateStatic);

        private Locale previousLocale;
        private GameObject localizationRoot;

        [SetUp]
        public void SetUp()
        {
            Assert.IsTrue(LocalizationSettings.HasSettings, "LocalizationSettings must be configured for table-backed tests.");
            previousLocale = LocalizationSettings.SelectedLocale;
            LocalizationBackendAvailableField.SetValue(null, true);
            InstanceField.SetValue(null, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (localizationRoot != null)
                UnityEngine.Object.DestroyImmediate(localizationRoot);

            localizationRoot = null;
            LocalizationBackendAvailableField.SetValue(null, true);

            if (LocalizationSettings.HasSettings && previousLocale != null)
                LocalizationSettings.SelectedLocale = previousLocale;

            InstanceField.SetValue(null, null);
        }

        [Test]
        public void GetModuleDescription_UsesEnglishTableEntry_ForRealModuleAsset()
        {
            ModuleDefinitionSO module = AssetDatabase.LoadAssetAtPath<ModuleDefinitionSO>(EffectsModulePath);
            StringTable englishTable = AssetDatabase.LoadAssetAtPath<StringTable>(EnglishTablePath);
            Assert.IsNotNull(module);
            Assert.IsNotNull(englishTable);

            ConfigureLocalizationInstance(ShowcaseLanguage.English);
            SetLocale("en");

            string key = ShowcaseLocalization.BuildModuleDescriptionKey(module);
            string expected = englishTable.GetEntry(key)?.Value;
            Assert.IsFalse(string.IsNullOrWhiteSpace(expected), $"Expected a localized value for key '{key}'.");

            Assert.AreEqual(expected, ShowcaseLocalization.GetModuleDescription(module));
        }

        [Test]
        public void GetVariantArchitectureDescription_UsesRussianTableEntry_ForRealVariantAsset()
        {
            VariantDefinitionSO variant = AssetDatabase.LoadAssetAtPath<VariantDefinitionSO>(ChunkVariantPath);
            StringTable russianTable = AssetDatabase.LoadAssetAtPath<StringTable>(RussianTablePath);
            Assert.IsNotNull(variant);
            Assert.IsNotNull(russianTable);

            ConfigureLocalizationInstance(ShowcaseLanguage.Russian);
            SetLocale("ru");

            string key = ShowcaseLocalization.BuildVariantArchitectureKey(variant);
            string expected = russianTable.GetEntry(key)?.Value;
            Assert.IsFalse(string.IsNullOrWhiteSpace(expected), $"Expected a localized value for key '{key}'.");

            Assert.AreEqual(expected, ShowcaseLocalization.GetVariantArchitectureDescription(variant));
        }

        [Test]
        public void MissingContentKey_ReturnsMissingMarker_WithRealTablesConfigured()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;

            try
            {
                module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();
                module.name = "SyntheticMissingLocalizationModule";
                SetField(module, "localizationKey", "synthetic_missing_localization_module");

                variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();
                variant.name = "SyntheticMissingLocalizationVariant";
                SetField(variant, "localizationKey", "synthetic_missing_localization_variant");

                ConfigureLocalizationInstance(ShowcaseLanguage.Russian);
                SetLocale("ru");
                Assert.AreEqual("[MISSING: ShowcaseContent.synthetic_missing_localization_module.description]", ShowcaseLocalization.GetModuleDescription(module));
                Assert.AreEqual("[MISSING: ShowcaseContent.synthetic_missing_localization_variant.architecture]", ShowcaseLocalization.GetVariantArchitectureDescription(variant));

                ConfigureLocalizationInstance(ShowcaseLanguage.English);
                SetLocale("en");
                Assert.AreEqual("[MISSING: ShowcaseContent.synthetic_missing_localization_module.description]", ShowcaseLocalization.GetModuleDescription(module));
                Assert.AreEqual("[MISSING: ShowcaseContent.synthetic_missing_localization_variant.architecture]", ShowcaseLocalization.GetVariantArchitectureDescription(variant));
            }
            finally
            {
                if (module != null)
                    UnityEngine.Object.DestroyImmediate(module);

                if (variant != null)
                    UnityEngine.Object.DestroyImmediate(variant);
            }
        }

        [Test]
        public void BuildContentKeys_UseExplicitLocalizationKey_WhenAssigned()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;

            try
            {
                module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();
                variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();

                module.name = "ModuleNameShouldNotMatter";
                variant.name = "VariantNameShouldNotMatter";

                SetField(module, "localizationKey", "module.explicit_id");
                SetField(variant, "localizationKey", "variant.explicit_id");

                Assert.AreEqual("module.explicit_id.description", ShowcaseLocalization.BuildModuleDescriptionKey(module));
                Assert.AreEqual("variant.explicit_id.architecture", ShowcaseLocalization.BuildVariantArchitectureKey(variant));
            }
            finally
            {
                if (module != null)
                    UnityEngine.Object.DestroyImmediate(module);

                if (variant != null)
                    UnityEngine.Object.DestroyImmediate(variant);
            }
        }

        private void ConfigureLocalizationInstance(ShowcaseLanguage language)
        {
            if (localizationRoot == null)
                localizationRoot = new GameObject("ShowcaseLocalization Test Harness");

            ShowcaseLocalization localization = localizationRoot.GetComponent<ShowcaseLocalization>();
            if (localization == null)
                localization = localizationRoot.AddComponent<ShowcaseLocalization>();

            CurrentLanguageField.SetValue(localization, language);
            InitializedField.SetValue(localization, true);
            InstanceField.SetValue(null, localization);
            LocalizationBackendAvailableField.SetValue(null, true);
        }

        private static void SetLocale(string localeCode)
        {
            Locale locale = LocalizationSettings.AvailableLocales?.GetLocale(localeCode);
            Assert.IsNotNull(locale, $"Locale '{localeCode}' must exist for localization tests.");
            LocalizationSettings.SelectedLocale = locale;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(target.GetType().FullName, fieldName);

            field.SetValue(target, value);
        }
    }
}
