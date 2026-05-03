using System;
using System.Collections.Generic;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Tests.UI
{
    [Category("LearningArchitect.UI.Edit")]
    public sealed class DescriptionPanelTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void OverviewTab_CreatesSixVisibleSections_WhenOptionalContentExists()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;
            DescriptionPanelHarness harness = null;

            try
            {
                module = CreateModule(
                    "ModuleAlpha",
                    description: "Module description",
                    problem: "Problem statement",
                    webGlPreset: "WebGL note");
                variant = CreateVariant(
                    "VariantAlpha",
                    compare: "Compare summary",
                    takeaway: "Key takeaway");
                harness = DescriptionPanelHarness.Create();

                harness.Panel.SetContent(module, variant);

                List<SectionSnapshot> visibleSections = harness.GetVisibleSections();

                Assert.AreEqual(6, visibleSections.Count);
                Assert.AreEqual(ShowcaseLocalization.GetText("about"), visibleSections[0].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("module_type"), visibleSections[1].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("problem"), visibleSections[2].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("compare"), visibleSections[3].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("takeaway"), visibleSections[4].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("webgl_preset"), visibleSections[5].Header);
                Assert.AreEqual("WebGL note", visibleSections[5].Body);
                Assert.IsTrue(harness.HasRuntimeSectionClone());
            }
            finally
            {
                harness?.Dispose();
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variant);
            }
        }

        [Test]
        public void ArchitectureTab_HidesOptionalSections_WhenVariantDoesNotProvideThem()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;
            DescriptionPanelHarness harness = null;

            try
            {
                module = CreateModule(
                    "ModuleBeta",
                    description: "Module description",
                    problem: "Architecture problem",
                    webGlPreset: string.Empty);
                variant = CreateVariant(
                    "VariantBeta",
                    architecture: "Architecture notes",
                    compare: string.Empty,
                    takeaway: string.Empty,
                    pros: string.Empty);
                harness = DescriptionPanelHarness.Create();

                harness.Panel.SetContent(module, variant);
                harness.Panel.SetTab(1);

                List<SectionSnapshot> visibleSections = harness.GetVisibleSections();

                Assert.AreEqual(4, visibleSections.Count);
                Assert.AreEqual(ShowcaseLocalization.GetText("module_type"), visibleSections[0].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("problem"), visibleSections[1].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("core_idea"), visibleSections[2].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("watch_out"), visibleSections[3].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("no_constraints"), visibleSections[3].Body);
                Assert.IsFalse(harness.HasVisibleSection(ShowcaseLocalization.GetText("strengths")));
                Assert.IsFalse(harness.HasVisibleSection(ShowcaseLocalization.GetText("data_flow")));
                Assert.IsFalse(harness.HasVisibleSection(ShowcaseLocalization.GetText("runtime_lifecycle")));
                Assert.IsFalse(harness.HasVisibleSection(ShowcaseLocalization.GetText("why_this_approach")));
                Assert.IsFalse(harness.HasVisibleSection(ShowcaseLocalization.GetText("takeaway")));
            }
            finally
            {
                harness?.Dispose();
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variant);
            }
        }

        [Test]
        public void ArchitectureTab_ShowsStructuredArchitectureSections_WhenVariantProvidesThem()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;
            DescriptionPanelHarness harness = null;

            try
            {
                module = CreateModule(
                    "ModuleStructured",
                    description: "Module description",
                    problem: "Scale update cost across thousands of entities",
                    webGlPreset: string.Empty);
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

                List<SectionSnapshot> visibleSections = harness.GetVisibleSections();

                Assert.AreEqual(9, visibleSections.Count);
                Assert.AreEqual(ShowcaseLocalization.GetText("module_type"), visibleSections[0].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("problem"), visibleSections[1].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("core_idea"), visibleSections[2].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("data_flow"), visibleSections[3].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("runtime_lifecycle"), visibleSections[4].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("why_this_approach"), visibleSections[5].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("strengths"), visibleSections[6].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("watch_out"), visibleSections[7].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("takeaway"), visibleSections[8].Header);
            }
            finally
            {
                harness?.Dispose();
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variant);
            }
        }

        [Test]
        public void TradeOffsTab_FormatsProsAndConsAsBulletLists()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;
            DescriptionPanelHarness harness = null;

            try
            {
                module = CreateModule(
                    "ModuleGamma",
                    description: "Module description",
                    problem: "Problem statement",
                    webGlPreset: string.Empty);
                variant = CreateVariant(
                    "VariantGamma",
                    compare: "Compare summary",
                    tradeOffs: "Trade-off summary",
                    pros: "- Fast\n- Stable",
                    cons: "* Complex");
                harness = DescriptionPanelHarness.Create();

                harness.Panel.SetContent(module, variant);
                harness.Panel.SetTab(2);

                List<SectionSnapshot> visibleSections = harness.GetVisibleSections();

                Assert.AreEqual(4, visibleSections.Count);
                Assert.AreEqual(ShowcaseLocalization.GetText("compare"), visibleSections[0].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("trade_offs"), visibleSections[1].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("pros"), visibleSections[2].Header);
                Assert.AreEqual("<color=#22C55E>\u2022</color> Fast\n<color=#22C55E>\u2022</color> Stable", visibleSections[2].Body);
                Assert.AreEqual(ShowcaseLocalization.GetText("cons"), visibleSections[3].Header);
                Assert.AreEqual("<color=#EF4444>\u2022</color> Complex", visibleSections[3].Body);
            }
            finally
            {
                harness?.Dispose();
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variant);
            }
        }

        [Test]
        public void LegacyTabContainerStructure_StillInitializesAndBuildsCompositeContent()
        {
            ModuleDefinitionSO module = null;
            VariantDefinitionSO variant = null;
            DescriptionPanelHarness harness = null;

            try
            {
                module = CreateModule(
                    "ModuleLegacy",
                    description: "Legacy module description",
                    problem: "Legacy problem",
                    webGlPreset: string.Empty);
                variant = CreateVariant(
                    "VariantLegacy",
                    compare: "Legacy compare",
                    takeaway: "Legacy takeaway");
                harness = DescriptionPanelHarness.Create(useLegacyTabs: true);

                harness.Panel.SetContent(module, variant);

                List<SectionSnapshot> visibleSections = harness.GetVisibleSections();

                Assert.AreEqual(5, visibleSections.Count);
                Assert.AreEqual(ShowcaseLocalization.GetText("about"), visibleSections[0].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("module_type"), visibleSections[1].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("problem"), visibleSections[2].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("compare"), visibleSections[3].Header);
                Assert.AreEqual(ShowcaseLocalization.GetText("takeaway"), visibleSections[4].Header);
            }
            finally
            {
                harness?.Dispose();
                DestroyImmediateSafe(module);
                DestroyImmediateSafe(variant);
            }
        }

        private static ModuleDefinitionSO CreateModule(string assetName, string description, string problem, string webGlPreset)
        {
            ModuleDefinitionSO module = ScriptableObject.CreateInstance<ModuleDefinitionSO>();
            module.name = assetName;
            SetField(module, "moduleName", assetName);
            SetField(module, "description", description);
            SetField(module, "problemStatement", problem);
            SetField(module, "webGlPresetNote", webGlPreset);
            SetField(module, "categoryLabel", "Architecture Pattern Module");
            return module;
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
            VariantDefinitionSO variant = ScriptableObject.CreateInstance<VariantDefinitionSO>();
            variant.name = assetName;
            SetField(variant, "variantName", assetName);
            SetField(variant, "architectureDescription", architecture);
            SetField(variant, "dataFlow", dataFlow);
            SetField(variant, "runtimeLifecycle", runtimeLifecycle);
            SetField(variant, "whyThisApproach", whyThisApproach);
            SetField(variant, "compareSummary", compare);
            SetField(variant, "takeaway", takeaway);
            SetField(variant, "tradeOffs", tradeOffs);
            SetField(variant, "pros", pros);
            SetField(variant, "cons", cons);
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

        private readonly struct SectionSnapshot
        {
            public SectionSnapshot(string name, string header, string body)
            {
                Name = name;
                Header = header;
                Body = body;
            }

            public string Name { get; }
            public string Header { get; }
            public string Body { get; }
        }

        private sealed class DescriptionPanelHarness
        {
            private readonly GameObject root;
            private readonly RectTransform viewport;
            private readonly List<GameObject> sectionRoots = new();

            private DescriptionPanelHarness(GameObject root, DescriptionPanel panel, RectTransform viewport)
            {
                this.root = root;
                Panel = panel;
                this.viewport = viewport;
            }

            public DescriptionPanel Panel { get; }

            public static DescriptionPanelHarness Create(bool useLegacyTabs = false)
            {
                GameObject canvasRoot = new("DescriptionPanel Test Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = canvasRoot.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject scrollObject = new("DescriptionScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(DescriptionPanel));
                scrollObject.transform.SetParent(canvasRoot.transform, false);
                RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
                scrollRectTransform.sizeDelta = new Vector2(620f, 900f);

                ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
                DescriptionPanel panel = scrollObject.GetComponent<DescriptionPanel>();

                string tabsRootName = useLegacyTabs ? "Container - DescriptionCharacters" : "TabsBar";
                RectTransform tabsBar = CreateRect(tabsRootName, scrollObject.transform, new Vector2(620f, 90f));
                tabsBar.anchorMin = new Vector2(0f, 1f);
                tabsBar.anchorMax = new Vector2(1f, 1f);
                tabsBar.pivot = new Vector2(0.5f, 1f);
                tabsBar.anchoredPosition = Vector2.zero;

                string firstTabName = useLegacyTabs ? "Text - CharacterName" : "Tab_0";
                string secondTabName = useLegacyTabs ? "Text - CharacterName" : "Tab_1";
                string thirdTabName = useLegacyTabs ? "Text - CharacterName" : "Tab_2";
                CreateLabel(firstTabName, tabsBar, "Overview");
                CreateLabel(secondTabName, tabsBar, "Architecture");
                CreateLabel(thirdTabName, tabsBar, "Trade-offs");
                RectTransform underline = CreateRect("ActiveTabUnderline", tabsBar, new Vector2(120f, 4f));
                underline.anchorMin = new Vector2(0f, 0f);
                underline.anchorMax = new Vector2(0f, 0f);

                RectTransform viewport = CreateRect("Viewport", scrollObject.transform, new Vector2(620f, 760f));
                viewport.anchorMin = new Vector2(0f, 0f);
                viewport.anchorMax = new Vector2(1f, 1f);
                viewport.offsetMin = new Vector2(0f, 0f);
                viewport.offsetMax = new Vector2(0f, -100f);
                viewport.gameObject.AddComponent<Image>();
                viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

                DescriptionPanelHarness harness = new(canvasRoot, panel, viewport);
                harness.BuildCompositeSections();

                scrollRect.viewport = viewport;
                panel.ScrollRect = scrollRect;

                canvasRoot.SetActive(true);
                Canvas.ForceUpdateCanvases();
                return harness;
            }

            public List<SectionSnapshot> GetVisibleSections()
            {
                Canvas.ForceUpdateCanvases();

                RectTransform contentRoot = viewport.Find("ContentRoot") as RectTransform;
                List<SectionSnapshot> snapshots = new();
                if (contentRoot == null)
                    return snapshots;

                for (int i = 0; i < contentRoot.childCount; i++)
                {
                    Transform child = contentRoot.GetChild(i);
                    if (!child.gameObject.activeSelf)
                        continue;

                    TextMeshProUGUI header = FindDeep(child, "Text - Header")?.GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI body = FindDeep(child, "Text - Description")?.GetComponent<TextMeshProUGUI>();
                    if (header == null || body == null)
                        continue;

                    snapshots.Add(new SectionSnapshot(child.name, header.text, body.text));
                }

                return snapshots;
            }

            public bool HasRuntimeSectionClone()
            {
                RectTransform contentRoot = viewport.Find("ContentRoot") as RectTransform;
                if (contentRoot == null)
                    return false;

                for (int i = 0; i < contentRoot.childCount; i++)
                {
                    if (contentRoot.GetChild(i).name.StartsWith("RuntimeSection_", StringComparison.Ordinal))
                        return true;
                }

                return false;
            }

            public bool HasVisibleSection(string header)
            {
                List<SectionSnapshot> sections = GetVisibleSections();
                for (int i = 0; i < sections.Count; i++)
                {
                    if (sections[i].Header == header)
                        return true;
                }

                return false;
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            private void BuildCompositeSections()
            {
                sectionRoots.Add(CreateSection("Container - AboutInfo", viewport));
                sectionRoots.Add(CreateSection("Container - ArchitectureInfo", viewport));
                sectionRoots.Add(CreateSection("Container - Trade-OffsInfo", viewport));

                RectTransform prosConsRoot = CreateRect("Container - ProsCons", viewport, new Vector2(620f, 240f));
                sectionRoots.Add(CreateSection("Container - Pros", prosConsRoot));
                sectionRoots.Add(CreateSection("Container - Cons", prosConsRoot));
            }

            private static GameObject CreateSection(string sectionName, Transform parent)
            {
                RectTransform root = CreateRect(sectionName, parent, new Vector2(620f, 160f));
                root.gameObject.AddComponent<Image>();
                root.gameObject.AddComponent<LayoutElement>();
                CreateLabel("Text - Header", root, sectionName + " Header", 24f);
                TextMeshProUGUI body = CreateLabel("Text - Description", root, sectionName + " Body", 20f);
                body.richText = true;
                return root.gameObject;
            }

            private static RectTransform CreateRect(string name, Transform parent, Vector2 size)
            {
                GameObject node = new(name, typeof(RectTransform), typeof(CanvasRenderer));
                node.transform.SetParent(parent, false);
                RectTransform rect = node.GetComponent<RectTransform>();
                rect.sizeDelta = size;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
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
}
