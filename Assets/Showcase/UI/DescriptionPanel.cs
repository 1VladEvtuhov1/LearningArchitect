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

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Color activeTabColor = default;
        [SerializeField] private Color inactiveTabColor = default;
        [SerializeField] private Color sectionTitleColor = default;
        [SerializeField] private Color positiveTextColor = default;
        [SerializeField] private Color negativeTextColor = default;

        private readonly TextMeshProUGUI[] tabLabels = new TextMeshProUGUI[3];
        private readonly List<SectionDefinition> sectionDefinitions = new(6);
        private readonly Vector3[] tabWorldCorners = new Vector3[4];

        private RectTransform activeTabUnderline;
        private int currentTabIndex;
        private ModuleDefinitionSO currentModule;
        private bool isInitialized;
        private TextMeshProUGUI legacyDescriptionText;
        private RectTransform tabsBar;
        private VariantDefinitionSO currentVariant;

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
            RebuildContent();
            RefreshTabVisuals();
        }

        public void SetContent(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (!EnsureInitialized())
                return;

            currentModule = module;
            currentVariant = variant;
            RebuildContent();
            RefreshTabVisuals();
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
            RefreshTabVisuals();
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
            if (scrollRect == null || scrollRect.viewport == null)
                return false;

            RectTransform panelRoot = scrollRect.transform as RectTransform;
            if (panelRoot == null)
                return false;

            tabsBar = panelRoot.Find("Container - DescriptionCharacters") as RectTransform;
            if (tabsBar == null)
                return false;

            activeTabUnderline = FindDeep(tabsBar, "ActiveTabUnderline") as RectTransform;
            if (activeTabUnderline == null)
                return false;

            legacyDescriptionText = FindDeep(scrollRect.viewport, "DescriptionText")?.GetComponent<TextMeshProUGUI>();
            if (legacyDescriptionText == null)
                return false;

            legacyDescriptionText.richText = true;

            if (!TryResolveTabLabels())
                return false;

            EnsureTabLayout();
            RefreshTabVisuals();
            return true;
        }

        private bool TryResolveTabLabels()
        {
            for (int i = 0; i < tabLabels.Length; i++)
                tabLabels[i] = null;

            List<TextMeshProUGUI> labels = new(tabLabels.Length);
            for (int i = 0; i < tabsBar.childCount; i++)
            {
                Transform child = tabsBar.GetChild(i);
                if (child == activeTabUnderline)
                    continue;

                TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>() ?? child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                    labels.Add(label);
            }

            if (labels.Count < tabLabels.Length)
                return false;

            for (int i = 0; i < tabLabels.Length; i++)
            {
                if (!TryBindTabLabel(i, labels[i].transform))
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

        private void RebuildContent()
        {
            if (legacyDescriptionText == null)
                return;

            if (currentVariant == null)
            {
                legacyDescriptionText.text = BuildLegacySection(
                    ShowcaseLocalization.GetText("overview"),
                    ShowcaseLocalization.GetText("no_variant"),
                    sectionTitleColor);
                ResetScroll();
                return;
            }

            string moduleDescription = ShowcaseLocalization.GetModuleDescription(currentModule);
            string moduleCategory = ShowcaseLocalization.GetModuleCategory(currentModule);
            string moduleProblem = ShowcaseLocalization.GetModuleProblemStatement(currentModule);
            string compareSummary = ShowcaseLocalization.GetVariantCompareSummary(currentVariant);
            string takeaway = ShowcaseLocalization.GetVariantTakeaway(currentVariant);
            string webGlPreset = ShowcaseLocalization.GetModuleWebGlPresetNote(currentModule);
            string architectureDescription = ShowcaseLocalization.GetVariantArchitectureDescription(currentVariant);
            string dataFlow = ShowcaseLocalization.GetVariantDataFlow(currentVariant);
            string runtimeLifecycle = ShowcaseLocalization.GetVariantRuntimeLifecycle(currentVariant);
            string whyThisApproach = ShowcaseLocalization.GetVariantWhyThisApproach(currentVariant);
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
                    AddRequiredTextSection(ShowcaseLocalization.GetText("problem"), moduleProblem, ShowcaseLocalization.GetText("no_problem_statement"));
                    AddRequiredTextSection(ShowcaseLocalization.GetText("core_idea"), architectureDescription, ShowcaseLocalization.GetText("no_architecture_notes"));
                    AddOptionalTextSection(ShowcaseLocalization.GetText("data_flow"), dataFlow);
                    AddOptionalTextSection(ShowcaseLocalization.GetText("runtime_lifecycle"), runtimeLifecycle);
                    AddOptionalTextSection(ShowcaseLocalization.GetText("why_this_approach"), string.IsNullOrWhiteSpace(whyThisApproach) ? compareSummary : whyThisApproach);
                    AddOptionalListSection(ShowcaseLocalization.GetText("strengths"), pros, positiveTextColor);
                    AddRequiredListSection(ShowcaseLocalization.GetText("watch_out"), cons, ShowcaseLocalization.GetText("no_constraints"), negativeTextColor);
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

            legacyDescriptionText.text = BuildLegacyBody();
            LayoutRebuilder.ForceRebuildLayoutImmediate(legacyDescriptionText.rectTransform);
            ResetScroll();
        }

        private string BuildLegacyBody()
        {
            StringBuilder builder = new();

            for (int i = 0; i < sectionDefinitions.Count; i++)
            {
                if (!TryResolveSectionBody(sectionDefinitions[i], out string body))
                    continue;

                if (builder.Length > 0)
                    builder.Append("\n\n");

                builder.Append(BuildLegacySection(
                    sectionDefinitions[i].Title,
                    body,
                    ResolveSectionTitleColor(sectionDefinitions[i].Title)));
            }

            return builder.ToString();
        }

        private static string BuildLegacySection(string title, string body, Color titleColor)
        {
            return "<color=" + ToHex(titleColor) + "><b>" + title + "</b></color>\n" + body;
        }

        private void ResetScroll()
        {
            if (scrollRect == null)
                return;

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private Color ResolveSectionTitleColor(string title)
        {
            string prosTitle = ShowcaseLocalization.GetText("pros");
            string consTitle = ShowcaseLocalization.GetText("cons");
            string strengthsTitle = ShowcaseLocalization.GetText("strengths");
            string watchOutTitle = ShowcaseLocalization.GetText("watch_out");

            if (string.Equals(title, prosTitle, StringComparison.Ordinal) ||
                string.Equals(title, strengthsTitle, StringComparison.Ordinal))
            {
                return positiveTextColor;
            }

            if (string.Equals(title, consTitle, StringComparison.Ordinal) ||
                string.Equals(title, watchOutTitle, StringComparison.Ordinal))
            {
                return negativeTextColor;
            }

            return sectionTitleColor;
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

        private void AddRequiredListSection(string title, string value, string fallback, Color bulletColor)
        {
            sectionDefinitions.Add(new SectionDefinition(title, value, fallback, false, true, bulletColor));
        }

        private bool TryResolveSectionBody(SectionDefinition definition, out string body)
        {
            string resolved = definition.TreatAsList
                ? FormatListBody(definition.Value, definition.EmptyFallback, definition.BulletColor)
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
