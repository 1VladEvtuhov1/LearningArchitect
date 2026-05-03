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
        private readonly struct SectionDefinition
        {
            public SectionDefinition(string title, string value, string emptyFallback, bool hideWhenEmpty, bool treatAsList = false, Color? bulletColor = null)
            {
                Title = title;
                Value = value;
                EmptyFallback = emptyFallback;
                HideWhenEmpty = hideWhenEmpty;
                TreatAsList = treatAsList;
                BulletColor = bulletColor ?? Color.white;
            }

            public string Title { get; }

            public string Value { get; }

            public string EmptyFallback { get; }

            public bool HideWhenEmpty { get; }

            public bool TreatAsList { get; }

            public Color BulletColor { get; }
        }

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

            public void Relayout()
            {
                if (!root.gameObject.activeSelf)
                    return;

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
        private readonly List<SectionView> sectionPool = new(6);
        private readonly List<SectionDefinition> sectionDefinitions = new(6);
        private readonly Vector3[] tabWorldCorners = new Vector3[4];

        private RectTransform activeTabUnderline;
        private Vector3 baseScale = Vector3.one;
        private RectTransform contentRoot;
        private CanvasGroup contentCanvasGroup;
        private int currentTabIndex;
        private ModuleDefinitionSO currentModule;
        private bool isCompositeMode;
        private bool isInitialized;
        private RectTransform legacyDescriptionRoot;
        private float fadeTimer;
        private RectTransform tabsBar;
        private VariantDefinitionSO currentVariant;
        private float lastKnownLayoutWidth = -1f;

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

        private void OnRectTransformDimensionsChange()
        {
            if (!isInitialized)
                return;

            EnsureTabLayout();
            RefreshTabVisuals();

            if (!isCompositeMode || contentRoot == null)
                return;

            float currentWidth = contentRoot.rect.width;
            if (Mathf.Abs(currentWidth - lastKnownLayoutWidth) >= 0.5f)
                RelayoutVisibleSections();
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
            {
                if (tabLabels[i] != null)
                    tabLabels[i].text = GetTabTitle(i);
            }

            EnsureTabLayout();
            RebuildContent();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!EnsureInitialized() || eventData.button != PointerEventData.InputButton.Left)
                return;

            for (int i = 0; i < tabLabels.Length; i++)
            {
                if (tabLabels[i] != null &&
                    RectTransformUtility.RectangleContainsScreenPoint(tabLabels[i].rectTransform, eventData.position, eventData.pressEventCamera))
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

            tabsBar = ResolveTabsBar(panelRoot);
            if (tabsBar == null)
                return false;

            activeTabUnderline = FindDeep(tabsBar, "ActiveTabUnderline") as RectTransform;
            if (activeTabUnderline == null)
                return false;

            if (!TryResolveTabLabels())
                return false;

            EnsureTabLayout();
            legacyDescriptionRoot = descriptionText == null ? null : descriptionText.rectTransform;
            isCompositeMode = TryResolveCompositeUi(scrollRect.viewport);
            RefreshTabVisuals();
            return isCompositeMode || TryResolveLegacyUi();
        }

        private RectTransform ResolveTabsBar(RectTransform panelRoot)
        {
            return panelRoot.Find("TabsBar") as RectTransform
                   ?? panelRoot.Find("Container - DescriptionCharacters") as RectTransform;
        }

        private bool TryResolveTabLabels()
        {
            for (int i = 0; i < tabLabels.Length; i++)
                tabLabels[i] = null;

            bool foundNamedTabs = true;
            for (int i = 0; i < tabLabels.Length; i++)
            {
                Transform tabTransform = tabsBar.Find("Tab_" + i);
                if (tabTransform == null)
                {
                    foundNamedTabs = false;
                    break;
                }

                if (!TryBindTabLabel(i, tabTransform))
                    return false;
            }

            if (foundNamedTabs)
                return true;

            List<TextMeshProUGUI> fallbackLabels = new(tabLabels.Length);
            for (int i = 0; i < tabsBar.childCount; i++)
            {
                Transform child = tabsBar.GetChild(i);
                if (child == activeTabUnderline)
                    continue;

                TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>() ?? child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                    fallbackLabels.Add(label);
            }

            if (fallbackLabels.Count < tabLabels.Length)
                return false;

            for (int i = 0; i < tabLabels.Length; i++)
            {
                if (!TryBindTabLabel(i, fallbackLabels[i].transform))
                    return false;
            }

            return true;
        }

        private bool TryBindTabLabel(int tabIndex, Transform tabTransform)
        {
            if (tabTransform == null)
                return false;

            TextMeshProUGUI label = tabTransform.GetComponent<TextMeshProUGUI>() ?? tabTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
                return false;

            label.text = GetTabTitle(tabIndex);
            label.raycastTarget = true;
            label.alignment = TextAlignmentOptions.Center;
            tabLabels[tabIndex] = label;

            DescriptionPanelTabClickTarget clickTarget = label.GetComponent<DescriptionPanelTabClickTarget>();
            if (clickTarget == null)
                clickTarget = label.gameObject.AddComponent<DescriptionPanelTabClickTarget>();

            clickTarget.Initialize(this, tabIndex);
            return true;
        }

        private bool TryResolveCompositeUi(RectTransform viewport)
        {
            SectionView overviewSection = ResolveSection(viewport, "Container - AboutInfo");
            SectionView architectureSection = ResolveSection(viewport, "Container - ArchitectureInfo");
            SectionView tradeOffsSection = ResolveSection(viewport, "Container - Trade-OffsInfo");
            RectTransform prosConsRoot = FindDeep(viewport, "Container - ProsCons") as RectTransform;
            if (prosConsRoot == null)
                return false;

            SectionView prosSection = ResolveSection(prosConsRoot, "Container - Pros");
            SectionView consSection = ResolveSection(prosConsRoot, "Container - Cons");

            if (overviewSection == null || architectureSection == null || tradeOffsSection == null || prosSection == null || consSection == null)
                return false;

            sectionPool.Clear();
            sectionPool.Add(overviewSection);
            sectionPool.Add(architectureSection);
            sectionPool.Add(tradeOffsSection);
            sectionPool.Add(prosSection);
            sectionPool.Add(consSection);

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

            VerticalLayoutGroup layoutGroup = GetOrAddComponent<VerticalLayoutGroup>(contentRoot.gameObject);
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = 18f;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter fitter = GetOrAddComponent<ContentSizeFitter>(contentRoot.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < sectionPool.Count; i++)
                sectionPool[i].Root.SetParent(contentRoot, false);

            if (legacyDescriptionRoot != null)
            {
                legacyDescriptionRoot.SetParent(contentRoot, false);
                legacyDescriptionRoot.gameObject.SetActive(false);
            }

            scrollRect.content = contentRoot;
            contentCanvasGroup = contentRoot.GetComponent<CanvasGroup>();
            if (contentCanvasGroup == null)
                contentCanvasGroup = contentRoot.gameObject.AddComponent<CanvasGroup>();

            lastKnownLayoutWidth = contentRoot.rect.width;

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

            sectionDefinitions.Clear();

            switch (currentTabIndex)
            {
                case 0:
                    AddRequiredTextSection(ShowcaseLocalization.GetText("about"), moduleDescription, ShowcaseLocalization.GetText("realtime_preview"));
                    AddRequiredTextSection(ShowcaseLocalization.GetText("module_type"), moduleCategory, ShowcaseLocalization.GetText("no_module_type"));
                    AddRequiredTextSection(ShowcaseLocalization.GetText("problem"), moduleProblem, ShowcaseLocalization.GetText("no_problem_statement"));
                    AddRequiredTextSection(ShowcaseLocalization.GetText("compare"), compareSummary, ShowcaseLocalization.GetText("no_compare_summary"));
                    AddOptionalTextSection(ShowcaseLocalization.GetText("takeaway"), takeaway);
                    AddOptionalTextSection(ShowcaseLocalization.GetText("webgl_preset"), webGlPreset);
                    break;

                case 1:
                    AddRequiredTextSection(ShowcaseLocalization.GetText("module_type"), moduleCategory, ShowcaseLocalization.GetText("no_module_type"));
                    AddRequiredTextSection(ShowcaseLocalization.GetText("architecture"), architectureDescription, ShowcaseLocalization.GetText("no_architecture_notes"));
                    AddRequiredTextSection(ShowcaseLocalization.GetText("problem"), moduleProblem, ShowcaseLocalization.GetText("no_problem_statement"));
                    AddOptionalListSection(ShowcaseLocalization.GetText("strengths"), pros, positiveTextColor);
                    AddOptionalTextSection(ShowcaseLocalization.GetText("takeaway"), takeaway);
                    break;

                default:
                    AddRequiredTextSection(ShowcaseLocalization.GetText("compare"), compareSummary, ShowcaseLocalization.GetText("no_compare_summary"));
                    AddRequiredTextSection(ShowcaseLocalization.GetText("trade_offs"), tradeOffs, ShowcaseLocalization.GetText("no_tradeoffs"));
                    AddOptionalListSection(ShowcaseLocalization.GetText("pros"), pros, positiveTextColor);
                    AddOptionalListSection(ShowcaseLocalization.GetText("cons"), cons, negativeTextColor);
                    AddOptionalTextSection(ShowcaseLocalization.GetText("takeaway"), takeaway);
                    break;
            }

            ApplyCompositeSections();
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
            sectionDefinitions.Clear();
            sectionDefinitions.Add(new SectionDefinition(
                ShowcaseLocalization.GetText("overview"),
                ShowcaseLocalization.GetText("no_variant"),
                ShowcaseLocalization.GetText("no_variant"),
                false));

            ApplyCompositeSections();

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
                lastKnownLayoutWidth = contentRoot.rect.width;
            }

            fadeTimer = fadeDuration;
        }

        private void RelayoutVisibleSections()
        {
            float preservedScroll = scrollRect.verticalNormalizedPosition;

            for (int i = 0; i < sectionPool.Count; i++)
                sectionPool[i].Relayout();

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            lastKnownLayoutWidth = contentRoot.rect.width;

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = preservedScroll;
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
                if (tabLabels[i] == null)
                    continue;

                tabLabels[i].color = i == currentTabIndex ? activeTabColor : inactiveTabColor;
                tabLabels[i].fontStyle = i == currentTabIndex ? FontStyles.Bold : FontStyles.Normal;
            }

            UpdateUnderlineLayout();
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

        private void AddRequiredTextSection(string title, string value, string fallback)
        {
            sectionDefinitions.Add(new SectionDefinition(title, value, fallback, false));
        }

        private void AddOptionalTextSection(string title, string value)
        {
            sectionDefinitions.Add(new SectionDefinition(title, value, string.Empty, true));
        }

        private void AddOptionalListSection(string title, string value, Color bulletColor)
        {
            sectionDefinitions.Add(new SectionDefinition(title, value, string.Empty, true, true, bulletColor));
        }

        private void ApplyCompositeSections()
        {
            EnsureSectionPoolSize(sectionDefinitions.Count);

            int visibleIndex = 0;
            for (int i = 0; i < sectionDefinitions.Count; i++)
            {
                if (!TryResolveSectionBody(sectionDefinitions[i], out string body))
                    continue;

                ApplySection(sectionPool[visibleIndex], visibleIndex, sectionDefinitions[i].Title, body);
                visibleIndex++;
            }

            for (int i = visibleIndex; i < sectionPool.Count; i++)
                sectionPool[i].SetVisible(false);
        }

        private bool TryResolveSectionBody(SectionDefinition definition, out string body)
        {
            string resolved = definition.TreatAsList
                ? FormatListBody(definition.Value, null, definition.BulletColor)
                : NormalizeBody(definition.Value);

            if (string.IsNullOrWhiteSpace(resolved))
            {
                if (definition.HideWhenEmpty)
                {
                    body = string.Empty;
                    return false;
                }

                resolved = definition.EmptyFallback;
            }

            body = resolved;
            return !string.IsNullOrWhiteSpace(body);
        }

        private void EnsureSectionPoolSize(int requiredCount)
        {
            if (requiredCount <= sectionPool.Count || sectionPool.Count == 0)
                return;

            SectionView template = sectionPool[0];
            while (sectionPool.Count < requiredCount)
            {
                GameObject clone = Instantiate(template.Root.gameObject, contentRoot, false);
                clone.name = "RuntimeSection_" + sectionPool.Count;
                SectionView cloneView = ResolveSectionFromRoot(clone.GetComponent<RectTransform>(), clone.name);
                if (cloneView == null)
                    break;

                sectionPool.Add(cloneView);
            }
        }

        private void EnsureTabLayout()
        {
            if (tabsBar == null)
                return;

            for (int i = 0; i < tabLabels.Length; i++)
            {
                TextMeshProUGUI label = tabLabels[i];
                if (label == null)
                    continue;

                RectTransform rect = label.rectTransform;
                rect.anchorMin = new Vector2(i / 3f, 0f);
                rect.anchorMax = new Vector2((i + 1) / 3f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = new Vector2(10f, 8f);
                rect.offsetMax = new Vector2(-10f, -10f);
                rect.anchoredPosition = Vector2.zero;
                label.alignment = TextAlignmentOptions.Center;
            }
        }

        private void UpdateUnderlineLayout()
        {
            if (activeTabUnderline == null || tabsBar == null || tabLabels[currentTabIndex] == null)
                return;

            RectTransform activeTab = tabLabels[currentTabIndex].rectTransform;
            activeTab.GetWorldCorners(tabWorldCorners);

            Vector3 leftLocal = tabsBar.InverseTransformPoint(tabWorldCorners[0]);
            Vector3 rightLocal = tabsBar.InverseTransformPoint(tabWorldCorners[3]);
            float underlineInset = 14f;
            float underlineWidth = Mathf.Max(32f, (rightLocal.x - leftLocal.x) - underlineInset * 2f);
            float underlineCenterX = (leftLocal.x + rightLocal.x) * 0.5f;

            activeTabUnderline.anchorMin = new Vector2(0.5f, 0f);
            activeTabUnderline.anchorMax = new Vector2(0.5f, 0f);
            activeTabUnderline.pivot = new Vector2(0.5f, 0f);
            activeTabUnderline.anchoredPosition = new Vector2(underlineCenterX, 10f);
            activeTabUnderline.sizeDelta = new Vector2(underlineWidth, activeTabUnderline.sizeDelta.y);
        }

        private static string NormalizeBody(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
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

        private static SectionView ResolveSectionFromRoot(RectTransform sectionRoot, string sectionName)
        {
            if (sectionRoot == null)
                return null;

            TextMeshProUGUI header = FindDeep(sectionRoot, "Text - Header")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI body = FindDeep(sectionRoot, "Text - Description")?.GetComponent<TextMeshProUGUI>();
            if (header == null || body == null)
                return null;

            return new SectionView(sectionName, sectionRoot, header, body);
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
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
