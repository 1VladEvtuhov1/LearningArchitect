using System.Text;
using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    public sealed class DescriptionPanel : MonoBehaviour
    {
        public TextMeshProUGUI descriptionText;
        public ScrollRect scrollRect;
        public float fadeDuration = 0.16f;
        public Color activeTabColor = default;
        public Color inactiveTabColor = default;
        public Color sectionTitleColor = default;
        public Color positiveTextColor = default;
        public Color negativeTextColor = default;

        private readonly StringBuilder builder = new StringBuilder(1024);
        private float fadeTimer;
        private Vector3 baseScale = Vector3.one;
        private RectTransform tabsBar;
        private RectTransform activeTabUnderline;
        private readonly TextMeshProUGUI[] tabLabels = new TextMeshProUGUI[3];
        private readonly Button[] tabButtons = new Button[3];
        private ModuleDefinitionSO currentModule;
        private VariantDefinitionSO currentVariant;
        private int currentTabIndex;

        private void Awake()
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

            EnsureScrollView();
            EnsureTabs();

            if (descriptionText != null)
            {
                baseScale = descriptionText.rectTransform.localScale;
                descriptionText.textWrappingMode = TextWrappingModes.Normal;
                descriptionText.overflowMode = TextOverflowModes.Overflow;
            }
        }

        private void OnEnable()
        {
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
        }

        private void Update()
        {
            if (descriptionText == null || fadeTimer <= 0f)
                return;

            fadeTimer -= Time.unscaledDeltaTime;

            float normalized = fadeDuration <= 0f ? 1f : 1f - Mathf.Clamp01(fadeTimer / fadeDuration);
            float eased = normalized * normalized * (3f - 2f * normalized);

            descriptionText.alpha = eased;
            descriptionText.rectTransform.localScale = baseScale * Mathf.Lerp(0.985f, 1f, eased);
        }

        public void SetVariant(VariantDefinitionSO variant)
        {
            SetContent(null, variant);
        }

        public void SetTab(int tabIndex)
        {
            currentTabIndex = Mathf.Clamp(tabIndex, 0, 2);
            RefreshTabVisuals();
            RebuildContent();
        }

        public void SetContent(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            currentModule = module;
            currentVariant = variant;
            RebuildContent();
        }

        private void RebuildContent()
        {
            if (descriptionText == null)
                return;

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
                    string moduleDescription = ShowcaseLocalization.GetModuleDescription(currentModule);
                    AppendSection(ShowcaseLocalization.GetText("about"), string.IsNullOrWhiteSpace(moduleDescription)
                        ? ShowcaseLocalization.GetText("realtime_preview")
                        : moduleDescription);
                    builder.Append("\n\n");
                    AppendSection(ShowcaseLocalization.GetText("key_features"), FormatList(ShowcaseLocalization.GetVariantPros(currentVariant), ShowcaseLocalization.GetText("no_key_features"), ToHex(positiveTextColor)), false);
                    builder.Append("\n\n");
                    AppendSection(ShowcaseLocalization.GetText("watch_out"), FormatList(ShowcaseLocalization.GetVariantCons(currentVariant), ShowcaseLocalization.GetText("no_constraints"), ToHex(negativeTextColor)), false);
                    break;

                case 1:
                    string architectureDescription = ShowcaseLocalization.GetVariantArchitectureDescription(currentVariant);
                    AppendSection(ShowcaseLocalization.GetText("architecture"), string.IsNullOrWhiteSpace(architectureDescription)
                        ? ShowcaseLocalization.GetText("no_architecture_notes")
                        : architectureDescription);
                    builder.Append("\n\n");
                    AppendSection(ShowcaseLocalization.GetText("strengths"), FormatList(ShowcaseLocalization.GetVariantPros(currentVariant), ShowcaseLocalization.GetText("no_strengths"), ToHex(positiveTextColor)), false);
                    break;

                default:
                    string tradeOffs = ShowcaseLocalization.GetVariantTradeOffs(currentVariant);
                    AppendSection(ShowcaseLocalization.GetText("trade_offs"), string.IsNullOrWhiteSpace(tradeOffs)
                        ? ShowcaseLocalization.GetText("no_tradeoffs")
                        : tradeOffs);
                    builder.Append("\n\n");
                    AppendSection(ShowcaseLocalization.GetText("pros"), FormatList(ShowcaseLocalization.GetVariantPros(currentVariant), ShowcaseLocalization.GetText("no_pros"), ToHex(positiveTextColor)), false, ToHex(positiveTextColor));
                    builder.Append("\n\n");
                    AppendSection(ShowcaseLocalization.GetText("cons"), FormatList(ShowcaseLocalization.GetVariantCons(currentVariant), ShowcaseLocalization.GetText("no_cons"), ToHex(negativeTextColor)), false, ToHex(negativeTextColor));
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
            if (string.IsNullOrEmpty(titleColor))
                titleColor = ToHex(sectionTitleColor);

            builder.Append("<color=").Append(titleColor).Append("><size=76%><b>")
                .Append(title).Append("</b></size></color>\n");

            if (preserveSpacing)
                builder.Append(body);
            else
                builder.Append(body.Trim());
        }

        private static string FormatList(string source, string fallback, string bulletColor)
        {
            if (string.IsNullOrWhiteSpace(source))
                return fallback;

            string[] lines = source.Split('\n');
            StringBuilder list = new StringBuilder(source.Length + lines.Length * 4);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                    continue;

                string item = line.StartsWith("-") || line.StartsWith("+") || line.StartsWith("*")
                    ? line.Substring(1).Trim()
                    : line;

                list.Append("<color=").Append(bulletColor).Append(">\u2022</color> ").Append(item);
                if (i < lines.Length - 1)
                    list.Append('\n');
            }

            return list.Length == 0 ? fallback : list.ToString();
        }

        private static string ToHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        private void EnsureScrollView()
        {
            RectTransform panelRoot = GetPanelRoot();
            if (descriptionText == null || panelRoot == null)
                return;

            if (scrollRect == null)
                scrollRect = panelRoot.GetComponent<ScrollRect>();

            if (scrollRect == null)
                scrollRect = panelRoot.gameObject.AddComponent<ScrollRect>();

            RectTransform viewportRect = panelRoot.Find("Viewport") as RectTransform;
            bool createdViewport = false;
            if (viewportRect == null)
            {
                GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
                viewportObject.transform.SetParent(panelRoot, false);
                viewportRect = viewportObject.GetComponent<RectTransform>();
                createdViewport = true;
            }

            if (createdViewport)
            {
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.offsetMin = new Vector2(22f, 18f);
                viewportRect.offsetMax = new Vector2(-22f, -18f);
            }

            Image viewportImage = viewportRect.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.color = new Color(1f, 1f, 1f, 0.001f);
                viewportImage.raycastTarget = true;
            }

            if (viewportRect.GetComponent<RectMask2D>() == null)
                viewportRect.gameObject.AddComponent<RectMask2D>();

            RectTransform textRect = descriptionText.rectTransform;
            textRect.SetParent(viewportRect, false);
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(0f, textRect.sizeDelta.y);

            ContentSizeFitter fitter = descriptionText.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = descriptionText.gameObject.AddComponent<ContentSizeFitter>();

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = textRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 24f;
        }

        private void EnsureTabs()
        {
            RectTransform panelRoot = GetPanelRoot();
            if (panelRoot == null)
                return;

            tabsBar = panelRoot.Find("TabsBar") as RectTransform;
            if (tabsBar == null)
                return;

            activeTabUnderline = tabsBar.Find("ActiveTabUnderline") as RectTransform;
            for (int i = 0; i < tabLabels.Length; i++)
            {
                Transform tabTransform = tabsBar.Find("Tab_" + i);
                if (tabTransform == null)
                    continue;

                TextMeshProUGUI label = tabTransform.GetComponent<TextMeshProUGUI>();
                if (label == null)
                    continue;

                label.text = GetTabTitle(i);
                tabLabels[i] = label;

                label.raycastTarget = true;

                Button button = tabTransform.GetComponent<Button>();
                if (button == null)
                    button = tabTransform.gameObject.AddComponent<Button>();

                button.transition = Selectable.Transition.None;
                button.targetGraphic = label;
                button.onClick.RemoveAllListeners();
                int tabIndex = i;
                button.onClick.AddListener(delegate { SetTab(tabIndex); });
                tabButtons[i] = button;
            }

            RefreshTabVisuals();
        }

        private RectTransform GetPanelRoot()
        {
            if (scrollRect != null)
                return scrollRect.transform as RectTransform;

            Transform current = descriptionText == null ? null : descriptionText.transform;
            while (current != null)
            {
                if (current.name == "DescriptionPanel")
                    return current as RectTransform;

                current = current.parent;
            }

            return null;
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

            if (activeTabUnderline == null || tabLabels[currentTabIndex] == null)
                return;

            RectTransform activeTab = tabLabels[currentTabIndex].rectTransform;
            activeTabUnderline.anchoredPosition = new Vector2(activeTab.anchoredPosition.x - 2f, activeTabUnderline.anchoredPosition.y);
            activeTabUnderline.sizeDelta = new Vector2(activeTab.sizeDelta.x + 4f, activeTabUnderline.sizeDelta.y);
        }

        private static string GetTabTitle(int index)
        {
            switch (index)
            {
                case 0:
                    return ShowcaseLocalization.GetText("overview");
                case 1:
                    return ShowcaseLocalization.GetText("architecture");
                default:
                    return ShowcaseLocalization.GetText("trade_offs");
            }
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            for (int i = 0; i < tabLabels.Length; i++)
            {
                if (tabLabels[i] != null)
                    tabLabels[i].text = GetTabTitle(i);
            }

            RebuildContent();
        }

        private void ResetScroll()
        {
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
