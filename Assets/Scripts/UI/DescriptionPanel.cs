using System;
using System.Collections.Generic;
using System.Text;
using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    public sealed class DescriptionPanel : MonoBehaviour, IPointerClickHandler
    {
        private sealed class SectionView
        {
            private const float VerticalSpacing = 12f;

            private readonly string rootName;
            private readonly Color defaultBodyColor;
            private readonly Vector2 bodyBaseMargin;

            private readonly RectTransform root;
            private readonly RectTransform headerRect;
            private readonly RectTransform bodyRect;
            private readonly TextMeshProUGUI header;
            private readonly TextMeshProUGUI body;
            private readonly LayoutElement layoutElement;

            public SectionView(string rootName, RectTransform root, TextMeshProUGUI header, TextMeshProUGUI body)
            {
                this.rootName = rootName;
                this.root = root;
                this.header = header;
                this.body = body;

                headerRect = header.rectTransform;
                bodyRect = body.rectTransform;
                defaultBodyColor = body.color;
                bodyBaseMargin = new Vector2(body.margin.x, body.margin.z);

                layoutElement = root.GetComponent<LayoutElement>();
                if (layoutElement == null)
                    layoutElement = root.gameObject.AddComponent<LayoutElement>();

                PrepareTransforms();
            }

            public string RootName => rootName;

            public RectTransform Root => root;

            public void SetVisible(bool isVisible)
            {
                root.gameObject.SetActive(isVisible);
            }

            public void ApplyContent(string title, string bodyText, Color titleColor, Color? bodyColor = null)
            {
                root.gameObject.SetActive(true);

                header.text = title;
                header.color = titleColor;
                body.text = bodyText;
                body.color = bodyColor ?? defaultBodyColor;

                Layout();
            }

            private void PrepareTransforms()
            {
                root.anchorMin = new Vector2(0f, 1f);
                root.anchorMax = new Vector2(1f, 1f);
                root.pivot = new Vector2(0.5f, 1f);
                root.sizeDelta = new Vector2(0f, root.sizeDelta.y);

                headerRect.anchorMin = new Vector2(0f, 1f);
                headerRect.anchorMax = new Vector2(1f, 1f);
                headerRect.pivot = new Vector2(0.5f, 1f);
                headerRect.anchoredPosition = Vector2.zero;
                headerRect.sizeDelta = new Vector2(0f, headerRect.sizeDelta.y);

                bodyRect.anchorMin = new Vector2(0f, 1f);
                bodyRect.anchorMax = new Vector2(1f, 1f);
                bodyRect.pivot = new Vector2(0.5f, 1f);
                bodyRect.sizeDelta = new Vector2(0f, bodyRect.sizeDelta.y);
            }

            private void Layout()
            {
                float rootWidth = root.rect.width;
                if (rootWidth < 1f && root.parent is RectTransform parentRect)
                    rootWidth = parentRect.rect.width;

                rootWidth = Mathf.Max(1f, rootWidth);
                float bodyAvailableWidth = Mathf.Max(1f, rootWidth - bodyBaseMargin.x - bodyBaseMargin.y);

                float headerHeight = Mathf.Max(header.preferredHeight, header.fontSize + 4f);
                float bodyHeight = Mathf.Max(body.GetPreferredValues(body.text, bodyAvailableWidth, 0f).y, body.fontSize + 4f);
                float totalHeight = Mathf.Ceil(headerHeight + VerticalSpacing + bodyHeight);

                headerRect.anchoredPosition = Vector2.zero;
                headerRect.sizeDelta = new Vector2(0f, headerHeight);

                bodyRect.anchoredPosition = new Vector2(0f, -(headerHeight + VerticalSpacing));
                bodyRect.sizeDelta = new Vector2(0f, bodyHeight);

                root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
                layoutElement.minHeight = totalHeight;
                layoutElement.preferredHeight = totalHeight;

                LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            }
        }

        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float fadeDuration = 0.16f;
        [SerializeField] private Color activeTabColor = default;
        [SerializeField] private Color inactiveTabColor = default;
        [SerializeField] private Color sectionTitleColor = default;
        [SerializeField] private Color positiveTextColor = default;
        [SerializeField] private Color negativeTextColor = default;

        private readonly StringBuilder builder = new(1024);
        private readonly TextMeshProUGUI[] tabLabels = new TextMeshProUGUI[3];
        private readonly List<SectionView> sectionOrder = new(5);

        private RectTransform activeTabUnderline;
        private Vector3 baseScale = Vector3.one;
        private RectTransform contentRoot;
        private CanvasGroup contentCanvasGroup;
        private int currentTabIndex;
        private ModuleDefinitionSO currentModule;
        private bool isCompositeMode;
        private bool isInitialized;
        private RectTransform legacyDescriptionRoot;
        private SectionView overviewSection;
        private SectionView architectureSection;
        private SectionView tradeOffsSection;
        private SectionView prosSection;
        private SectionView consSection;
        private float fadeTimer;
        private RectTransform tabsBar;
        private VariantDefinitionSO currentVariant;

        public TextMeshProUGUI DescriptionText
        {
            get => descriptionText;
            set => descriptionText = value;
        }

        public ScrollRect ScrollRect
        {
            get => scrollRect;
            set => scrollRect = value;
        }

        public Color SectionTitleColor
        {
            get => sectionTitleColor;
            set => sectionTitleColor = value;
        }

        public Color PositiveTextColor
        {
            get => positiveTextColor;
            set => positiveTextColor = value;
        }

        public Color NegativeTextColor
        {
            get => negativeTextColor;
            set => negativeTextColor = value;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            if (!EnsureInitialized())
                return;

            if (fadeTimer <= 0f)
                return;

            fadeTimer -= Time.unscaledDeltaTime;

            float normalized = fadeDuration <= 0f ? 1f : 1f - Mathf.Clamp01(fadeTimer / fadeDuration);
            float eased = normalized * normalized * (3f - 2f * normalized);

            if (isCompositeMode)
            {
                if (contentCanvasGroup != null)
                    contentCanvasGroup.alpha = eased;

                if (contentRoot != null)
                    contentRoot.localScale = baseScale * Mathf.Lerp(0.985f, 1f, eased);
            }
            else if (descriptionText != null)
            {
                descriptionText.alpha = eased;
                descriptionText.rectTransform.localScale = baseScale * Mathf.Lerp(0.985f, 1f, eased);
            }
        }

        public void SetVariant(VariantDefinitionSO variant)
        {
            if (!EnsureInitialized())
                return;

            SetContent(null, variant);
        }

        public void SetTab(int tabIndex)
        {
            if (!EnsureInitialized())
                return;

            currentTabIndex = Mathf.Clamp(tabIndex, 0, tabLabels.Length - 1);
            RefreshTabVisuals();
            RebuildContent();
        }

        public void SetContent(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (!EnsureInitialized())
                return;

            currentModule = module;
            currentVariant = variant;
            RebuildContent();
        }

        public void RefreshLocalizedContent()
        {
            if (!EnsureInitialized())
                return;

            for (int i = 0; i < tabLabels.Length; i++)
                tabLabels[i].text = GetTabTitle(i);

            RebuildContent();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!EnsureInitialized() || eventData.button != PointerEventData.InputButton.Left)
                return;

            for (int i = 0; i < tabLabels.Length; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(tabLabels[i].rectTransform, eventData.position, eventData.pressEventCamera))
                {
                    SetTab(i);
                    return;
                }
            }
        }

        private bool EnsureInitialized()
        {
            if (isInitialized)
                return true;

            ApplyPaletteDefaults();
            if (!TryResolveUi())
                return false;

            if (isCompositeMode)
            {
                baseScale = contentRoot == null ? Vector3.one : contentRoot.localScale;
                if (contentCanvasGroup == null && contentRoot != null)
                    contentCanvasGroup = contentRoot.gameObject.AddComponent<CanvasGroup>();
            }
            else
            {
                baseScale = descriptionText.rectTransform.localScale;
                descriptionText.textWrappingMode = TextWrappingModes.Normal;
                descriptionText.overflowMode = TextOverflowModes.Overflow;
            }

            isInitialized = true;
            return true;
        }

        private void ApplyPaletteDefaults()
        {
            if (activeTabColor == default)
                activeTabColor = ShowcasePalette.AccentMain;

            if (inactiveTabColor == default)
                inactiveTabColor = ShowcasePalette.TextSecondary;

            if (sectionTitleColor == default)
                sectionTitleColor = ShowcasePalette.AccentMain;

            if (positiveTextColor == default)
                positiveTextColor = ShowcasePalette.Success;

            if (negativeTextColor == default)
                negativeTextColor = ShowcasePalette.Error;
        }

        private bool TryResolveUi()
        {
            if (scrollRect == null)
                return false;

            RectTransform panelRoot = scrollRect.transform as RectTransform;
            if (panelRoot == null || scrollRect.viewport == null)
                return false;

            tabsBar = panelRoot.Find("TabsBar") as RectTransform;
            if (tabsBar == null)
                return false;

            activeTabUnderline = tabsBar.Find("ActiveTabUnderline") as RectTransform;
            if (activeTabUnderline == null)
                return false;

            for (int i = 0; i < tabLabels.Length; i++)
            {
                Transform tabTransform = tabsBar.Find("Tab_" + i);
                if (tabTransform == null)
                    return false;

                TextMeshProUGUI label = tabTransform.GetComponent<TextMeshProUGUI>();
                if (label == null)
                    return false;

                label.text = GetTabTitle(i);
                label.raycastTarget = true;
                tabLabels[i] = label;
            }

            legacyDescriptionRoot = descriptionText == null ? null : descriptionText.rectTransform;
            isCompositeMode = TryResolveCompositeUi(scrollRect.viewport);
            RefreshTabVisuals();
            return isCompositeMode || TryResolveLegacyUi();
        }

        private bool TryResolveCompositeUi(RectTransform viewport)
        {
            overviewSection = ResolveSection(viewport, "Container - AboutInfo");
            architectureSection = ResolveSection(viewport, "Container - ArchitectureInfo");
            tradeOffsSection = ResolveSection(viewport, "Container - Trade-OffsInfo");
            RectTransform prosConsRoot = FindDeep(viewport, "Container - ProsCons") as RectTransform;
            if (prosConsRoot == null)
                return false;

            prosSection = ResolveSection(prosConsRoot, "Container - Pros");
            consSection = ResolveSection(prosConsRoot, "Container - Cons");

            if (overviewSection == null || architectureSection == null || tradeOffsSection == null || prosSection == null || consSection == null)
                return false;

            sectionOrder.Clear();
            sectionOrder.Add(overviewSection);
            sectionOrder.Add(architectureSection);
            sectionOrder.Add(tradeOffsSection);
            sectionOrder.Add(prosSection);
            sectionOrder.Add(consSection);

            contentRoot = viewport.Find("ContentRoot") as RectTransform;
            if (contentRoot == null)
            {
                GameObject contentObject = new("ContentRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                contentRoot = contentObject.GetComponent<RectTransform>();
                contentRoot.SetParent(viewport, false);
            }

            contentRoot.anchorMin = new Vector2(0f, 1f);
            contentRoot.anchorMax = new Vector2(1f, 1f);
            contentRoot.pivot = new Vector2(0.5f, 1f);
            contentRoot.anchoredPosition = Vector2.zero;
            contentRoot.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup layoutGroup = contentRoot.GetComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = 18f;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter fitter = contentRoot.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < sectionOrder.Count; i++)
                sectionOrder[i].Root.SetParent(contentRoot, false);

            if (legacyDescriptionRoot != null)
            {
                legacyDescriptionRoot.SetParent(contentRoot, false);
                legacyDescriptionRoot.gameObject.SetActive(false);
            }

            scrollRect.content = contentRoot;
            contentCanvasGroup = contentRoot.GetComponent<CanvasGroup>();
            if (contentCanvasGroup == null)
                contentCanvasGroup = contentRoot.gameObject.AddComponent<CanvasGroup>();

            return true;
        }

        private bool TryResolveLegacyUi()
        {
            if (descriptionText == null)
                return false;

            if (scrollRect.content != descriptionText.rectTransform)
                return false;

            if (descriptionText.GetComponent<ContentSizeFitter>() == null)
                return false;

            return true;
        }

        private static SectionView ResolveSection(Transform root, string sectionName)
        {
            RectTransform sectionRoot = FindDeep(root, sectionName) as RectTransform;
            if (sectionRoot == null)
                return null;

            TextMeshProUGUI header = FindDeep(sectionRoot, "Text - Header")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI body = FindDeep(sectionRoot, "Text - Description")?.GetComponent<TextMeshProUGUI>();
            if (header == null || body == null)
                return null;

            return new SectionView(sectionName, sectionRoot, header, body);
        }

        private void RebuildContent()
        {
            if (isCompositeMode)
            {
                RebuildCompositeContent();
                return;
            }

            RebuildLegacyContent();
        }

        private void RebuildCompositeContent()
        {
            if (currentVariant == null)
            {
                ShowFallbackComposite();
                return;
            }

            string moduleDescription = ShowcaseLocalization.GetModuleDescription(currentModule);
            string moduleCategory = ShowcaseLocalization.GetModuleCategory(currentModule);
            string moduleProblem = ShowcaseLocalization.GetModuleProblemStatement(currentModule);
            string compareSummary = ShowcaseLocalization.GetVariantCompareSummary(currentVariant);
            string takeaway = ShowcaseLocalization.GetVariantTakeaway(currentVariant);
            string webGlPreset = ShowcaseLocalization.GetModuleWebGlPresetNote(currentModule);
            string architectureDescription = ShowcaseLocalization.GetVariantArchitectureDescription(currentVariant);
            string tradeOffs = ShowcaseLocalization.GetVariantTradeOffs(currentVariant);
            string pros = ShowcaseLocalization.GetVariantPros(currentVariant);
            string cons = ShowcaseLocalization.GetVariantCons(currentVariant);

            switch (currentTabIndex)
            {
                case 0:
                    ApplySection(overviewSection, 0, ShowcaseLocalization.GetText("about"),
                        FallbackTo(moduleDescription, ShowcaseLocalization.GetText("realtime_preview")));
                    ApplySection(architectureSection, 1, ShowcaseLocalization.GetText("problem"),
                        FallbackTo(moduleProblem, ShowcaseLocalization.GetText("no_problem_statement")));
                    ApplySection(tradeOffsSection, 2, ShowcaseLocalization.GetText("compare"),
                        FallbackTo(compareSummary, ShowcaseLocalization.GetText("no_compare_summary")));
                    ApplySection(prosSection, 3, ShowcaseLocalization.GetText("takeaway"),
                        FallbackTo(takeaway, ShowcaseLocalization.GetText("no_takeaway")));
                    ApplySection(consSection, 4, ShowcaseLocalization.GetText("webgl_preset"),
                        FallbackTo(webGlPreset, ShowcaseLocalization.GetText("no_webgl_note")));
                    break;

                case 1:
                    ApplySection(overviewSection, 0, ShowcaseLocalization.GetText("module_type"),
                        FallbackTo(moduleCategory, ShowcaseLocalization.GetText("no_module_type")));
                    ApplySection(architectureSection, 1, ShowcaseLocalization.GetText("architecture"),
                        FallbackTo(architectureDescription, ShowcaseLocalization.GetText("no_architecture_notes")));
                    ApplySection(tradeOffsSection, 2, ShowcaseLocalization.GetText("problem"),
                        FallbackTo(moduleProblem, ShowcaseLocalization.GetText("no_problem_statement")));
                    ApplySection(prosSection, 3, ShowcaseLocalization.GetText("strengths"),
                        FormatListBody(pros, ShowcaseLocalization.GetText("no_strengths"), positiveTextColor));
                    ApplySection(consSection, 4, ShowcaseLocalization.GetText("takeaway"),
                        FallbackTo(takeaway, ShowcaseLocalization.GetText("no_takeaway")));
                    break;

                default:
                    ApplySection(overviewSection, 0, ShowcaseLocalization.GetText("compare"),
                        FallbackTo(compareSummary, ShowcaseLocalization.GetText("no_compare_summary")));
                    ApplySection(architectureSection, 1, ShowcaseLocalization.GetText("trade_offs"),
                        FallbackTo(tradeOffs, ShowcaseLocalization.GetText("no_tradeoffs")));
                    ApplySection(tradeOffsSection, 2, ShowcaseLocalization.GetText("takeaway"),
                        FallbackTo(takeaway, ShowcaseLocalization.GetText("no_takeaway")));
                    ApplySection(prosSection, 3, ShowcaseLocalization.GetText("pros"),
                        FormatListBody(pros, ShowcaseLocalization.GetText("no_pros"), positiveTextColor));
                    ApplySection(consSection, 4, ShowcaseLocalization.GetText("cons"),
                        FormatListBody(cons, ShowcaseLocalization.GetText("no_cons"), negativeTextColor));
                    break;
            }

            PlayCompositeFade();
            ResetScroll();
            RefreshTabVisuals();
        }

        private void ApplySection(SectionView section, int siblingIndex, string title, string body)
        {
            if (section == null)
                return;

            section.Root.SetSiblingIndex(siblingIndex);
            section.ApplyContent(title, body, ResolveSectionTitleColor(title));
        }

        private void ShowFallbackComposite()
        {
            ApplySection(overviewSection, 0, ShowcaseLocalization.GetText("overview"), ShowcaseLocalization.GetText("no_variant"));

            if (architectureSection != null)
                architectureSection.SetVisible(false);
            if (tradeOffsSection != null)
                tradeOffsSection.SetVisible(false);
            if (prosSection != null)
                prosSection.SetVisible(false);
            if (consSection != null)
                consSection.SetVisible(false);

            PlayCompositeFade();
            ResetScroll();
            RefreshTabVisuals();
        }

        private void PlayCompositeFade()
        {
            if (contentCanvasGroup != null)
                contentCanvasGroup.alpha = 0f;

            if (contentRoot != null)
            {
                contentRoot.localScale = baseScale * 0.985f;
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            }

            fadeTimer = fadeDuration;
        }

        private void RebuildLegacyContent()
        {
            if (currentVariant == null)
            {
                descriptionText.text = ShowcaseLocalization.GetText("no_variant");
                descriptionText.alpha = 1f;
                ResetScroll();
                RefreshTabVisuals();
                return;
            }

            builder.Length = 0;

            switch (currentTabIndex)
            {
                case 0:
                    AppendSection(
                        ShowcaseLocalization.GetText("about"),
                        FallbackTo(ShowcaseLocalization.GetModuleDescription(currentModule), ShowcaseLocalization.GetText("realtime_preview")));
                    builder.Append("\n\n");
                    AppendSection(
                        ShowcaseLocalization.GetText("module_type"),
                        FallbackTo(ShowcaseLocalization.GetModuleCategory(currentModule), ShowcaseLocalization.GetText("no_module_type")));
                    builder.Append("\n\n");
                    AppendSection(
                        ShowcaseLocalization.GetText("problem"),
                        FallbackTo(ShowcaseLocalization.GetModuleProblemStatement(currentModule), ShowcaseLocalization.GetText("no_problem_statement")));
                    builder.Append("\n\n");
                    AppendSection(
                        ShowcaseLocalization.GetText("compare"),
                        FallbackTo(ShowcaseLocalization.GetVariantCompareSummary(currentVariant), ShowcaseLocalization.GetText("no_compare_summary")));
                    builder.Append("\n\n");
                    AppendSection(
                        ShowcaseLocalization.GetText("takeaway"),
                        FallbackTo(ShowcaseLocalization.GetVariantTakeaway(currentVariant), ShowcaseLocalization.GetText("no_takeaway")));
                    builder.Append("\n\n");
                    AppendSection(
                        ShowcaseLocalization.GetText("webgl_preset"),
                        FallbackTo(ShowcaseLocalization.GetModuleWebGlPresetNote(currentModule), ShowcaseLocalization.GetText("no_webgl_note")));
                    break;

                case 1:
                    AppendSection(
                        ShowcaseLocalization.GetText("architecture"),
                        FallbackTo(ShowcaseLocalization.GetVariantArchitectureDescription(currentVariant), ShowcaseLocalization.GetText("no_architecture_notes")));
                    builder.Append("\n\n");
                    AppendSection(
                        ShowcaseLocalization.GetText("strengths"),
                        FormatListBody(ShowcaseLocalization.GetVariantPros(currentVariant), ShowcaseLocalization.GetText("no_strengths"), positiveTextColor),
                        false);
                    break;

                default:
                    AppendSection(
                        ShowcaseLocalization.GetText("trade_offs"),
                        FallbackTo(ShowcaseLocalization.GetVariantTradeOffs(currentVariant), ShowcaseLocalization.GetText("no_tradeoffs")));
                    builder.Append("\n\n");
                    AppendSection(
                        ShowcaseLocalization.GetText("pros"),
                        FormatListBody(ShowcaseLocalization.GetVariantPros(currentVariant), ShowcaseLocalization.GetText("no_pros"), positiveTextColor),
                        false,
                        ToHex(positiveTextColor));
                    builder.Append("\n\n");
                    AppendSection(
                        ShowcaseLocalization.GetText("cons"),
                        FormatListBody(ShowcaseLocalization.GetVariantCons(currentVariant), ShowcaseLocalization.GetText("no_cons"), negativeTextColor),
                        false,
                        ToHex(negativeTextColor));
                    break;
            }

            descriptionText.alpha = 0f;
            descriptionText.rectTransform.localScale = baseScale * 0.985f;
            descriptionText.text = builder.ToString();
            fadeTimer = fadeDuration;
            ResetScroll();
            RefreshTabVisuals();
        }

        private void AppendSection(string title, string body, bool preserveSpacing = true, string titleColor = null)
        {
            titleColor ??= ToHex(sectionTitleColor);

            builder
                .Append("<color=")
                .Append(titleColor)
                .Append("><size=76%><b>")
                .Append(title)
                .Append("</b></size></color>\n");

            builder.Append(preserveSpacing ? body : body.Trim());
        }

        private void RefreshTabVisuals()
        {
            for (int i = 0; i < tabLabels.Length; i++)
            {
                tabLabels[i].color = i == currentTabIndex ? activeTabColor : inactiveTabColor;
                tabLabels[i].fontStyle = i == currentTabIndex ? FontStyles.Bold : FontStyles.Normal;
            }

            RectTransform activeTab = tabLabels[currentTabIndex].rectTransform;
            activeTabUnderline.anchoredPosition = new Vector2(activeTab.anchoredPosition.x - 2f, activeTabUnderline.anchoredPosition.y);
            activeTabUnderline.sizeDelta = new Vector2(activeTab.sizeDelta.x + 4f, activeTabUnderline.sizeDelta.y);
        }

        private void ResetScroll()
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private Color ResolveSectionTitleColor(string title)
        {
            string prosTitle = ShowcaseLocalization.GetText("pros");
            string consTitle = ShowcaseLocalization.GetText("cons");
            string strengthsTitle = ShowcaseLocalization.GetText("strengths");

            if (string.Equals(title, prosTitle, StringComparison.Ordinal) ||
                string.Equals(title, strengthsTitle, StringComparison.Ordinal))
            {
                return positiveTextColor;
            }

            if (string.Equals(title, consTitle, StringComparison.Ordinal))
                return negativeTextColor;

            return sectionTitleColor;
        }

        private static string FallbackTo(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string FormatListBody(string source, string fallback, Color bulletColor)
        {
            if (string.IsNullOrWhiteSpace(source))
                return fallback;

            string[] lines = source.Split('\n');
            StringBuilder list = new(source.Length + lines.Length * 8);
            string bulletHex = ToHex(bulletColor);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                    continue;

                string item = line.StartsWith("-") || line.StartsWith("+") || line.StartsWith("*")
                    ? line.Substring(1).Trim()
                    : line;

                if (list.Length > 0)
                    list.Append('\n');

                list.Append("<color=").Append(bulletHex).Append(">\u2022</color> ").Append(item);
            }

            return list.Length == 0 ? fallback : list.ToString();
        }

        private static string GetTabTitle(int index)
        {
            return index switch
            {
                0 => ShowcaseLocalization.GetText("overview"),
                1 => ShowcaseLocalization.GetText("architecture"),
                _ => ShowcaseLocalization.GetText("trade_offs")
            };
        }

        private static string ToHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
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
