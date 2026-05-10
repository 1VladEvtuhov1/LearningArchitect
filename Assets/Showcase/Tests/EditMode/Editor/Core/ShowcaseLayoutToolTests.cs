using System;
using System.Reflection;
using LearningArchitect.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ShowcaseLayoutToolTests
    {
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;

        [Test]
        public void ApplyLayout_CurrentPrefabShape_DoesNotCreateLegacyAliasNodes()
        {
            GameObject root = null;

            try
            {
                root = CreateMinimalCurrentShapeRoot();

                Assert.DoesNotThrow(() => InvokeApplyLayout(root));

                Assert.That(FindDeep(root.transform, "ModuleInfoPanel"), Is.Not.Null);
                Assert.That(FindDeep(root.transform, "HeaderPanel"), Is.Null);
                Assert.That(FindDeep(root.transform, "Text - ModuleName"), Is.Not.Null);
                Assert.That(FindDeep(root.transform, "ModuleName"), Is.Null);
                Assert.That(FindDeep(root.transform, "Container - ChartPlaceholder"), Is.Not.Null);
                Assert.That(FindDeep(root.transform, "ChartPlaceholder"), Is.Null);
                Assert.That(FindDeep(root.transform, "Text - InputHints"), Is.Not.Null);
                Assert.That(FindDeep(root.transform, "InputHints"), Is.Null);
                Assert.That(FindDeep(root.transform, "DescriptionText"), Is.Not.Null);
                Assert.That(FindDeep(root.transform, "ContentRoot"), Is.Null);
            }
            finally
            {
                DestroyImmediateSafe(root);
            }
        }

        [Test]
        public void ApplyLayout_PreservesLegacyDescriptionLayout_AndCleansStressDrift()
        {
            GameObject root = null;

            try
            {
                root = CreateMinimalCurrentShapeRoot();
                RemoveNode(root.transform, "DescriptionText");

                Transform nextModuleButton = EnsureNode(root.transform, "Canvas/RootFrame/ModuleInfoPanel/NavigationControlsPanel/NextModuleButton");
                CreateNode("ActiveGlow", nextModuleButton, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

                Transform stressSection = EnsureNode(root.transform, "Canvas/RootFrame/StressControlsPanel/Container - StressSection");
                CreateNode("Container - StressSummary", stressSection, typeof(RectTransform));

                InvokeApplyLayout(root);

                Transform descriptionText = FindDeep(root.transform, "DescriptionText");
                Assert.That(descriptionText, Is.Not.Null);

                Transform stressContent = EnsureNode(root.transform, "Canvas/RootFrame/StressControlsPanel/Container - StressSection/Container - StressContent");
                Assert.That(CountDirectChildrenByName(stressSection, "Container - StressSummary"), Is.EqualTo(0));
                Assert.That(CountDirectChildrenByName(stressContent, "Container - StressSummary"), Is.EqualTo(1));

                Assert.That(FindDirectChild(nextModuleButton, "ActiveGlow"), Is.Null);
                Assert.That(FindDirectChild(nextModuleButton, "Image - ActiveGlow"), Is.Not.Null);
            }
            finally
            {
                DestroyImmediateSafe(root);
            }
        }

        private static void InvokeApplyLayout(GameObject root)
        {
            MethodInfo applyLayout = typeof(ShowcaseLayoutTool).GetMethod("ApplyLayout", PrivateStatic);
            if (applyLayout == null)
                throw new MissingMethodException(typeof(ShowcaseLayoutTool).FullName, "ApplyLayout");

            applyLayout.Invoke(null, new object[] { root });
        }

        private static GameObject CreateMinimalCurrentShapeRoot()
        {
            GameObject root = new("ArchitectureShowcaseHub");
            CreateNode("ModuleRoot", root.transform);

            GameObject canvasObject = CreateNode(
                "Canvas",
                root.transform,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            GameObject rootFrame = CreateNode("RootFrame", canvasObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("ShowcaseBackgroundTint", canvasObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("TransitionOverlay", canvasObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));

            GameObject headerLine = CreateNode("HeaderLine", rootFrame.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("Breadcrumb", headerLine.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));

            GameObject moduleInfoPanel = CreateNode("ModuleInfoPanel", rootFrame.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("Image - ModuleIcon", moduleInfoPanel.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("Text - ModuleName", moduleInfoPanel.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("Text - VariantName", moduleInfoPanel.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("NavigationControlsPanel", moduleInfoPanel.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("HorizontalLayout - ModuleStats", moduleInfoPanel.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));

            GameObject performanceCard = CreateNode("PerformanceCard", rootFrame.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("Text - Header", performanceCard.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("Text - PerformanceInsight", performanceCard.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("Container - ChartPlaceholder", performanceCard.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("Container - PerformanceStats", performanceCard.transform, typeof(RectTransform), typeof(VerticalLayoutGroup));

            GameObject descriptionPanel = CreateNode("DescriptionPanel", rootFrame.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            GameObject tabsBar = CreateNode("Container - DescriptionCharacters", descriptionPanel.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            CreateNode("ActiveTabUnderline", tabsBar.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            GameObject viewport = CreateNode("Viewport", descriptionPanel.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            CreateNode("DescriptionText", viewport.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI), typeof(ContentSizeFitter));

            GameObject stressControls = CreateNode("StressControlsPanel", rootFrame.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            GameObject moduleSection = CreateNode("Container - ModuleSection", stressControls.transform, typeof(RectTransform));
            GameObject moduleSelector = CreateNode("ModuleSelector", moduleSection.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            CreateNode("Text - ModuleHeader", moduleSection.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("Text - ModuleValue", moduleSelector.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("Image - ModuleSelectorIcon", moduleSelector.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            GameObject variantSection = CreateNode("Container - VariantSection", stressControls.transform, typeof(RectTransform));
            GameObject variantSelector = CreateNode("VariantSelector", variantSection.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            CreateNode("Text - VariantHeader", variantSection.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("Text - VariantValue", variantSelector.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            GameObject previousVariantButton = CreateNode("PreviousVariantButton", variantSelector.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            CreateNode("Label", previousVariantButton.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            GameObject nextVariantButton = CreateNode("NextVariantButton", variantSelector.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            CreateNode("Label", nextVariantButton.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("Image - VariantSelectorIcon", variantSelector.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            GameObject stressSection = CreateNode("Container - StressSection", stressControls.transform, typeof(RectTransform));
            CreateNode("Text - StressHeader", stressSection.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            GameObject stressContent = CreateNode("Container - StressContent", stressSection.transform, typeof(RectTransform));
            GameObject stressRow = CreateNode("HorizontalLayout - StressPresets", stressContent.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            CreateStressButton("Button - StressPreset01", stressRow.transform);
            CreateStressButton("Button - StressPreset02", stressRow.transform);
            CreateStressButton("Button - StressPreset03", stressRow.transform);
            GameObject stressSummary = CreateNode("Container - StressSummary", stressContent.transform, typeof(RectTransform));
            CreateNode("Text - StressSummary", stressSummary.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            CreateNode("Image - StressStatusIcon", stressSummary.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            CreateNode("SystemStatusCard", rootFrame.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            return root;
        }

        private static void CreateStressButton(string name, Transform parent)
        {
            GameObject button = CreateNode(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            CreateNode("Text - PresetValue", button.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
        }

        private static GameObject CreateNode(string name, Transform parent, params Type[] components)
        {
            Type[] nodeComponents = components == null || components.Length == 0
                ? new[] { typeof(RectTransform) }
                : components;

            GameObject node = new(name, nodeComponents);
            node.transform.SetParent(parent, false);
            return node;
        }

        private static Transform EnsureNode(Transform root, string path)
        {
            string[] parts = path.Split('/');
            Transform current = root;

            for (int i = 0; i < parts.Length; i++)
            {
                Transform next = current.Find(parts[i]);
                if (next == null)
                    next = CreateNode(parts[i], current).transform;

                current = next;
            }

            return current;
        }

        private static void RemoveNode(Transform root, string name)
        {
            Transform node = FindDeep(root, name);
            if (node != null)
                UnityEngine.Object.DestroyImmediate(node.gameObject);
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

        private static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null)
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                    return child;
            }

            return null;
        }

        private static int CountDirectChildrenByName(Transform parent, string name)
        {
            if (parent == null)
                return 0;

            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == name)
                    count++;
            }

            return count;
        }

        private static void DestroyImmediateSafe(UnityEngine.Object target)
        {
            if (target != null)
                UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
