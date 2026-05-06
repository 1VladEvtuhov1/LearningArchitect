using LearningArchitect.Core;
using LearningArchitect.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Editor
{
    public static class ShowcaseLayoutTool
    {
        private const string PrefabPath = "Assets/Showcase/Prefabs/ArchitectureShowcaseHub.prefab";
        private const string SafeModeMessage = "Showcase layout rebuild is disabled in safe mode. Use the PreReorganize visual baseline and adjust wiring manually until the tool is rewritten.";

        [MenuItem("Tools/LearningArchitect/Rebuild Showcase Layout")]
        public static void RebuildMenu()
        {
            Debug.LogWarning(SafeModeMessage);
        }

        [MenuItem("Tools/LearningArchitect/Apply Showcase Layout To Scene")]
        public static void ApplyToSceneMenu()
        {
            Debug.LogWarning(SafeModeMessage);
        }

        public static string Rebuild()
        {
            Debug.LogWarning(SafeModeMessage);
            return SafeModeMessage;
        }

        public static string ApplyToScene()
        {
            Debug.LogWarning(SafeModeMessage);
            return SafeModeMessage;
        }

        private static void ApplyLayout(GameObject prefabRoot)
        {
            CleanupLegacyCanvases(prefabRoot.transform);

            Canvas canvas = FindCanvasByChild(prefabRoot.transform, "RootFrame");
            if (canvas == null)
                return;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Sprite borderSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Frames/Border_r16.png");
            Sprite frame16Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Frames/Frame_r16.png");
            Sprite frame8WhiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Frames/Frame_r8_white.png");
            Sprite frame8YellowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Frames/Frame_r8_yellow.png");
            Sprite effectIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Icons/EffectSystem_icon.png");
            Sprite moduleIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Icons/Module_icon.png");
            Sprite variantIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Icons/Variant_icon.png");
            Sprite loadedIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Icons/Loaded_icon.png");
            Sprite orbitIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Icons/Orbit_icon.png");
            Sprite zoomIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Icons/Zoom_icon.png");
            Sprite perfIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Icons/Performance_icon.png");
            Sprite stressIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Icons/Stress_icon.png");
            Sprite lineSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Lines/Line.png");
            Sprite lineGreenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Shared/UI/Lines/LineGreen.png");

            RectTransform tint = EnsureRect(canvas.transform, canvas.transform, "ShowcaseBackgroundTint");
            RectTransform rootFrame = EnsureRect(canvas.transform, canvas.transform, "RootFrame");
            RectTransform headerLine = EnsureRect(rootFrame, rootFrame, "HeaderLine");
            RectTransform breadcrumb = EnsureRect(headerLine, headerLine, "Breadcrumb");
            RectTransform headerPanel = EnsureRect(rootFrame, rootFrame, "ModuleInfoPanel", "HeaderPanel");
            RectTransform moduleIcon = EnsureRect(headerPanel, headerPanel, "Image - ModuleIcon", "ModuleIcon");
            RectTransform moduleName = EnsureRect(headerPanel, headerPanel, "Text - ModuleName", "ModuleName");
            RectTransform variantName = EnsureRect(headerPanel, headerPanel, "Text - VariantName", "VariantName");
            RectTransform inputHints = EnsureRect(headerPanel, headerPanel, "Text - InputHints", "InputHints");
            RectTransform infoDivider = EnsureRect(headerPanel, headerPanel, "InfoDivider");
            RectTransform performanceCard = EnsureRect(rootFrame, rootFrame, "PerformanceCard");
            RectTransform previewFrame = EnsureRect(rootFrame, rootFrame, "PreviewFrame");
            RectTransform previewLabel = EnsureRect(previewFrame, previewFrame, "PreviewLabel");
            RectTransform descriptionPanel = EnsureRect(rootFrame, rootFrame, "DescriptionPanel");
            RectTransform tabsBar = EnsureRect(descriptionPanel, descriptionPanel, "Container - DescriptionCharacters", "TabsBar");
            RectTransform activeTabUnderline = EnsureRect(tabsBar, tabsBar, "ActiveTabUnderline");
            RectTransform bodyDivider = EnsureRect(descriptionPanel, descriptionPanel, "BodyDivider");
            RectTransform viewport = EnsureRect(descriptionPanel, descriptionPanel, "Viewport");
            RectTransform stressPanel = EnsureRect(rootFrame, rootFrame, "StressControlsPanel");
            RectTransform systemStatusCard = EnsureRect(rootFrame, rootFrame, "SystemStatusCard");
            RectTransform navPanel = EnsureRect(headerPanel, headerPanel, "NavigationControlsPanel");
            RectTransform transitionOverlay = EnsureRect(canvas.transform, canvas.transform, "TransitionOverlay");

            StretchFull(tint);
            ConfigureImage(GetOrAdd<Image>(tint.gameObject), null, ShowcasePalette.WithAlpha(ShowcasePalette.BgDeep, 0.32f), Image.Type.Simple, false);

            StretchInside(rootFrame, 18f, 18f, 18f, 18f);
            ConfigureImage(GetOrAdd<Image>(rootFrame.gameObject), borderSprite, new Color(1f, 1f, 1f, 0f), Image.Type.Sliced, false);
            Outline rootOutline = GetOrAdd<Outline>(rootFrame.gameObject);
            rootOutline.effectColor = ShowcasePalette.BorderAccent(0.08f);
            rootOutline.effectDistance = new Vector2(1f, -1f);

            StretchTop(headerLine, 16f, 16f, 12f, 28f);
            ConfigureImage(GetOrAdd<Image>(headerLine.gameObject), null, ShowcasePalette.Divider(0.045f), Image.Type.Simple, false);
            ConfigureAccent(headerLine, "LeftAccent", new Vector2(12f, 0f), new Vector2(2f, 18f), 0.16f);
            ConfigureAccent(headerLine, "RightAccent", new Vector2(-12f, 0f), new Vector2(2f, 18f), 0.10f, true);
            ConfigureLanguageToggle(canvas, headerLine, frame8WhiteSprite, frame8YellowSprite);

            SetRect(breadcrumb, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(420f, 20f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(breadcrumb.gameObject), "ENGINE SYSTEMS / <color=#" + ShowcasePalette.AccentHex + ">VISUALIZER</color>", 18f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

            SetRect(headerPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -48f), new Vector2(622f, 214f));
            ConfigureCard(headerPanel, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.94f), 0.035f, 0.28f);

            SetRect(moduleIcon, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -26f), new Vector2(80f, 80f));
            ConfigureImage(GetOrAdd<Image>(moduleIcon.gameObject), effectIconSprite, Color.white, Image.Type.Simple, false);
            Outline moduleIconOutline = GetOrAdd<Outline>(moduleIcon.gameObject);
            moduleIconOutline.effectColor = ShowcasePalette.AccentSoft(0.10f);
            moduleIconOutline.effectDistance = new Vector2(0f, -1f);
            RectTransform iconGlyph = Rect(headerPanel, "IconGlyph");
            if (iconGlyph != null)
                iconGlyph.gameObject.SetActive(false);

            SetRect(moduleName, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -18f), new Vector2(320f, 42f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(moduleName.gameObject), "Effects System", 28f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);
            SetRect(variantName, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -58f), new Vector2(360f, 28f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(variantName.gameObject), "<b>Chunk-based Variant</b>", 18f, ShowcasePalette.AccentMain, TextAlignmentOptions.Left);
            SetRect(inputHints, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -88f), new Vector2(240f, 18f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(inputHints.gameObject), "Realtime architecture preview", 12f, ShowcasePalette.TextMuted, TextAlignmentOptions.Left);

            SetRect(infoDivider, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(-44f, 1f));
            ConfigureImage(GetOrAdd<Image>(infoDivider.gameObject), lineSprite, ShowcasePalette.BorderMain(0.075f), Image.Type.Sliced, false);

            string[] statLabels = { "MODULE", "VARIANT", "LOADED", "ORBIT", "ZOOM" };
            string[] statValues = { "Effects", "Chunk-based", "12 / 25", "RMB", "Wheel" };
            Sprite[] statIcons = { moduleIconSprite, variantIconSprite, loadedIconSprite, orbitIconSprite, zoomIconSprite };
            ConfigureModuleStats(headerPanel, statLabels, statValues, statIcons);

            SetRect(performanceCard, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18f, 196f), new Vector2(398f, 334f));
            ConfigureCard(performanceCard, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.94f), 0.030f, 0.28f);
            ConfigureStructuredPerformanceCard(performanceCard, perfIconSprite, lineSprite);

            StretchInside(previewFrame, 286f, 514f, 138f, 74f);
            ConfigureImage(GetOrAdd<Image>(previewFrame.gameObject), null, new Color(1f, 1f, 1f, 0f), Image.Type.Simple, false);
            previewLabel.gameObject.SetActive(false);

            descriptionPanel.anchorMin = new Vector2(1f, 0f);
            descriptionPanel.anchorMax = new Vector2(1f, 1f);
            descriptionPanel.pivot = new Vector2(1f, 0.5f);
            descriptionPanel.offsetMin = new Vector2(-516f, 138f);
            descriptionPanel.offsetMax = new Vector2(-18f, -48f);
            ConfigureCard(descriptionPanel, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.95f), 0.030f, 0.30f);

            tabsBar.name = "Container - DescriptionCharacters";
            StretchTop(tabsBar, 12f, 12f, 12f, 76f);
            ConfigureImage(GetOrAdd<Image>(tabsBar.gameObject), frame8WhiteSprite, new Color(0.043f, 0.053f, 0.067f, 0.94f), Image.Type.Sliced, false);
            CleanupTabsBarChildren(tabsBar);
            string[] tabs = { "Overview", "Architecture", "Trade-offs" };
            for (int i = 0; i < 3; i++)
            {
                RectTransform tabRect = EnsureRect(tabsBar, tabsBar, "Tab_" + i);
                SetRect(tabRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f + 138f * i, -2f), new Vector2(126f, 28f));
                ConfigureText(GetOrAdd<TextMeshProUGUI>(tabRect.gameObject), tabs[i], 16f, i == 0 ? ShowcasePalette.AccentMain : ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);
            }

            SetRect(activeTabUnderline, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(22f, 10f), new Vector2(130f, 3f));
            ConfigureImage(GetOrAdd<Image>(activeTabUnderline.gameObject), null, ShowcasePalette.AccentMain, Image.Type.Simple, false);
            AddAccentGlow(activeTabUnderline, "UnderlineGlow", ShowcasePalette.AccentSoft(0.28f), new Vector2(142f, 10f));
            SetRect(bodyDivider, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(-36f, 1f));
            ConfigureImage(GetOrAdd<Image>(bodyDivider.gameObject), lineSprite, ShowcasePalette.BorderMain(0.070f), Image.Type.Sliced, false);

            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.pivot = new Vector2(0.5f, 0.5f);
            viewport.offsetMin = new Vector2(22f, 22f);
            viewport.offsetMax = new Vector2(-22f, -94f);
            ConfigureImage(GetOrAdd<Image>(viewport.gameObject), null, new Color(1f, 1f, 1f, 0.001f), Image.Type.Simple, true);
            GetOrAdd<RectMask2D>(viewport.gameObject);

            stressPanel.anchorMin = new Vector2(0f, 0f);
            stressPanel.anchorMax = new Vector2(1f, 0f);
            stressPanel.pivot = new Vector2(0.5f, 0f);
            stressPanel.offsetMin = new Vector2(18f, 18f);
            stressPanel.offsetMax = new Vector2(-540f, 168f);
            ConfigureCard(stressPanel, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.95f), 0.030f, 0.28f);

            ConfigureFooter(canvas, moduleIconSprite, variantIconSprite, stressIconSprite, frame8WhiteSprite, frame8YellowSprite);
            ConfigureStatusCard(canvas, systemStatusCard, frame16Sprite, frame8WhiteSprite, lineGreenSprite);
            ConfigureNavButtons(canvas, navPanel, frame8WhiteSprite, frame8YellowSprite);

            StretchFull(transitionOverlay);
            ConfigureImage(GetOrAdd<Image>(transitionOverlay.gameObject), null, new Color(0.004f, 0.008f, 0.012f, 0.72f), Image.Type.Simple, false);

            ShowcaseRuntimeController runtimeController = GetOrAdd<ShowcaseRuntimeController>(prefabRoot);
            ShowcaseCompositionRoot compositionRoot = GetOrAdd<ShowcaseCompositionRoot>(prefabRoot);
            ShowcaseCommandRouter commandRouter = GetOrAdd<ShowcaseCommandRouter>(prefabRoot);
            ShowcaseStateHub stateHub = GetOrAdd<ShowcaseStateHub>(prefabRoot);
            HubUI hubUI = GetOrAdd<HubUI>(prefabRoot);
            DescriptionPanel description = GetOrAdd<DescriptionPanel>(prefabRoot);
            MetricsOverlay metrics = GetOrAdd<MetricsOverlay>(prefabRoot);
            StressTestControls stress = GetOrAdd<StressTestControls>(prefabRoot);
            ModuleNavigationControls navigation = GetOrAdd<ModuleNavigationControls>(prefabRoot);
            ShowcaseTransitionController transitionController = GetOrAdd<ShowcaseTransitionController>(prefabRoot);
            GetOrAdd<ShowcaseLocalization>(prefabRoot);

            hubUI.ModuleName = GetOrAdd<TextMeshProUGUI>(moduleName.gameObject);
            hubUI.VariantName = GetOrAdd<TextMeshProUGUI>(variantName.gameObject);
            hubUI.InputHints = GetOrAdd<TextMeshProUGUI>(inputHints.gameObject);
            hubUI.ModuleSelectorName = GetOrAdd<TextMeshProUGUI>(EnsureRect(stressPanel, stressPanel, "Text - ModuleValue").gameObject);
            hubUI.VariantSelectorName = GetOrAdd<TextMeshProUGUI>(EnsureRect(stressPanel, stressPanel, "Text - VariantValue").gameObject);
            hubUI.ModuleColor = ShowcasePalette.TextPrimary;
            hubUI.VariantColor = ShowcasePalette.AccentMain;
            hubUI.HintColor = ShowcasePalette.TextMuted;
            hubUI.PulseColor = ShowcasePalette.AccentStrong;

            ScrollRect scrollRect = GetOrAdd<ScrollRect>(descriptionPanel.gameObject);
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;
            description.ScrollRect = scrollRect;
            description.SectionTitleColor = ShowcasePalette.AccentMain;
            description.PositiveTextColor = ShowcasePalette.Success;
            description.NegativeTextColor = ShowcasePalette.Error;

            ResetDescriptionViewport(viewport);
            EnsureCompositeDescriptionSections(viewport);

            if (!TryWireCompositeDescriptionContent(viewport, scrollRect))
                throw new System.InvalidOperationException("DescriptionPanel requires composite section content under Viewport.");

            metrics.FpsText = null;
            metrics.TitleColor = ShowcasePalette.AccentMain;
            metrics.LabelColor = ShowcasePalette.TextSecondary;
            metrics.ValueColor = ShowcasePalette.TextPrimary;
            metrics.GraphBackgroundColor = ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.055f);
            stress.OneKButton = GetOrAdd<Button>(EnsureRect(stressPanel, stressPanel, "Button - StressPreset01").gameObject);
            stress.FiveKButton = GetOrAdd<Button>(EnsureRect(stressPanel, stressPanel, "Button - StressPreset02").gameObject);
            stress.TenKButton = GetOrAdd<Button>(EnsureRect(stressPanel, stressPanel, "Button - StressPreset03").gameObject);
            stress.StatusText = GetOrAdd<TextMeshProUGUI>(EnsureRect(stressPanel, stressPanel, "Text - StressSummary").gameObject);
            stress.InactiveLabelColor = ShowcasePalette.TextSecondary;
            stress.HoverBackgroundColor = new Color(0.070f, 0.083f, 0.102f, 0.96f);
            stress.PressedBackgroundColor = new Color(0.145f, 0.083f, 0.045f, 0.98f);
            navigation.panelSprite = frame8WhiteSprite;
            navigation.buttonSprite = frame8WhiteSprite;
            transitionController.transitionOverlay = GetOrAdd<CanvasGroup>(transitionOverlay.gameObject);
            transitionController.transitionDuration = 0.16f;
            compositionRoot.RuntimeController = runtimeController;
            compositionRoot.CommandRouter = commandRouter;
            compositionRoot.TransitionController = transitionController;
            compositionRoot.StateHub = stateHub;
            compositionRoot.ModuleRoot = EnsureTransformChild(prefabRoot.transform, "ModuleRoot");
            compositionRoot.Modules = ResolveModules(compositionRoot.Modules);
            commandRouter.RuntimeController = runtimeController;
        }

        private static void ConfigureLanguageToggle(Canvas canvas, RectTransform headerLine, Sprite frame8WhiteSprite, Sprite frame8YellowSprite)
        {
            GameObject buttonObject = EnsureChild(headerLine, "LanguageToggleButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            SetRect(buttonRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(44f, 24f));
            Color languageButtonColor = new Color(0.055f, 0.067f, 0.082f, 0.96f);
            ConfigureImage(buttonObject.GetComponent<Image>(), frame8WhiteSprite, languageButtonColor, Image.Type.Sliced, true);

            Button button = GetOrAdd<Button>(buttonObject);
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = buttonObject.GetComponent<Image>();
            ColorBlock colors = button.colors;
            colors.normalColor = languageButtonColor;
            colors.highlightedColor = ShowcasePalette.PanelHover;
            colors.pressedColor = ShowcasePalette.AccentStrong;
            colors.selectedColor = languageButtonColor;
            colors.disabledColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelSoft, 0.42f);
            button.colors = colors;

            GameObject labelObject = EnsureChild(buttonRect, "Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            StretchFull(labelRect);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            ConfigureText(label, "RU", 13f, ShowcasePalette.AccentMain, TextAlignmentOptions.Center);
        }

        private static void ConfigureFooter(Canvas canvas, Sprite moduleIconSprite, Sprite variantIconSprite, Sprite stressIconSprite, Sprite frame8WhiteSprite, Sprite frame8YellowSprite)
        {
            RectTransform stressPanel = EnsureRect(canvas.transform, canvas.transform, "StressControlsPanel");
            RectTransform moduleTitle = EnsureRect(stressPanel, stressPanel, "Text - ModuleHeader");
            RectTransform moduleSelector = EnsureRect(stressPanel, stressPanel, "ModuleSelector");
            RectTransform moduleSelectorName = EnsureRect(moduleSelector, moduleSelector, "Text - ModuleValue");
            RectTransform variantTitle = EnsureRect(stressPanel, stressPanel, "Text - VariantHeader");
            RectTransform variantSelector = EnsureRect(stressPanel, stressPanel, "VariantSelector");
            RectTransform variantSelectorName = EnsureRect(variantSelector, variantSelector, "Text - VariantValue");
            RectTransform variantSelectorIcon = FindDeep(variantSelector, "Image - VariantSelectorIcon") as RectTransform
                                                ?? EnsureRect(variantSelector, variantSelector, "Image - VariantSelectorIcon", "Image");
            RectTransform previousVariantButton = EnsureRect(variantSelector, variantSelector, "PreviousVariantButton");
            RectTransform nextVariantButton = EnsureRect(variantSelector, variantSelector, "NextVariantButton");
            RectTransform stressTitle = EnsureRect(stressPanel, stressPanel, "Text - StressHeader");
            RectTransform stressRow = EnsureRect(stressPanel, stressPanel, "HorizontalLayout - StressPresets");
            RectTransform stressStatusText = EnsureRect(stressPanel, stressPanel, "Text - StressSummary");
            RectTransform stressStatusIcon = FindDeep(stressPanel, "Image - StressStatusIcon") as RectTransform
                                             ?? FindDeep(stressPanel, "StressStatusIcon") as RectTransform;

            RectTransform moduleSection = EnsureRectChild(stressPanel, "Container - ModuleSection");
            RectTransform variantSection = EnsureRectChild(stressPanel, "Container - VariantSection");
            RectTransform stressSection = EnsureRectChild(stressPanel, "Container - StressSection");
            RectTransform stressContent = EnsureRectChild(stressSection, "Container - StressContent");
            RectTransform stressStatus = stressContent.Find("Container - StressSummary") as RectTransform;
            RectTransform legacyStressStatus = stressSection.Find("Container - StressSummary") as RectTransform;
            if (stressStatus == null && legacyStressStatus != null)
            {
                stressStatus = legacyStressStatus;
            }
            else if (stressStatus != null && legacyStressStatus != null)
            {
                Object.DestroyImmediate(legacyStressStatus.gameObject);
            }

            if (stressStatus == null)
                stressStatus = EnsureRectChild(stressContent, "Container - StressSummary");

            SetRect(moduleSection, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(332f, -36f));
            SetRect(variantSection, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(372f, 0f), new Vector2(332f, -36f));
            SetRect(stressSection, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(724f, 0f), new Vector2(-744f, -36f));

            moduleTitle.SetParent(moduleSection, false);
            moduleSelector.SetParent(moduleSection, false);
            variantTitle.SetParent(variantSection, false);
            variantSelector.SetParent(variantSection, false);
            stressTitle.SetParent(stressSection, false);
            stressContent.SetParent(stressSection, false);
            stressRow.SetParent(stressContent, false);
            stressStatus.SetParent(stressContent, false);
            stressStatusText.SetParent(stressStatus, false);
            if (stressStatusIcon != null)
                stressStatusIcon.SetParent(stressStatus, false);

            SetRect(moduleTitle, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -18f), new Vector2(0f, 18f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(moduleTitle.gameObject), "SELECT MODULE", 16f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

            SetRect(moduleSelector, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 18f), new Vector2(0f, 58f));
            ConfigureImage(GetOrAdd<Image>(moduleSelector.gameObject), frame8WhiteSprite, new Color(0.070f, 0.083f, 0.102f, 0.96f), Image.Type.Sliced, false);
            SetRect(moduleSelectorName, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, 0f), new Vector2(210f, 22f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(moduleSelectorName.gameObject), "Effects", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);
            GameObject moduleSelectIcon = EnsureChild(moduleSelector, "Image - ModuleSelectorIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            SetRect(moduleSelectIcon.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(20f, 20f));
            ConfigureImage(moduleSelectIcon.GetComponent<Image>(), moduleIconSprite, ShowcasePalette.TextPrimary, Image.Type.Simple, false);

            SetRect(variantTitle, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -18f), new Vector2(0f, 18f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(variantTitle.gameObject), "SELECT VARIANT", 16f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);
            SetRect(variantSelector, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 18f), new Vector2(0f, 58f));
            ConfigureImage(GetOrAdd<Image>(variantSelector.gameObject), frame8WhiteSprite, new Color(0.070f, 0.083f, 0.102f, 0.96f), Image.Type.Sliced, false);
            SetRect(variantSelectorName, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, 0f), new Vector2(180f, 22f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(variantSelectorName.gameObject), "Chunk-based", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);
            SetRect(variantSelectorIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(20f, 20f));
            ConfigureImage(GetOrAdd<Image>(variantSelectorIcon.gameObject), variantIconSprite, ShowcasePalette.TextPrimary, Image.Type.Simple, false);

            SetRect(previousVariantButton, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-68f, 0f), new Vector2(36f, 36f));
            ConfigureImage(GetOrAdd<Image>(previousVariantButton.gameObject), frame8WhiteSprite, new Color(0.045f, 0.055f, 0.070f, 0.96f), Image.Type.Sliced, true);
            ConfigureText(GetOrAdd<TextMeshProUGUI>(EnsureRect(previousVariantButton, previousVariantButton, "Label", "Text - Label").gameObject), "\u2039", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Center);
            SetRect(nextVariantButton, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(36f, 36f));
            ConfigureImage(GetOrAdd<Image>(nextVariantButton.gameObject), frame8WhiteSprite, new Color(0.045f, 0.055f, 0.070f, 0.96f), Image.Type.Sliced, true);
            ConfigureText(GetOrAdd<TextMeshProUGUI>(EnsureRect(nextVariantButton, nextVariantButton, "Label", "Text - Label").gameObject), "\u203A", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Center);

            SetRect(stressTitle, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -18f), new Vector2(0f, 18f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(stressTitle.gameObject), "STRESS TEST", 16f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

            stressContent.anchorMin = new Vector2(0f, 0f);
            stressContent.anchorMax = new Vector2(1f, 1f);
            stressContent.pivot = new Vector2(0.5f, 0.5f);
            stressContent.offsetMin = new Vector2(0f, 18f);
            stressContent.offsetMax = new Vector2(0f, -28f);
            stressContent.localScale = Vector3.one;
            stressContent.localRotation = Quaternion.identity;

            HorizontalLayoutGroup stressLayout = GetOrAdd<HorizontalLayoutGroup>(stressRow.gameObject);
            stressLayout.padding = new RectOffset(0, 0, 0, 0);
            stressLayout.spacing = 12f;
            stressLayout.childAlignment = TextAnchor.MiddleLeft;
            stressLayout.childControlWidth = false;
            stressLayout.childControlHeight = false;
            stressLayout.childForceExpandWidth = false;
            stressLayout.childForceExpandHeight = false;

            ContentSizeFitter stressRowFitter = GetOrAdd<ContentSizeFitter>(stressRow.gameObject);
            stressRowFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            stressRowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            SetRect(stressRow, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(0f, 50f));

            string[] labels = { "1 000", "5 000", "10 000" };
            string[] buttonNames = { "Button - StressPreset01", "Button - StressPreset02", "Button - StressPreset03" };
            for (int i = 0; i < buttonNames.Length; i++)
            {
                RectTransform button = Rect(canvas.transform, buttonNames[i]);
                button.SetParent(stressRow, false);
                SetRect(button, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(68f, 50f));
                LayoutElement element = GetOrAdd<LayoutElement>(button.gameObject);
                element.minWidth = 68f;
                element.preferredWidth = 68f;
                element.minHeight = 50f;
                element.preferredHeight = 50f;
                Color buttonColor = i == 2
                    ? new Color(0.145f, 0.083f, 0.045f, 0.98f)
                    : new Color(0.045f, 0.055f, 0.070f, 0.96f);
                Color labelColor = i == 2
                    ? new Color(1f, 0.765f, 0.550f, 1f)
                    : ShowcasePalette.TextSecondary;
                ConfigureImage(GetOrAdd<Image>(button.gameObject), i == 2 ? frame8YellowSprite : frame8WhiteSprite, buttonColor, Image.Type.Sliced, true);

                RectTransform presetValue = EnsureChild(button, "Text - PresetValue", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)).GetComponent<RectTransform>();
                StretchFull(presetValue);
                ConfigureText(GetOrAdd<TextMeshProUGUI>(presetValue.gameObject), labels[i], 18f, labelColor, TextAlignmentOptions.Center);

                Transform legacyLabel = button.Find("Label");
                if (legacyLabel != null)
                    legacyLabel.gameObject.SetActive(false);
            }

            SetRect(stressStatus, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(286f, 50f));
            SetRect(stressStatusText, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(-30f, 0f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(stressStatusText.gameObject), "<size=72%><color=#" + ShowcasePalette.TextSecondaryHex + ">Stress Load</color></size>\n<color=#" + ShowcasePalette.AccentHex + "><size=118%><b>10K</b></size></color>  <size=84%>Active items 10K</size>", 17f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);

            if (stressStatusIcon == null)
                stressStatusIcon = EnsureRectChild(stressStatus, "Image - StressStatusIcon");

            SetRect(stressStatusIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(18f, 18f));
            ConfigureImage(GetOrAdd<Image>(stressStatusIcon.gameObject), stressIconSprite, ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.78f), Image.Type.Simple, false);
        }

        private static void ConfigureStatusCard(Canvas canvas, RectTransform systemStatusCard, Sprite frame16Sprite, Sprite frame8WhiteSprite, Sprite lineGreenSprite)
        {
            SetRect(systemStatusCard, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f), new Vector2(500f, 118f));
            ConfigureCard(systemStatusCard, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.95f), 0.025f, 0.28f, false);

            RectTransform statusTitle = EnsureRect(systemStatusCard, systemStatusCard, "StatusTitle");
            SetRect(statusTitle, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -18f), new Vector2(220f, 18f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(statusTitle.gameObject), "SYSTEM STATUS", 16f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);

            GameObject statusDot = EnsureChild(systemStatusCard, "StatusDot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            SetRect(statusDot.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -20f), new Vector2(12f, 12f));
            ConfigureImage(statusDot.GetComponent<Image>(), null, ShowcasePalette.Success, Image.Type.Simple, false);

            string[] statusTexts = { "CPU 38%", "GPU 52%", "MEM 6.1 GB" };
            for (int i = 0; i < statusTexts.Length; i++)
            {
                RectTransform metric = EnsureRect(systemStatusCard, systemStatusCard, "StatusMetric_" + i);
                RectTransform bar = EnsureRect(systemStatusCard, systemStatusCard, "StatusBar_" + i);
                RectTransform fill = EnsureRect(bar, bar, "Fill");
                float startX = 18f + i * 154f;
                SetRect(metric, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(startX, -42f), new Vector2(132f, 16f));
                ConfigureText(GetOrAdd<TextMeshProUGUI>(metric.gameObject), statusTexts[i], 14f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

                SetRect(bar, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(startX, -66f), new Vector2(110f, 8f));
                ConfigureImage(GetOrAdd<Image>(bar.gameObject), frame8WhiteSprite, new Color(0.070f, 0.083f, 0.102f, 0.90f), Image.Type.Sliced, false);

                StretchInside(fill, 1f, 30f - i * 8f, 1f, 1f);
                ConfigureImage(GetOrAdd<Image>(fill.gameObject), lineGreenSprite, ShowcasePalette.Success, Image.Type.Sliced, false);
            }
        }

        private static void ConfigureNavButtons(Canvas canvas, RectTransform navPanel, Sprite frame8WhiteSprite, Sprite frame8YellowSprite)
        {
            StretchTop(navPanel, 18f, 18f, 48f, 214f);
            ConfigureImage(GetOrAdd<Image>(navPanel.gameObject), null, new Color(1f, 1f, 1f, 0f), Image.Type.Simple, false);

            RectTransform previousModuleButton = EnsureRect(navPanel, navPanel, "PreviousModuleButton");
            RectTransform nextModuleButton = EnsureRect(navPanel, navPanel, "NextModuleButton");
            SetRect(previousModuleButton, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(510f, -28f), new Vector2(44f, 44f));
            ConfigureImage(GetOrAdd<Image>(previousModuleButton.gameObject), frame8WhiteSprite, new Color(0.055f, 0.067f, 0.082f, 0.96f), Image.Type.Sliced, true);
            ConfigureText(GetOrAdd<TextMeshProUGUI>(EnsureRect(previousModuleButton, previousModuleButton, "Text - Label", "Label").gameObject), "\u2039", 22f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Center);

            SetRect(nextModuleButton, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(562f, -28f), new Vector2(44f, 44f));
            ConfigureImage(GetOrAdd<Image>(nextModuleButton.gameObject), frame8YellowSprite, new Color(0.110f, 0.071f, 0.047f, 0.98f), Image.Type.Sliced, true);
            ConfigureText(GetOrAdd<TextMeshProUGUI>(EnsureRect(nextModuleButton, nextModuleButton, "Text - Label", "Label").gameObject), "\u203A", 22f, ShowcasePalette.AccentMain, TextAlignmentOptions.Center);
            AddAccentGlow(nextModuleButton, "Image - ActiveGlow", ShowcasePalette.AccentSoft(0.28f), new Vector2(58f, 58f));

            Transform legacyGlow = nextModuleButton.Find("ActiveGlow");
            if (legacyGlow != null)
                Object.DestroyImmediate(legacyGlow.gameObject);
        }

        private static void ConfigureModuleStats(RectTransform headerPanel, string[] labels, string[] values, Sprite[] icons)
        {
            RectTransform statsRow = EnsureRect(headerPanel, headerPanel, "HorizontalLayout - ModuleStats");
            SetRect(statsRow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(28f, 8f), new Vector2(-56f, 54f));

            HorizontalLayoutGroup layoutGroup = GetOrAdd<HorizontalLayoutGroup>(statsRow.gameObject);
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = 12f;
            layoutGroup.childAlignment = TextAnchor.LowerLeft;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            for (int i = 0; i < labels.Length; i++)
            {
                string containerName = i == 0 ? "Container - ModuleStat" : $"Container - ModuleStat ({i})";
                RectTransform container = EnsureRect(statsRow, statsRow, containerName);
                LayoutElement element = GetOrAdd<LayoutElement>(container.gameObject);
                element.minWidth = 102f;
                element.preferredWidth = 102f;
                element.minHeight = 40f;
                element.preferredHeight = 40f;

                RectTransform icon = EnsureRect(container, container, "Image - StatIcon");
                RectTransform label = EnsureRect(container, container, "Text - StatLabel");
                RectTransform value = EnsureRect(container, container, "Text - StatValue");

                SetRect(icon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 8f), new Vector2(14f, 14f));
                ConfigureImage(GetOrAdd<Image>(icon.gameObject), icons[i], ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.68f), Image.Type.Simple, false);

                SetRect(label, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(20f, 22f), new Vector2(-20f, 14f));
                ConfigureText(GetOrAdd<TextMeshProUGUI>(label.gameObject), labels[i], 11f, ShowcasePalette.TextMuted, TextAlignmentOptions.Left);

                SetRect(value, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(20f, 6f), new Vector2(-20f, 20f));
                ConfigureText(GetOrAdd<TextMeshProUGUI>(value.gameObject), values[i], 15f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);
            }
        }

        private static void ConfigureStructuredPerformanceCard(RectTransform performanceCard, Sprite perfIconSprite, Sprite lineSprite)
        {
            RectTransform header = EnsureRect(performanceCard, performanceCard, "Text - Header");
            RectTransform insight = EnsureRect(performanceCard, performanceCard, "Text - PerformanceInsight", "MetricsOverlay");
            RectTransform chartPlaceholder = EnsureRect(performanceCard, performanceCard, "Container - ChartPlaceholder", "ChartPlaceholder");
            RectTransform performanceGraph = EnsureRect(chartPlaceholder, chartPlaceholder, "PerformanceGraph");
            RectTransform performanceValues = EnsureRect(chartPlaceholder, chartPlaceholder, "Container - PerformanceValue");
            RectTransform chartSummary = EnsureRect(chartPlaceholder, chartPlaceholder, "Label");
            RectTransform statsRoot = EnsureRect(performanceCard, performanceCard, "Container - PerformanceStats", "Container - PerfomanceStats");

            GameObject perfIcon = EnsureChild(performanceCard, "Image - PerformanceIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            SetRect(perfIcon.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(14f, 14f));
            ConfigureImage(perfIcon.GetComponent<Image>(), perfIconSprite, ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.78f), Image.Type.Simple, false);

            SetRect(header, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -18f), new Vector2(220f, 18f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(header.gameObject), "PERFORMANCE", 16f, ShowcasePalette.AccentMain, TextAlignmentOptions.Left);

            SetRect(insight, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(18f, -44f), new Vector2(-36f, 38f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(insight.gameObject), "Live metrics show frame-level behavior and module-local CPU cost.", 13f, ShowcasePalette.TextSecondary, TextAlignmentOptions.TopLeft);

            SetRect(chartPlaceholder, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(18f, 138f), new Vector2(-36f, 84f));
            ConfigureImage(GetOrAdd<Image>(chartPlaceholder.gameObject), lineSprite, ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.055f), Image.Type.Sliced, false);

            SetRect(performanceGraph, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            GetOrAdd<PerformanceGraph>(performanceGraph.gameObject);

            SetRect(chartSummary, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -8f), new Vector2(180f, 16f));
            ConfigureText(GetOrAdd<TextMeshProUGUI>(chartSummary.gameObject), "Realtime chart", 12f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

            performanceValues.anchorMin = new Vector2(1f, 0f);
            performanceValues.anchorMax = new Vector2(1f, 1f);
            performanceValues.pivot = new Vector2(1f, 0.5f);
            performanceValues.offsetMin = new Vector2(-48f, 8f);
            performanceValues.offsetMax = new Vector2(-8f, -8f);
            performanceValues.localScale = Vector3.one;
            performanceValues.localRotation = Quaternion.identity;

            VerticalLayoutGroup valuesLayout = GetOrAdd<VerticalLayoutGroup>(performanceValues.gameObject);
            valuesLayout.padding = new RectOffset(0, 0, 0, 0);
            valuesLayout.spacing = 6f;
            valuesLayout.childAlignment = TextAnchor.UpperRight;
            valuesLayout.childControlWidth = true;
            valuesLayout.childControlHeight = false;
            valuesLayout.childForceExpandWidth = false;
            valuesLayout.childForceExpandHeight = false;

            string[] chartValues = { "144", "72", "0" };
            for (int i = 0; i < chartValues.Length; i++)
            {
                RectTransform value = EnsureRect(performanceValues, performanceValues, $"Text - Value{i}");
                LayoutElement element = GetOrAdd<LayoutElement>(value.gameObject);
                element.minHeight = 16f;
                element.preferredHeight = 16f;
                ConfigureText(GetOrAdd<TextMeshProUGUI>(value.gameObject), chartValues[i], 12f, i == 0 ? ShowcasePalette.AccentMain : ShowcasePalette.TextSecondary, TextAlignmentOptions.Right);
            }

            statsRoot.anchorMin = new Vector2(0f, 0f);
            statsRoot.anchorMax = new Vector2(1f, 0f);
            statsRoot.pivot = new Vector2(0.5f, 0f);
            statsRoot.offsetMin = new Vector2(18f, 18f);
            statsRoot.offsetMax = new Vector2(-18f, 120f);
            statsRoot.localScale = Vector3.one;
            statsRoot.localRotation = Quaternion.identity;

            VerticalLayoutGroup statsLayout = GetOrAdd<VerticalLayoutGroup>(statsRoot.gameObject);
            statsLayout.padding = new RectOffset(0, 0, 0, 0);
            statsLayout.spacing = 8f;
            statsLayout.childAlignment = TextAnchor.UpperLeft;
            statsLayout.childControlWidth = true;
            statsLayout.childControlHeight = false;
            statsLayout.childForceExpandWidth = true;
            statsLayout.childForceExpandHeight = false;

            string[] labels = { "FPS", "Frame ms", "Module CPU", "Simulated", "Visible" };
            string[] values = { "142 FPS", "7.0 ms", "1.4 ms", "10,000", "320" };
            for (int i = 0; i < labels.Length; i++)
            {
                string rowName = $"Container - Stat ({i + 1})";
                RectTransform row = EnsureRect(statsRoot, statsRoot, rowName);
                row.anchorMin = new Vector2(0f, 1f);
                row.anchorMax = new Vector2(1f, 1f);
                row.pivot = new Vector2(0.5f, 1f);
                row.sizeDelta = new Vector2(0f, 18f);
                LayoutElement rowLayout = GetOrAdd<LayoutElement>(row.gameObject);
                rowLayout.minWidth = 0f;
                rowLayout.preferredWidth = 0f;
                rowLayout.flexibleWidth = 1f;
                rowLayout.minHeight = 18f;
                rowLayout.preferredHeight = 18f;

                RectTransform label = EnsureRect(row, row, "Text -StatName", "Text - StatName");
                RectTransform value = EnsureRect(row, row, "Text -StatValue", "Text - StatValue");

                SetRect(label, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(140f, 18f));
                ConfigureText(GetOrAdd<TextMeshProUGUI>(label.gameObject), labels[i], 14f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

                SetRect(value, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(140f, 18f));
                ConfigureText(GetOrAdd<TextMeshProUGUI>(value.gameObject), values[i], 14f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Right);
            }
        }

        private static void ConfigureAccent(RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size, float alpha, bool right = false)
        {
            GameObject accent = EnsureChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = accent.GetComponent<RectTransform>();
            SetRect(rect, right ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f), right ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f), right ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f), anchoredPosition, size);
            ConfigureImage(accent.GetComponent<Image>(), null, ShowcasePalette.AccentSoft(alpha), Image.Type.Simple, false);
        }

        private static void ConfigureCard(RectTransform rect, Sprite sprite, Color backgroundColor, float topSheenAlpha, float bottomShadeAlpha, bool raycast = true)
        {
            ConfigureImage(GetOrAdd<Image>(rect.gameObject), sprite, backgroundColor, Image.Type.Sliced, raycast);
            Outline outline = GetOrAdd<Outline>(rect.gameObject);
            outline.effectColor = ShowcasePalette.BorderMain(0.035f);
            outline.effectDistance = new Vector2(1f, -1f);
            ConfigurePanelGradient(rect, topSheenAlpha, bottomShadeAlpha);
        }

        private static void ConfigurePanelGradient(RectTransform rect, float topSheenAlpha, float bottomShadeAlpha)
        {
            GameObject topSheen = EnsureChild(rect, "TopSheen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform topRect = topSheen.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 0.52f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.offsetMin = new Vector2(1f, 1f);
            topRect.offsetMax = new Vector2(-1f, -1f);
            topRect.SetAsFirstSibling();
            ConfigureImage(topSheen.GetComponent<Image>(), null, ShowcasePalette.AccentSoft(topSheenAlpha), Image.Type.Simple, false);

            GameObject bottomShade = EnsureChild(rect, "BottomShade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform shadeRect = bottomShade.GetComponent<RectTransform>();
            shadeRect.anchorMin = Vector2.zero;
            shadeRect.anchorMax = new Vector2(1f, 0.64f);
            shadeRect.pivot = new Vector2(0.5f, 0f);
            shadeRect.offsetMin = new Vector2(1f, 1f);
            shadeRect.offsetMax = new Vector2(-1f, -1f);
            shadeRect.SetAsFirstSibling();
            ConfigureImage(bottomShade.GetComponent<Image>(), null, new Color(0f, 0f, 0f, bottomShadeAlpha), Image.Type.Simple, false);
        }

        private static void CleanupLegacyCanvases(Transform root)
        {
            if (root == null)
                return;

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                Canvas childCanvas = child.GetComponent<Canvas>();
                if (childCanvas == null)
                    continue;

                if (FindDeep(child, "RootFrame") != null)
                    continue;

                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void AddAccentGlow(RectTransform parent, string name, Color color, Vector2 size)
        {
            GameObject glow = EnsureChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform glowRect = glow.GetComponent<RectTransform>();
            SetRect(glowRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            glowRect.SetAsFirstSibling();
            ConfigureImage(glow.GetComponent<Image>(), null, color, Image.Type.Simple, false);
        }

        private static void ConfigureImage(Image image, Sprite sprite, Color color, Image.Type type, bool raycast)
        {
            image.sprite = sprite;
            image.type = sprite != null ? type : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = raycast;
            image.preserveAspect = false;
        }

        private static void ConfigureText(TextMeshProUGUI text, string value, float size, Color color, TextAlignmentOptions alignment)
        {
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            text.richText = true;
            text.margin = Vector4.zero;

            if (TMP_Settings.defaultFontAsset != null)
                text.font = TMP_Settings.defaultFontAsset;
        }

        private static RectTransform Rect(Transform root, string name)
        {
            return FindDeep(root, name) as RectTransform;
        }

        private static RectTransform EnsureRect(Transform searchRoot, Transform createParent, string primaryName, params string[] aliases)
        {
            Transform match = FindDeepAny(searchRoot, primaryName, aliases);
            if (match is RectTransform rect)
                return rect;

            return EnsureRectChild(createParent, primaryName);
        }

        private static Canvas FindCanvasByChild(Transform root, string childName)
        {
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (FindDeep(canvases[i].transform, childName) != null)
                    return canvases[i];
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

        private static Transform FindDeepAny(Transform root, string primaryName, params string[] aliases)
        {
            Transform match = FindDeep(root, primaryName);
            if (match != null)
                return match;

            if (aliases == null)
                return null;

            for (int i = 0; i < aliases.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(aliases[i]))
                    continue;

                match = FindDeep(root, aliases[i]);
                if (match != null)
                    return match;
            }

            return null;
        }

        private static void CleanupTabsBarChildren(RectTransform tabsBar)
        {
            if (tabsBar == null)
                return;

            for (int i = tabsBar.childCount - 1; i >= 0; i--)
            {
                Transform child = tabsBar.GetChild(i);
                if (child.name == "ActiveTabUnderline")
                    continue;

                if (child.name == "Tab_0" || child.name == "Tab_1" || child.name == "Tab_2")
                    continue;

                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void ResetDescriptionViewport(RectTransform viewport)
        {
            if (viewport == null)
                return;

            for (int i = viewport.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(viewport.GetChild(i).gameObject);
        }

        private static bool HasCompositeDescriptionContent(RectTransform viewport)
        {
            return viewport != null &&
                   FindDeep(viewport, "Container - AboutInfo") != null &&
                   FindDeep(viewport, "Container - ArchitectureInfo") != null &&
                   FindDeep(viewport, "Container - Trade-OffsInfo") != null &&
                   FindDeep(viewport, "Container - ProsCons") != null;
        }

        private static void EnsureCompositeDescriptionSections(RectTransform viewport)
        {
            if (viewport == null)
                return;

            RectTransform aboutInfo = EnsureDescriptionSection(viewport, "Container - AboutInfo", 156f);
            RectTransform architectureInfo = EnsureDescriptionSection(viewport, "Container - ArchitectureInfo", 176f);
            RectTransform tradeOffsInfo = EnsureDescriptionSection(viewport, "Container - Trade-OffsInfo", 156f);
            RectTransform prosCons = EnsureRectChild(viewport, "Container - ProsCons");

            prosCons.anchorMin = new Vector2(0f, 1f);
            prosCons.anchorMax = new Vector2(1f, 1f);
            prosCons.pivot = new Vector2(0.5f, 1f);
            prosCons.sizeDelta = new Vector2(0f, 286f);

            LayoutElement prosConsLayout = GetOrAdd<LayoutElement>(prosCons.gameObject);
            prosConsLayout.minWidth = 0f;
            prosConsLayout.preferredWidth = 0f;
            prosConsLayout.flexibleWidth = 1f;
            prosConsLayout.minHeight = 286f;
            prosConsLayout.preferredHeight = 286f;

            VerticalLayoutGroup prosConsGroup = GetOrAdd<VerticalLayoutGroup>(prosCons.gameObject);
            prosConsGroup.padding = new RectOffset(0, 0, 0, 0);
            prosConsGroup.spacing = 14f;
            prosConsGroup.childAlignment = TextAnchor.UpperLeft;
            prosConsGroup.childControlWidth = true;
            prosConsGroup.childControlHeight = false;
            prosConsGroup.childForceExpandWidth = true;
            prosConsGroup.childForceExpandHeight = false;

            EnsureDescriptionSection(prosCons, "Container - Pros", 136f);
            EnsureDescriptionSection(prosCons, "Container - Cons", 136f);

            aboutInfo.SetAsFirstSibling();
            architectureInfo.SetAsLastSibling();
            architectureInfo.SetSiblingIndex(1);
            tradeOffsInfo.SetSiblingIndex(2);
            prosCons.SetSiblingIndex(3);
        }

        private static RectTransform EnsureDescriptionSection(Transform parent, string sectionName, float preferredHeight)
        {
            RectTransform section = EnsureRectChild(parent, sectionName);
            section.anchorMin = new Vector2(0f, 1f);
            section.anchorMax = new Vector2(1f, 1f);
            section.pivot = new Vector2(0.5f, 1f);
            section.sizeDelta = new Vector2(0f, preferredHeight);

            LayoutElement layout = GetOrAdd<LayoutElement>(section.gameObject);
            layout.minWidth = 0f;
            layout.preferredWidth = 0f;
            layout.flexibleWidth = 1f;
            layout.minHeight = preferredHeight;
            layout.preferredHeight = preferredHeight;

            Image background = GetOrAdd<Image>(section.gameObject);
            ConfigureImage(background, null, new Color(0.065f, 0.078f, 0.097f, 0.86f), Image.Type.Simple, false);

            RectTransform header = EnsureTextChild(section, "Text - Header", 13f, ShowcasePalette.AccentMain, TextAlignmentOptions.TopLeft);
            RectTransform body = EnsureTextChild(section, "Text - Description", 16f, ShowcasePalette.TextPrimary, TextAlignmentOptions.TopLeft);

            StretchTop(header, 14f, 14f, 12f, 18f);
            StretchInside(body, 14f, 14f, 12f, 40f);
            return section;
        }

        private static RectTransform EnsureTextChild(Transform parent, string name, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject textObject = EnsureChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(textObject);
            ConfigureText(text, name == "Text - Header" ? "SECTION" : "Description placeholder", fontSize, color, alignment);
            return text.rectTransform;
        }

        private static bool TryWireCompositeDescriptionContent(RectTransform viewport, ScrollRect scrollRect)
        {
            if (!HasCompositeDescriptionContent(viewport) || scrollRect == null)
                return false;

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
            layoutGroup.childForceExpandWidth = true;
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
            return true;
        }

        private static GameObject EnsureChild(Transform parent, string name, params System.Type[] components)
        {
            Transform child = parent.Find(name);
            if (child != null)
                return child.gameObject;

            GameObject go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static RectTransform EnsureRectChild(Transform parent, string name)
        {
            return EnsureChild(parent, name, typeof(RectTransform)).GetComponent<RectTransform>();
        }

        private static Transform EnsureTransformChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
                return child;

            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void StretchInside(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void StretchTop(RectTransform rect, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -(top + height));
            rect.offsetMax = new Vector2(-right, -top);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static ModuleDefinitionSO[] ResolveModules(ModuleDefinitionSO[] currentModules)
        {
            if (currentModules != null && currentModules.Length > 0)
                return currentModules;

            string[] guids = AssetDatabase.FindAssets("t:ModuleDefinitionSO");
            ModuleDefinitionSO[] modules = new ModuleDefinitionSO[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                modules[i] = AssetDatabase.LoadAssetAtPath<ModuleDefinitionSO>(path);
            }

            return modules;
        }
    }
}
