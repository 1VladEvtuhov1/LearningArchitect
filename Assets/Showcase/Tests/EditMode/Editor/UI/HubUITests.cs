using System;
using System.Collections.Generic;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LearningArchitect.Tests.UI
{
    [Category("LearningArchitect.UI.Edit")]
    public sealed class HubUITests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;

        private const string EffectsModulePath = "Assets/Modules/EffectsSystem/Data/EffectsModule.asset";

        private static readonly FieldInfo InstanceField =
            typeof(ShowcaseLocalization).GetField("<Instance>k__BackingField", PrivateStatic);
        private static readonly FieldInfo CurrentLanguageField =
            typeof(ShowcaseLocalization).GetField("currentLanguage", PrivateInstance);
        private static readonly FieldInfo InitializedField =
            typeof(ShowcaseLocalization).GetField("initialized", PrivateInstance);
        private static readonly FieldInfo LocalizationBackendAvailableField =
            typeof(ShowcaseLocalization).GetField("localizationBackendAvailable", PrivateStatic);

        private Locale previousLocale;
        private GameObject localizationHarness;

        [SetUp]
        public void SetUp()
        {
            Assert.IsTrue(LocalizationSettings.HasSettings, "LocalizationSettings must be configured for HubUI tests.");
            previousLocale = LocalizationSettings.SelectedLocale;
            LocalizationBackendAvailableField.SetValue(null, true);
            InstanceField.SetValue(null, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (localizationHarness != null)
                UnityEngine.Object.DestroyImmediate(localizationHarness);

            localizationHarness = null;
            LocalizationBackendAvailableField.SetValue(null, true);

            if (LocalizationSettings.HasSettings && previousLocale != null)
                LocalizationSettings.SelectedLocale = previousLocale;

            InstanceField.SetValue(null, null);
        }

        [Test]
        public void ShowSelection_PopulatesModuleStatsFromResolvedLayout()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variantB = null;
            GameObject root = null;

            try
            {
                module = AssetDatabase.LoadAssetAtPath<ModuleDefinitionSO>(EffectsModulePath);
                Assert.IsNotNull(module);
                Assert.That(module.Variants, Is.Not.Null.And.Not.Empty);
                variantB = module.Variants[1];
                Assert.IsNotNull(variantB);

                ConfigureLocalizationHarness(ShowcaseLanguage.English);
                SetLocale("en");

                root = new GameObject("HubRoot", typeof(RectTransform));
                CreatePrimaryText(root.transform, "Text - ModuleName");
                CreatePrimaryText(root.transform, "Text - VariantName");
                CreatePrimaryText(root.transform, "Text - ModuleValue");
                CreatePrimaryText(root.transform, "Text - VariantValue");

                GameObject modulePanel = new GameObject("Container - ModuleInfo", typeof(RectTransform));
                modulePanel.transform.SetParent(root.transform, false);
                GameObject moduleStats = new GameObject("Layout - ModuleStats", typeof(RectTransform));
                moduleStats.transform.SetParent(modulePanel.transform, false);

                for (int i = 0; i < 5; i++)
                {
                    GameObject statRoot = new GameObject("Container - ModuleStat (" + i + ")", typeof(RectTransform));
                    statRoot.transform.SetParent(moduleStats.transform, false);
                    CreatePrimaryText(statRoot.transform, "Text - StatLabel");
                    CreatePrimaryText(statRoot.transform, "Text - StatValue");
                }

                HubUI hubUi = root.AddComponent<HubUI>();
                hubUi.ModuleName = root.transform.Find("Text - ModuleName").GetComponent<TextMeshProUGUI>();
                hubUi.VariantName = root.transform.Find("Text - VariantName").GetComponent<TextMeshProUGUI>();
                hubUi.ModuleSelectorName = root.transform.Find("Text - ModuleValue").GetComponent<TextMeshProUGUI>();
                hubUi.VariantSelectorName = root.transform.Find("Text - VariantValue").GetComponent<TextMeshProUGUI>();
                SetField(hubUi, "moduleStatsContainer", moduleStats.GetComponent<RectTransform>());
                SetField(hubUi, "moduleStats", CreateModuleStatBindings(moduleStats.transform));

                hubUi.RunInitializeForEditModeTests();
                hubUi.ShowSelection(module, variantB);

                TextMeshProUGUI[] labels = moduleStats.GetComponentsInChildren<TextMeshProUGUI>(true);
                string expectedModuleName = ShowcaseLocalization.GetModuleName(module);
                string expectedModuleCategory = ShowcaseLocalization.GetModuleCategory(module);
                string expectedVariantName = ShowcaseLocalization.GetVariantName(variantB);
                string expectedActiveItemsValue = ShowcaseLocalization.GetModuleActiveItemLabel(module);

                Assert.AreEqual("MODULE", FindText(labels, "Text - StatLabel", 0));
                Assert.AreEqual(expectedModuleName, FindText(labels, "Text - StatValue", 0));
                Assert.AreEqual("MODULE TYPE", FindText(labels, "Text - StatLabel", 1));
                Assert.AreEqual(expectedModuleCategory, FindText(labels, "Text - StatValue", 1));
                Assert.AreEqual("VARIANT", FindText(labels, "Text - StatLabel", 2));
                Assert.AreEqual(expectedVariantName, FindText(labels, "Text - StatValue", 2));
                Assert.AreEqual("VARIANTS", FindText(labels, "Text - StatLabel", 3));
                Assert.AreEqual("2/2", FindText(labels, "Text - StatValue", 3));
                Assert.AreEqual(ShowcaseLocalization.GetText("active_items"), FindText(labels, "Text - StatLabel", 4));
                Assert.AreEqual(expectedActiveItemsValue, FindText(labels, "Text - StatValue", 4));
            }
            finally
            {
                DestroyImmediateSafe(root);
            }
        }

        private void ConfigureLocalizationHarness(ShowcaseLanguage language)
        {
            if (localizationHarness == null)
                localizationHarness = new GameObject("HubUI Localization Harness");

            ShowcaseLocalization localization = localizationHarness.GetComponent<ShowcaseLocalization>();
            if (localization == null)
                localization = localizationHarness.AddComponent<ShowcaseLocalization>();

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

        private static TextMeshProUGUI CreatePrimaryText(Transform parent, string name)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            return textObject.GetComponent<TextMeshProUGUI>();
        }

        private static string FindText(TextMeshProUGUI[] texts, string objectName, int occurrenceIndex)
        {
            int currentIndex = 0;
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name != objectName)
                    continue;

                if (currentIndex == occurrenceIndex)
                    return texts[i].text;

                currentIndex++;
            }

            Assert.Fail("Could not find text object '" + objectName + "' at index " + occurrenceIndex + ".");
            return string.Empty;
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

        private static object CreateModuleStatBindings(Transform moduleStatsRoot)
        {
            Type bindingType = typeof(HubUI).GetNestedType("ModuleStatBinding", BindingFlags.NonPublic);
            if (bindingType == null)
                throw new MissingMemberException(typeof(HubUI).FullName, "ModuleStatBinding");

            List<Transform> statRoots = new();
            for (int i = 0; i < moduleStatsRoot.childCount; i++)
                statRoots.Add(moduleStatsRoot.GetChild(i));

            Array bindings = Array.CreateInstance(bindingType, statRoots.Count);
            for (int i = 0; i < statRoots.Count; i++)
            {
                object binding = Activator.CreateInstance(bindingType);
                bindingType.GetField("root").SetValue(binding, statRoots[i].GetComponent<RectTransform>());
                bindingType.GetField("label").SetValue(binding, statRoots[i].Find("Text - StatLabel").GetComponent<TextMeshProUGUI>());
                bindingType.GetField("value").SetValue(binding, statRoots[i].Find("Text - StatValue").GetComponent<TextMeshProUGUI>());
                bindings.SetValue(binding, i);
            }

            return bindings;
        }
    }
}
