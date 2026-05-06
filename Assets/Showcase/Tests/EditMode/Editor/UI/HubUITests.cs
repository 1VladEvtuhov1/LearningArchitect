using System;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace LearningArchitect.Tests.UI
{
    [Category("LearningArchitect.UI.Edit")]
    public sealed class HubUITests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void ShowSelection_PopulatesModuleStatsFromResolvedLayout()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variantA = null;
            VariantDefinitionSO variantB = null;
            GameObject root = null;

            try
            {
                module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();
                variantA = ScriptableObject.CreateInstance<VariantDefinitionSO>();
                variantB = ScriptableObject.CreateInstance<VariantDefinitionSO>();

                module.name = "SyntheticEffectsModule";
                variantA.name = "SyntheticEffectsVariantA";
                variantB.name = "SyntheticEffectsVariantB";

                SetField(module, "moduleName", "Effects");
                SetField(module, "categoryLabel", "Simulation Module");
                SetField(module, "activeItemLabel", "Particles");
                SetField(module, "variants", new[] { variantA, variantB });
                SetField(variantA, "variantName", "Chunk");
                SetField(variantB, "variantName", "Indie");

                root = new GameObject("HubRoot", typeof(RectTransform));
                CreatePrimaryText(root.transform, "Text - ModuleName");
                CreatePrimaryText(root.transform, "Text - VariantName");
                CreatePrimaryText(root.transform, "Text - InputHints");
                CreatePrimaryText(root.transform, "Text - ModuleValue");
                CreatePrimaryText(root.transform, "Text - VariantValue");

                GameObject modulePanel = new GameObject("ModuleInfoPanel", typeof(RectTransform));
                modulePanel.transform.SetParent(root.transform, false);
                GameObject moduleStats = new GameObject("HorizontalLayout - ModuleStats", typeof(RectTransform));
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
                hubUi.InputHints = root.transform.Find("Text - InputHints").GetComponent<TextMeshProUGUI>();
                hubUi.ModuleSelectorName = root.transform.Find("Text - ModuleValue").GetComponent<TextMeshProUGUI>();
                hubUi.VariantSelectorName = root.transform.Find("Text - VariantValue").GetComponent<TextMeshProUGUI>();

                InvokePrivate(hubUi, "Awake");
                hubUi.ShowSelection(module, variantB);

                TextMeshProUGUI[] labels = moduleStats.GetComponentsInChildren<TextMeshProUGUI>(true);

                Assert.AreEqual("MODULE", FindText(labels, "Text - StatLabel", 0));
                Assert.AreEqual("Effects", FindText(labels, "Text - StatValue", 0));
                Assert.AreEqual("MODULE TYPE", FindText(labels, "Text - StatLabel", 1));
                Assert.AreEqual("Simulation Module", FindText(labels, "Text - StatValue", 1));
                Assert.AreEqual("VARIANT", FindText(labels, "Text - StatLabel", 2));
                Assert.AreEqual("Indie", FindText(labels, "Text - StatValue", 2));
                Assert.AreEqual("VARIANTS", FindText(labels, "Text - StatLabel", 3));
                Assert.AreEqual("2/2", FindText(labels, "Text - StatValue", 3));
                Assert.AreEqual("Active items", FindText(labels, "Text - StatLabel", 4));
                Assert.AreEqual("Particles", FindText(labels, "Text - StatValue", 4));
            }
            finally
            {
                DestroyImmediateSafe(root);
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variantA);
                DestroyImmediateSafe(variantB);
            }
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

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, PrivateInstance);
            if (method == null)
                throw new MissingMethodException(target.GetType().FullName, methodName);

            method.Invoke(target, null);
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
    }
}
