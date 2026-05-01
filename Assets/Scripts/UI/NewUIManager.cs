using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    [DefaultExecutionOrder(-1000)]
    public sealed class NewUIManager : MonoBehaviour
    {
        public Vector2 referenceResolution = new Vector2(1920f, 1080f);

        [Header("Palette")]
        public Color windowTint = new Color(0.042f, 0.055f, 0.058f, 0.16f);
        public Color cardColor = new Color(0.105f, 0.097f, 0.092f, 0.94f);
        public Color softCardColor = new Color(0.145f, 0.132f, 0.124f, 0.88f);
        public Color accentGlowColor = new Color(0.004f, 0.576f, 0.604f, 0.24f);
        public Color accentChipColor = new Color(0.106f, 0.435f, 0.447f, 0.74f);
        public Color primaryTextColor = new Color(0.984f, 0.950f, 0.864f, 1f);
        public Color secondaryTextColor = new Color(0.816f, 0.749f, 0.584f, 1f);
        public Color accentColor = new Color(0.004f, 0.576f, 0.604f, 1f);
        public Color accentPressedColor = new Color(0.784f, 0.188f, 0.180f, 1f);
        public Color metricsColor = new Color(0.784f, 0.612f, 0.196f, 1f);

        private const float SideMargin = 28f;
        private const float TopMargin = 18f;
        private const float BottomMargin = 16f;
        private const float HeaderHeight = 68f;
        private const float FooterHeight = 56f;
        private const float Gap = 14f;
        private const float DescriptionWidth = 300f;

        private Canvas canvas;
        private Sprite panelSprite;

        private void Awake()
        {
            canvas = GetComponentInChildren<Canvas>(true);
            if (canvas == null)
                return;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = referenceResolution;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            panelSprite = SamplePanelSprite();
            PrepareComponents();
            BuildModernUi();

            if (GetComponent<RecruiterDemoController>() == null)
                gameObject.AddComponent<RecruiterDemoController>();
        }

        private void PrepareComponents()
        {
            HubUI hubUi = GetComponent<HubUI>();
            if (hubUi != null)
            {
                hubUi.ModuleColor = primaryTextColor;
                hubUi.VariantColor = primaryTextColor;
                hubUi.HintColor = secondaryTextColor;
                hubUi.PulseColor = accentColor;
            }

            MetricsOverlay metrics = GetComponent<MetricsOverlay>();
            if (metrics != null)
            {
                metrics.HealthyColor = metricsColor;
                metrics.WarningColor = new Color(0.95f, 0.75f, 0.42f, 1f);
                metrics.CriticalColor = new Color(0.95f, 0.45f, 0.42f, 1f);
            }

            StressTestControls stress = GetComponent<StressTestControls>();
            if (stress != null)
            {
                stress.ActiveColor = accentColor;
                stress.InactiveColor = softCardColor;
                stress.TextColor = primaryTextColor;
            }

            ModuleNavigationControls navigation = GetComponent<ModuleNavigationControls>();
            if (navigation != null)
            {
                navigation.panelColor = new Color(1f, 1f, 1f, 0f);
                navigation.panelGlowTopColor = new Color(1f, 1f, 1f, 0f);
                navigation.panelGlowBottomColor = new Color(1f, 1f, 1f, 0f);
                navigation.buttonColor = softCardColor;
                navigation.buttonHoverColor = accentColor;
                navigation.buttonPressedColor = accentPressedColor;
                navigation.buttonDisabledColor = new Color(softCardColor.r, softCardColor.g, softCardColor.b, 0.40f);
                navigation.buttonGlowColor = new Color(0f, 0f, 0f, 0f);
                navigation.iconColor = primaryTextColor;
                navigation.tooltipColor = softCardColor;
                navigation.tooltipTextColor = primaryTextColor;
                navigation.panelSize = new Vector2(1f, 1f);
                navigation.buttonSize = new Vector2(44f, 44f);
                navigation.buttonSpacing = 40f;
            }

        }

        private void BuildModernUi()
        {
            RenameLegacy("HeaderBackdrop");
            RenameLegacy("HeaderPanel");
            RenameLegacy("DescriptionBackdrop");
            RenameLegacy("DescriptionPanel");
            RenameLegacy("StressControlsPanel");
            RenameLegacy("MetricsOverlay");
            RenameLegacy("NavigationControlsPanel");
            RenameLegacy("ActiveVariantAccent");
            RenameLegacy("PreviewFrame");

            ApplyWindowTint();

            RectTransform headerBackdrop = CreatePanel("HeaderBackdrop", canvas.transform, accentGlowColor, false);
            RectTransform headerPanel = CreatePanel("HeaderPanel", canvas.transform, cardColor, true);
            RectTransform previewFrame = CreatePanel("PreviewFrame", canvas.transform, new Color(0.08f, 0.09f, 0.14f, 0.82f), false);
            RectTransform descriptionBackdrop = CreatePanel("DescriptionBackdrop", canvas.transform, accentGlowColor, false);
            RectTransform descriptionPanel = CreatePanel("DescriptionPanel", canvas.transform, cardColor, true);
            RectTransform footerPanel = CreatePanel("StressControlsPanel", canvas.transform, cardColor, true);
            RectTransform navigationPanel = CreateContainer("NavigationControlsPanel", canvas.transform);
            RectTransform demoOverlay = CreateDemoOverlay(canvas.transform);

            LayoutHeader(headerBackdrop, headerPanel);
            LayoutPreview(previewFrame);
            LayoutDescription(descriptionBackdrop, descriptionPanel);
            LayoutFooter(footerPanel);
            LayoutNavigation(navigationPanel, headerPanel);
            StretchFull(demoOverlay);
            StretchTransitionOverlay();
        }

        private void LayoutHeader(RectTransform backdrop, RectTransform panel)
        {
            StretchTop(backdrop, SideMargin - 2f, SideMargin - 2f, TopMargin - 2f, HeaderHeight + 4f);
            StretchTop(panel, SideMargin, SideMargin, TopMargin, HeaderHeight);

            RectTransform moduleText = CreateText("ModuleName", panel, "<size=52%><color=#8A8F99><b>MODULE</b></color></size>\n<b>Effects System</b>", 30f, primaryTextColor, TextAlignmentOptions.Left);
            SetRect(moduleText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(58f, -2f), new Vector2(420f, 54f));

            RectTransform variantChip = CreatePanel("ActiveVariantAccent", panel, accentChipColor, false);
            SetRect(variantChip, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-54f, 0f), new Vector2(248f, 44f));

            RectTransform variantText = CreateText("VariantName", panel, "<size=50%><color=#8A8F99><b>VARIANT</b></color></size>\n<b>Indie Object Variant</b>", 22f, primaryTextColor, TextAlignmentOptions.Right);
            SetRect(variantText, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-56f, -1f), new Vector2(232f, 42f));

            RectTransform inputHints = CreateText("InputHints", panel, "Module  \u25C0 \u25B6    Variant  \u25B2 \u25BC", 13f, secondaryTextColor, TextAlignmentOptions.Center);
            SetRect(inputHints, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(280f, 16f));

            RectTransform recruiterDemo = CreateButton("RecruiterDemoButton", panel, ShowcaseLocalization.GetText("start_demo"));
            SetRect(recruiterDemo, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-332f, 0f), new Vector2(156f, 30f));

            HubUI hubUi = GetComponent<HubUI>();
            if (hubUi != null)
            {
                hubUi.ModuleName = moduleText.GetComponent<TextMeshProUGUI>();
                hubUi.VariantName = variantText.GetComponent<TextMeshProUGUI>();
                hubUi.InputHints = inputHints.GetComponent<TextMeshProUGUI>();
            }
        }

        private void LayoutPreview(RectTransform previewFrame)
        {
            previewFrame.anchorMin = Vector2.zero;
            previewFrame.anchorMax = Vector2.one;
            previewFrame.offsetMin = new Vector2(SideMargin, BottomMargin + FooterHeight + Gap);
            previewFrame.offsetMax = new Vector2(-(SideMargin + DescriptionWidth + Gap), -(TopMargin + HeaderHeight + Gap));

            RectTransform label = CreateText("PreviewLabel", previewFrame, "<b>PREVIEW</b>", 16f, secondaryTextColor, TextAlignmentOptions.Left);
            SetRect(label, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(160f, 18f));
        }

        private void LayoutDescription(RectTransform backdrop, RectTransform panel)
        {
            backdrop.anchorMin = new Vector2(1f, 0f);
            backdrop.anchorMax = new Vector2(1f, 1f);
            backdrop.pivot = new Vector2(1f, 0.5f);
            backdrop.offsetMin = new Vector2(-(DescriptionWidth + SideMargin + 2f), BottomMargin + FooterHeight + Gap - 2f);
            backdrop.offsetMax = new Vector2(-(SideMargin - 2f), -(TopMargin + HeaderHeight + Gap - 2f));

            panel.anchorMin = new Vector2(1f, 0f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 0.5f);
            panel.offsetMin = new Vector2(-(DescriptionWidth + SideMargin), BottomMargin + FooterHeight + Gap);
            panel.offsetMax = new Vector2(-SideMargin, -(TopMargin + HeaderHeight + Gap));

            RectTransform tabsBar = CreateContainer("TabsBar", panel);
            tabsBar.anchorMin = new Vector2(0f, 1f);
            tabsBar.anchorMax = new Vector2(1f, 1f);
            tabsBar.pivot = new Vector2(0.5f, 1f);
            tabsBar.offsetMin = new Vector2(14f, -46f);
            tabsBar.offsetMax = new Vector2(-14f, -14f);

            RectTransform tab0 = CreateText("Tab_0", tabsBar, ShowcaseLocalization.GetText("overview"), 15f, primaryTextColor, TextAlignmentOptions.Center);
            RectTransform tab1 = CreateText("Tab_1", tabsBar, ShowcaseLocalization.GetText("architecture"), 15f, secondaryTextColor, TextAlignmentOptions.Center);
            RectTransform tab2 = CreateText("Tab_2", tabsBar, ShowcaseLocalization.GetText("trade_offs"), 15f, secondaryTextColor, TextAlignmentOptions.Center);
            SetRect(tab0, new Vector2(0f, 0.5f), new Vector2(0.3333f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 24f));
            SetRect(tab1, new Vector2(0.3333f, 0.5f), new Vector2(0.6666f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 24f));
            SetRect(tab2, new Vector2(0.6666f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 24f));

            RectTransform underline = CreatePanel("ActiveTabUnderline", tabsBar, accentColor, false);
            underline.anchorMin = new Vector2(0f, 0f);
            underline.anchorMax = new Vector2(0.3333f, 0f);
            underline.pivot = new Vector2(0.5f, 0f);
            underline.anchoredPosition = new Vector2(0f, -2f);
            underline.sizeDelta = new Vector2(-12f, 3f);

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            viewportObject.transform.SetParent(panel, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            StretchInside(viewport, 14f, 14f, 14f, 52f);

            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.001f);
            viewportImage.raycastTarget = true;

            ScrollRect scrollRect = panel.gameObject.GetComponent<ScrollRect>();
            if (scrollRect == null)
                scrollRect = panel.gameObject.AddComponent<ScrollRect>();

            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 24f;

            DescriptionPanel description = GetComponent<DescriptionPanel>();
            if (description != null)
            {
                description.ScrollRect = scrollRect;
            }

            if (TryWireCompositeDescriptionContent(viewport, scrollRect, description))
                return;

            RectTransform descriptionText = CreateText("DescriptionText", viewport, "Loading description...", 18f, primaryTextColor, TextAlignmentOptions.TopLeft);
            descriptionText.anchorMin = new Vector2(0f, 1f);
            descriptionText.anchorMax = new Vector2(1f, 1f);
            descriptionText.pivot = new Vector2(0.5f, 1f);
            descriptionText.anchoredPosition = Vector2.zero;
            descriptionText.sizeDelta = new Vector2(0f, 0f);

            ContentSizeFitter contentFitter = descriptionText.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = descriptionText;

            if (description != null)
                description.DescriptionText = descriptionText.GetComponent<TextMeshProUGUI>();
        }

        private void LayoutFooter(RectTransform panel)
        {
            StretchBottom(panel, SideMargin, SideMargin, BottomMargin, FooterHeight);

            RectTransform moduleSection = CreateContainer("Container - ModuleSection", panel);
            RectTransform variantSection = CreateContainer("Container - VariantSection", panel);
            RectTransform stressSection = CreateContainer("Container - StressSection", panel);
            SetRect(moduleSection, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(332f, -20f));
            SetRect(variantSection, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(372f, 0f), new Vector2(332f, -20f));
            SetRect(stressSection, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(724f, 0f), new Vector2(-744f, -20f));

            RectTransform moduleTitle = CreateText("Text - ModuleHeader", moduleSection, ShowcaseLocalization.GetText("select_module"), 16f, secondaryTextColor, TextAlignmentOptions.Left);
            SetRect(moduleTitle, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -16f), new Vector2(0f, 18f));

            RectTransform moduleSelector = CreateButton("ModuleSelector", moduleSection, string.Empty);
            SetRect(moduleSelector, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 30f));
            RectTransform moduleSelectorName = CreateText("Text - ModuleValue", moduleSelector, "Effects", 18f, primaryTextColor, TextAlignmentOptions.Left);
            SetRect(moduleSelectorName, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(-20f, 22f));

            RectTransform variantTitle = CreateText("Text - VariantHeader", variantSection, ShowcaseLocalization.GetText("select_variant"), 16f, secondaryTextColor, TextAlignmentOptions.Left);
            SetRect(variantTitle, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -16f), new Vector2(0f, 18f));

            RectTransform variantSelector = CreateButton("VariantSelector", variantSection, string.Empty);
            SetRect(variantSelector, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 30f));
            RectTransform variantSelectorName = CreateText("Text - VariantValue", variantSelector, "Chunk-based", 18f, primaryTextColor, TextAlignmentOptions.Left);
            SetRect(variantSelectorName, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(-70f, 22f));

            RectTransform previousVariantButton = CreateButton("PreviousVariantButton", variantSelector, "\u2039");
            RectTransform nextVariantButton = CreateButton("NextVariantButton", variantSelector, "\u203A");
            SetRect(previousVariantButton, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-56f, 0f), new Vector2(28f, 28f));
            SetRect(nextVariantButton, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(28f, 28f));

            RectTransform stressTitle = CreateText("Text - StressHeader", stressSection, ShowcaseLocalization.GetText("stress_test"), 16f, secondaryTextColor, TextAlignmentOptions.Left);
            SetRect(stressTitle, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -16f), new Vector2(0f, 18f));

            RectTransform stressContent = CreateContainer("Container - StressContent", stressSection);
            stressContent.anchorMin = new Vector2(0f, 0f);
            stressContent.anchorMax = new Vector2(1f, 1f);
            stressContent.offsetMin = new Vector2(0f, 0f);
            stressContent.offsetMax = new Vector2(0f, -24f);

            RectTransform stressRow = CreateContainer("HorizontalLayout - StressPresets", stressContent);
            SetRect(stressRow, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(228f, 32f));

            RectTransform oneK = CreateStressPresetButton("Button - StressPreset01", stressRow, "1 000");
            RectTransform fiveK = CreateStressPresetButton("Button - StressPreset02", stressRow, "5 000");
            RectTransform tenK = CreateStressPresetButton("Button - StressPreset03", stressRow, "10 000");
            SetRect(oneK, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(54f, 28f));
            SetRect(fiveK, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(66f, 0f), new Vector2(54f, 28f));
            SetRect(tenK, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(132f, 0f), new Vector2(54f, 28f));

            RectTransform statusContainer = CreateContainer("Container - StressSummary", stressContent);
            SetRect(statusContainer, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(286f, 44f));

            RectTransform statusIcon = CreatePanel("Image - StressStatusIcon", statusContainer, accentColor, false);
            SetRect(statusIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(16f, 16f));

            RectTransform status = CreateText("Text - StressSummary", statusContainer, "<size=72%>Stress load</size>\n<size=114%><b>1K</b></size>  <size=84%>Active 1K</size>", 14f, primaryTextColor, TextAlignmentOptions.Left);
            SetRect(status, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(-26f, 0f));

            RectTransform rootFrame = CreateContainer("RootFrame", panel);
            SetRect(rootFrame, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(360f, 76f));

            RectTransform metrics = CreateText("MetricsOverlay", rootFrame, "PERFORMANCE\nFPS 60\nFrame 16.0 ms\nSIM 1K\nActive 1K", 14f, primaryTextColor, TextAlignmentOptions.TopRight);
            metrics.anchorMin = new Vector2(0f, 0.34f);
            metrics.anchorMax = new Vector2(1f, 1f);
            metrics.offsetMin = Vector2.zero;
            metrics.offsetMax = Vector2.zero;
            metrics.pivot = new Vector2(1f, 1f);

            RectTransform chartPlaceholder = CreatePanel("ChartPlaceholder", rootFrame, new Color(1f, 1f, 1f, 0.02f), false);
            chartPlaceholder.anchorMin = new Vector2(0f, 0f);
            chartPlaceholder.anchorMax = new Vector2(1f, 0.30f);
            chartPlaceholder.offsetMin = Vector2.zero;
            chartPlaceholder.offsetMax = Vector2.zero;

            GameObject performanceGraphObject = new GameObject("PerformanceGraph", typeof(RectTransform), typeof(CanvasRenderer), typeof(PerformanceGraph));
            performanceGraphObject.transform.SetParent(chartPlaceholder, false);
            RectTransform performanceGraphRect = performanceGraphObject.GetComponent<RectTransform>();
            StretchFull(performanceGraphRect);

            StressTestControls stress = GetComponent<StressTestControls>();
            if (stress != null)
            {
                stress.OneKButton = oneK.GetComponent<Button>();
                stress.FiveKButton = fiveK.GetComponent<Button>();
                stress.TenKButton = tenK.GetComponent<Button>();
                stress.StatusText = status.GetComponent<TextMeshProUGUI>();
            }

            MetricsOverlay overlay = GetComponent<MetricsOverlay>();
            if (overlay != null)
                overlay.FpsText = metrics.GetComponent<TextMeshProUGUI>();
        }

        private void LayoutNavigation(RectTransform navigationPanel, RectTransform headerPanel)
        {
            StretchTop(navigationPanel, SideMargin, SideMargin, TopMargin, HeaderHeight);
            SetTransparent(navigationPanel);

            RectTransform previousModule = CreateNavButton("PreviousModuleButton", navigationPanel, "\u25C0");
            RectTransform nextModule = CreateNavButton("NextModuleButton", navigationPanel, "\u25B6");
            RectTransform previousVariant = CreateNavButton("PreviousVariantButton", navigationPanel, "\u25B2");
            RectTransform nextVariant = CreateNavButton("NextVariantButton", navigationPanel, "\u25BC");

            SetRect(previousModule, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14f, 0f), new Vector2(42f, 42f));
            SetRect(nextModule, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(354f, 0f), new Vector2(42f, 42f));
            SetRect(previousVariant, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-12f, -10f), new Vector2(34f, 34f));
            SetRect(nextVariant, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-12f, 10f), new Vector2(34f, 34f));
        }

        private void StretchTransitionOverlay()
        {
            RectTransform overlay = FindRect(canvas.transform, "TransitionOverlay");
            if (overlay == null)
                return;

            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;
        }

        private void ApplyWindowTint()
        {
            RectTransform tint = FindRect(canvas.transform, "ShowcaseBackgroundTint");
            if (tint == null)
                return;

            StretchFull(tint);

            RawImage rawImage = tint.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = null;
                rawImage.color = windowTint;
                rawImage.raycastTarget = false;
                tint.gameObject.SetActive(true);
            }
        }

        private void RenameLegacy(string objectName)
        {
            Transform legacy = canvas.transform.Find(objectName);
            if (legacy == null)
                return;

            legacy.name = "Legacy_" + objectName;
            legacy.gameObject.SetActive(false);
        }

        private RectTransform CreatePanel(string name, Transform parent, Color color, bool shadow)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            Image image = go.GetComponent<Image>();
            image.sprite = panelSprite;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;

            if (shadow)
            {
                Shadow s = go.AddComponent<Shadow>();
                s.effectColor = new Color(0f, 0f, 0f, 0.18f);
                s.effectDistance = new Vector2(0f, -5f);
                s.useGraphicAlpha = true;
            }

            return rect;
        }

        private RectTransform CreateContainer(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            SetTransparent(rect);
            return rect;
        }

        private RectTransform CreateDemoOverlay(Transform parent)
        {
            GameObject root = new GameObject("DemoOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);

            RectTransform rootRect = root.GetComponent<RectTransform>();
            Image rootImage = root.GetComponent<Image>();
            rootImage.color = new Color(0.03f, 0.05f, 0.08f, 0.82f);
            rootImage.raycastTarget = true;

            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            RectTransform card = CreatePanel("DemoOverlayCard", root.transform, cardColor, true);
            SetRect(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(620f, 240f));

            RectTransform chip = CreateText("DemoOverlayChip", card, ShowcaseLocalization.GetText("guided_demo"), 16f, secondaryTextColor, TextAlignmentOptions.Center);
            SetRect(chip, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(260f, 20f));

            RectTransform title = CreateText("DemoOverlayTitle", card, "Recruiter Demo", 34f, primaryTextColor, TextAlignmentOptions.Center);
            SetRect(title, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(520f, 48f));

            RectTransform body = CreateText("DemoOverlayBody", card, "Guided walkthrough of the current showcase modules.", 18f, secondaryTextColor, TextAlignmentOptions.Center);
            SetRect(body, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -36f), new Vector2(520f, 72f));

            return rootRect;
        }

        private RectTransform CreateText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.Normal;

            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;

            return rect;
        }

        private RectTransform CreateButton(string name, Transform parent, string label)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.sprite = panelSprite;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = softCardColor;

            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = softCardColor;
            colors.highlightedColor = accentColor;
            colors.pressedColor = accentPressedColor;
            colors.selectedColor = accentColor;
            colors.disabledColor = new Color(softCardColor.r, softCardColor.g, softCardColor.b, 0.42f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            RectTransform labelRect = CreateText("Text", go.transform, "<b>" + label + "</b>", 13f, primaryTextColor, TextAlignmentOptions.Center);
            StretchFull(labelRect);
            return go.GetComponent<RectTransform>();
        }

        private RectTransform CreateStressPresetButton(string name, Transform parent, string label)
        {
            RectTransform root = CreateButton(name, parent, string.Empty);

            Transform legacyText = root.Find("Text");
            RectTransform valueRect;
            if (legacyText != null)
            {
                legacyText.name = "Text - PresetValue";
                valueRect = legacyText as RectTransform;
                TextMeshProUGUI valueText = legacyText.GetComponent<TextMeshProUGUI>();
                if (valueText != null)
                {
                    valueText.text = "<b>" + label + "</b>";
                    valueText.color = primaryTextColor;
                    valueText.alignment = TextAlignmentOptions.Center;
                }
            }
            else
            {
                valueRect = CreateText("Text - PresetValue", root, "<b>" + label + "</b>", 13f, primaryTextColor, TextAlignmentOptions.Center);
            }

            StretchFull(valueRect);
            return root;
        }

        private RectTransform CreateNavButton(string name, Transform parent, string glyph)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.sprite = panelSprite;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = softCardColor;

            RectTransform labelRect = CreateText("Label", go.transform, glyph, 18f, primaryTextColor, TextAlignmentOptions.Center);
            StretchFull(labelRect);
            return go.GetComponent<RectTransform>();
        }

        private void SetTransparent(RectTransform rect)
        {
            if (rect == null)
                return;

            Image image = rect.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = new Color(1f, 1f, 1f, 0f);
                image.raycastTarget = false;
            }
        }

        private void StretchFull(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private void StretchInside(RectTransform rect, float left, float right, float bottom, float top)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private void StretchTop(RectTransform rect, float left, float right, float top, float height)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -(top + height));
            rect.offsetMax = new Vector2(-right, -top);
        }

        private void StretchBottom(RectTransform rect, float left, float right, float bottom, float height)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, bottom + height);
        }

        private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            if (rect == null)
                return;

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private Sprite SamplePanelSprite()
        {
            string[] candidates = { "HeaderPanel", "DescriptionPanel", "StressControlsPanel", "HeaderBackdrop" };
            for (int i = 0; i < candidates.Length; i++)
            {
                Transform target = canvas.transform.Find(candidates[i]);
                if (target == null)
                    continue;

                Image image = target.GetComponent<Image>();
                if (image != null && image.sprite != null)
                    return image.sprite;
            }

            return null;
        }

        private static RectTransform FindRect(Transform root, string name)
        {
            if (root == null)
                return null;

            if (root.name == name)
                return root as RectTransform;

            for (int i = 0; i < root.childCount; i++)
            {
                RectTransform nested = FindRect(root.GetChild(i), name);
                if (nested != null)
                    return nested;
            }

            return null;
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

        private static bool TryWireCompositeDescriptionContent(RectTransform viewport, ScrollRect scrollRect, DescriptionPanel description)
        {
            if (viewport == null || scrollRect == null)
                return false;

            if (FindDeep(viewport, "Container - AboutInfo") == null ||
                FindDeep(viewport, "Container - ArchitectureInfo") == null ||
                FindDeep(viewport, "Container - Trade-OffsInfo") == null)
            {
                return false;
            }

            RectTransform contentRoot = viewport.Find("ContentRoot") as RectTransform;
            if (contentRoot == null)
            {
                GameObject contentObject = new GameObject("ContentRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                contentRoot = contentObject.GetComponent<RectTransform>();
                contentRoot.SetParent(viewport, false);
            }

            contentRoot.anchorMin = new Vector2(0f, 1f);
            contentRoot.anchorMax = new Vector2(1f, 1f);
            contentRoot.pivot = new Vector2(0.5f, 1f);
            contentRoot.anchoredPosition = Vector2.zero;
            contentRoot.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layoutGroup = GetOrAdd<VerticalLayoutGroup>(contentRoot.gameObject);
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = 18f;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter fitter = GetOrAdd<ContentSizeFitter>(contentRoot.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            string[] sectionNames =
            {
                "Container - AboutInfo",
                "Container - ArchitectureInfo",
                "Container - Trade-OffsInfo",
                "Container - ProsCons"
            };

            for (int i = 0; i < sectionNames.Length; i++)
            {
                RectTransform section = FindDeep(viewport, sectionNames[i]) as RectTransform;
                if (section != null)
                    section.SetParent(contentRoot, false);
            }

            scrollRect.content = contentRoot;

            if (description != null)
                description.DescriptionText = null;

            return true;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
