using System;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Tests.UI
{
    [Category("LearningArchitect.UI.Edit")]
    public sealed class DescriptionPanelTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void OverviewTab_RendersExpectedSections_InLegacyViewport()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;
            DescriptionPanelHarness harness = null;

            try
            {
                module = CreateModule("ModuleAlpha", "Module description", "Problem statement", "WebGL note");
                variant = CreateVariant("VariantAlpha", compare: "Compare summary", takeaway: "Key takeaway");
                harness = DescriptionPanelHarness.Create();

                harness.Panel.SetContent(module, variant);
                string body = harness.GetRenderedBody();

                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("about")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("module_type")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("problem")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("compare")));
                AssertSectionOrder(
                    body,
                    ShowcaseLocalization.GetText("about"),
                    ShowcaseLocalization.GetText("module_type"),
                    ShowcaseLocalization.GetText("problem"),
                    ShowcaseLocalization.GetText("compare"));
            }
            finally
            {
                harness?.Dispose();
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variant);
            }
        }

        [Test]
        public void ArchitectureTab_ShowsMissingMarkers_WhenVariantDoesNotProvideOptionalContent()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;
            DescriptionPanelHarness harness = null;

            try
            {
                module = CreateModule("ModuleBeta", "Module description", "Architecture problem", string.Empty);
                variant = CreateVariant("VariantBeta", architecture: "Architecture notes", compare: string.Empty, takeaway: string.Empty, pros: string.Empty);
                harness = DescriptionPanelHarness.Create();

                harness.Panel.SetContent(module, variant);
                harness.Panel.SetTab(1);
                string body = harness.GetRenderedBody();

                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("module_type")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("problem")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("core_idea")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("data_flow")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("runtime_lifecycle")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("why_this_approach")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("strengths")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("watch_out")));
                Assert.That(body, Does.Contain("[MISSING: ShowcaseContent.variantbeta.data_flow]"));
                Assert.That(body, Does.Contain("[MISSING: ShowcaseContent.variantbeta.runtime_lifecycle]"));
                Assert.That(body, Does.Contain("[MISSING: ShowcaseContent.variantbeta.why_this_approach]"));
                Assert.That(body, Does.Contain("[MISSING: ShowcaseContent.variantbeta.pros]"));
                Assert.That(body, Does.Contain("[MISSING: ShowcaseContent.variantbeta.cons]"));
            }
            finally
            {
                harness?.Dispose();
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variant);
            }
        }

        [Test]
        public void ArchitectureTab_RendersRequiredSections_ForArchitectureContract()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;
            DescriptionPanelHarness harness = null;

            try
            {
                module = CreateModule("ModuleStructured", "Module description", "Scale update cost across thousands of entities", string.Empty);
                variant = CreateVariant(
                    "VariantStructured",
                    architecture: "A central coordinator owns update ordering and shared state.",
                    dataFlow: "Input -> Coordinator -> Systems -> Presenters",
                    runtimeLifecycle: "Bootstrap, simulate per frame, then release pooled visuals on deactivate.",
                    whyThisApproach: "It keeps ownership explicit and makes cross-system profiling easier.",
                    takeaway: "Good when global orchestration matters.",
                    pros: "- Predictable\n- Easy to profile",
                    cons: "- More coupling to the coordinator");
                harness = DescriptionPanelHarness.Create();

                harness.Panel.SetContent(module, variant);
                harness.Panel.SetTab(1);
                string body = harness.GetRenderedBody();

                AssertSectionOrder(
                    body,
                    ShowcaseLocalization.GetText("module_type"),
                    ShowcaseLocalization.GetText("problem"),
                    ShowcaseLocalization.GetText("core_idea"),
                    ShowcaseLocalization.GetText("data_flow"),
                    ShowcaseLocalization.GetText("runtime_lifecycle"),
                    ShowcaseLocalization.GetText("why_this_approach"),
                    ShowcaseLocalization.GetText("strengths"),
                    ShowcaseLocalization.GetText("watch_out"));
            }
            finally
            {
                harness?.Dispose();
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variant);
            }
        }
        [Test]
        public void FormatListBody_FormatsProsAndConsAsBulletLists()
        {
            string formattedPros = DescriptionPanel.FormatListBodyForEditModeTests("- Fast\n- Stable", "Fallback", new Color(0x22 / 255f, 0xC5 / 255f, 0x5E / 255f, 1f));
            string formattedCons = DescriptionPanel.FormatListBodyForEditModeTests("* Complex", "Fallback", new Color(0xEF / 255f, 0x44 / 255f, 0x44 / 255f, 1f));

            Assert.AreEqual("<color=#22C55E>•</color> Fast\n<color=#22C55E>•</color> Stable", formattedPros);
            Assert.AreEqual("<color=#EF4444>•</color> Complex", formattedCons);
        }

        [Test]
        public void NoVariant_RendersFallbackOverview()
        {
            DescriptionPanelHarness harness = null;

            try
            {
                harness = DescriptionPanelHarness.Create();
                harness.Panel.SetContent(null, null);

                string body = harness.GetRenderedBody();
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("overview")));
                Assert.That(body, Does.Contain(ShowcaseLocalization.GetText("no_variant")));
            }
            finally
            {
                harness?.Dispose();
            }
        }

        private static void AssertSectionOrder(string body, params string[] headers)
        {
            int previousIndex = -1;
            for (int i = 0; i < headers.Length; i++)
            {
                int currentIndex = body.IndexOf(headers[i], StringComparison.Ordinal);
                Assert.That(currentIndex, Is.GreaterThanOrEqualTo(0), $"Expected body to contain section '{headers[i]}'.");
                Assert.That(currentIndex, Is.GreaterThan(previousIndex), $"Section '{headers[i]}' should appear after the previous section.");
                previousIndex = currentIndex;
            }
        }

        private static ModuleDefinitionSO CreateModule(string assetName, string description, string problem, string webGlPreset)
        {
            _ = description;
            _ = problem;
            _ = webGlPreset;

            ModuleDefinitionSO module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();
            module.name = assetName;
            SetField(module, "localizationKey", assetName.ToLowerInvariant());
            SetCategory(module, ShowcaseModuleCategory.ArchitecturePatternModule);
            return module;
        }

        private static void SetCategory(ModuleDefinitionSO module, ShowcaseModuleCategory category)
        {
            SerializedObject so = new(module);
            so.FindProperty("category").enumValueIndex = (int)category;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static VariantDefinitionSO CreateVariant(
            string assetName,
            string architecture = "",
            string dataFlow = "",
            string runtimeLifecycle = "",
            string whyThisApproach = "",
            string compare = "",
            string takeaway = "",
            string tradeOffs = "",
            string pros = "",
            string cons = "")
        {
            _ = architecture;
            _ = dataFlow;
            _ = runtimeLifecycle;
            _ = whyThisApproach;
            _ = compare;
            _ = takeaway;
            _ = tradeOffs;
            _ = pros;
            _ = cons;

            VariantDefinitionSO variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();
            variant.name = assetName;
            SetField(variant, "localizationKey", assetName.ToLowerInvariant());
            return variant;
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

        private sealed class DescriptionPanelHarness
        {
            private readonly GameObject root;
            private readonly TextMeshProUGUI descriptionText;

            private DescriptionPanelHarness(GameObject root, DescriptionPanel panel, TextMeshProUGUI descriptionText)
            {
                this.root = root;
                Panel = panel;
                this.descriptionText = descriptionText;
            }

            public DescriptionPanel Panel { get; }

            public static DescriptionPanelHarness Create()
            {
                GameObject canvasRoot = new("DescriptionPanel Test Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = canvasRoot.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject panelHost = new("DescriptionPanel Host", typeof(RectTransform), typeof(DescriptionPanel));
                panelHost.transform.SetParent(canvasRoot.transform, false);

                GameObject scrollObject = new("DescriptionScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
                scrollObject.transform.SetParent(panelHost.transform, false);
                RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
                scrollRectTransform.sizeDelta = new Vector2(620f, 900f);

                ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
                DescriptionPanel panel = panelHost.GetComponent<DescriptionPanel>();

                RectTransform tabsBar = CreateRect("Layout - DescriptionTabs", scrollObject.transform, new Vector2(620f, 90f));
                tabsBar.anchorMin = new Vector2(0f, 1f);
                tabsBar.anchorMax = new Vector2(1f, 1f);
                tabsBar.pivot = new Vector2(0.5f, 1f);
                tabsBar.anchoredPosition = Vector2.zero;

                CreateLabel("Text - OverviewTab", tabsBar, "Overview");
                CreateLabel("Text - ArchitectureTab", tabsBar, "Architecture");
                CreateLabel("Text - TradeOffsTab", tabsBar, "Trade-offs");
                RectTransform underline = CreateRect("Image - ActiveTabUnderline", tabsBar, new Vector2(120f, 4f));
                underline.anchorMin = new Vector2(0f, 0f);
                underline.anchorMax = new Vector2(0f, 0f);

                RectTransform viewport = CreateRect("Container - Viewport", scrollObject.transform, new Vector2(620f, 760f));
                viewport.anchorMin = new Vector2(0f, 0f);
                viewport.anchorMax = new Vector2(1f, 1f);
                viewport.offsetMin = new Vector2(0f, 0f);
                viewport.offsetMax = new Vector2(0f, -100f);
                viewport.gameObject.AddComponent<Image>();
                viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

                TextMeshProUGUI descriptionText = CreateLabel("Text - Description", viewport, string.Empty, 20f);
                descriptionText.rectTransform.anchorMin = new Vector2(0f, 1f);
                descriptionText.rectTransform.anchorMax = new Vector2(1f, 1f);
                descriptionText.rectTransform.pivot = new Vector2(0.5f, 1f);
                descriptionText.rectTransform.offsetMin = new Vector2(16f, 0f);
                descriptionText.rectTransform.offsetMax = new Vector2(-16f, 0f);
                descriptionText.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                scrollRect.viewport = viewport;
                scrollRect.content = descriptionText.rectTransform;
                panel.ScrollRect = scrollRect;
                SetField(panel, "tabsBar", tabsBar);
                SetField(panel, "activeTabUnderline", underline);
                SetField(panel, "legacyDescriptionText", descriptionText);
                SetField(panel, "overviewTabLabel", tabsBar.Find("Text - OverviewTab").GetComponent<TextMeshProUGUI>());
                SetField(panel, "architectureTabLabel", tabsBar.Find("Text - ArchitectureTab").GetComponent<TextMeshProUGUI>());
                SetField(panel, "tradeOffsTabLabel", tabsBar.Find("Text - TradeOffsTab").GetComponent<TextMeshProUGUI>());

                canvasRoot.SetActive(true);
                Canvas.ForceUpdateCanvases();
                return new DescriptionPanelHarness(canvasRoot, panel, descriptionText);
            }

            public string GetRenderedBody()
            {
                Canvas.ForceUpdateCanvases();
                return descriptionText.text;
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            private static RectTransform CreateRect(string name, Transform parent, Vector2 size)
            {
                GameObject node = new(name, typeof(RectTransform), typeof(CanvasRenderer));
                node.transform.SetParent(parent, false);
                RectTransform rect = node.GetComponent<RectTransform>();
                rect.sizeDelta = size;
                return rect;
            }

            private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, float fontSize = 22f)
            {
                GameObject node = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                node.transform.SetParent(parent, false);
                RectTransform rect = node.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(0f, 60f);

                TextMeshProUGUI label = node.GetComponent<TextMeshProUGUI>();
                label.text = text;
                label.fontSize = fontSize;
                label.textWrappingMode = TextWrappingModes.Normal;
                label.richText = true;
                label.raycastTarget = false;
                label.color = Color.white;
                if (TMP_Settings.defaultFontAsset != null)
                    label.font = TMP_Settings.defaultFontAsset;

                return label;
            }
        }
    }
}

