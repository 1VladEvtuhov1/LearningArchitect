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
        private const string PrefabPath = "Assets/Prefabs/Showcase/ArchitectureShowcaseHub.prefab";

        [MenuItem("Tools/LearningArchitect/Rebuild Showcase Layout")]
        public static void RebuildMenu()
        {
            Rebuild();
        }

        [MenuItem("Tools/LearningArchitect/Apply Showcase Layout To Scene")]
        public static void ApplyToSceneMenu()
        {
            ApplyToScene();
        }

        public static string Rebuild()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

            try
            {
                ApplyLayout(prefabRoot);
                EditorUtility.SetDirty(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return "Rebuilt showcase layout.";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        public static string ApplyToScene()
        {
            GameObject sceneRoot = GameObject.Find("ArchitectureShowcaseHub");
            if (sceneRoot == null)
                return "ArchitectureShowcaseHub not found in scene.";

            ApplyLayout(sceneRoot);
            EditorUtility.SetDirty(sceneRoot);
            return "Applied showcase layout to scene.";
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

            Sprite borderSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Frames/Border_r16.png");
            Sprite frame16Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Frames/Frame_r16.png");
            Sprite frame8WhiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Frames/Frame_r8_white.png");
            Sprite frame8YellowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Frames/Frame_r8_yellow.png");
            Sprite effectIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/EffectSystem_icon.png");
            Sprite moduleIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Module_icon.png");
            Sprite variantIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Variant_icon.png");
            Sprite loadedIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Loaded_icon.png");
            Sprite orbitIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Orbit_icon.png");
            Sprite zoomIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Zoom_icon.png");
            Sprite perfIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Performance_icon.png");
            Sprite stressIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Icons/Stress_icon.png");
            Sprite lineSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Lines/Line.png");
            Sprite lineGreenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Content/UI/Lines/LineGreen.png");

            RectTransform tint = Rect(canvas.transform, "ShowcaseBackgroundTint");
            RectTransform rootFrame = Rect(canvas.transform, "RootFrame");
            RectTransform headerLine = Rect(canvas.transform, "HeaderLine");
            RectTransform breadcrumb = Rect(canvas.transform, "Breadcrumb");
            RectTransform headerPanel = Rect(canvas.transform, "HeaderPanel");
            RectTransform moduleIcon = Rect(canvas.transform, "ModuleIcon");
            RectTransform moduleName = Rect(canvas.transform, "ModuleName");
            RectTransform variantName = Rect(canvas.transform, "VariantName");
            RectTransform inputHints = Rect(canvas.transform, "InputHints");
            RectTransform infoDivider = Rect(canvas.transform, "InfoDivider");
            RectTransform performanceCard = Rect(canvas.transform, "PerformanceCard");
            RectTransform metricsOverlay = Rect(canvas.transform, "MetricsOverlay");
            RectTransform chartPlaceholder = Rect(canvas.transform, "ChartPlaceholder");
            RectTransform previewFrame = Rect(canvas.transform, "PreviewFrame");
            RectTransform previewLabel = Rect(canvas.transform, "PreviewLabel");
            RectTransform descriptionPanel = Rect(canvas.transform, "DescriptionPanel");
            RectTransform tabsBar = Rect(canvas.transform, "TabsBar");
            RectTransform activeTabUnderline = Rect(canvas.transform, "ActiveTabUnderline");
            RectTransform bodyDivider = Rect(canvas.transform, "BodyDivider");
            RectTransform viewport = Rect(canvas.transform, "Viewport");
            RectTransform descriptionText = Rect(canvas.transform, "DescriptionText");
            RectTransform stressPanel = Rect(canvas.transform, "StressControlsPanel");
            RectTransform systemStatusCard = Rect(canvas.transform, "SystemStatusCard");
            RectTransform navPanel = Rect(canvas.transform, "NavigationControlsPanel");
            RectTransform transitionOverlay = Rect(canvas.transform, "TransitionOverlay");

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
            ConfigureText(breadcrumb.GetComponent<TextMeshProUGUI>(), "ENGINE SYSTEMS / <color=#" + ShowcasePalette.AccentHex + ">VISUALIZER</color>", 18f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

            SetRect(headerPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -48f), new Vector2(622f, 214f));
            ConfigureCard(headerPanel, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.94f), 0.035f, 0.28f);

            SetRect(moduleIcon, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -26f), new Vector2(80f, 80f));
            ConfigureImage(GetOrAdd<Image>(moduleIcon.gameObject), effectIconSprite, Color.white, Image.Type.Simple, false);
            Outline moduleIconOutline = GetOrAdd<Outline>(moduleIcon.gameObject);
            moduleIconOutline.effectColor = ShowcasePalette.AccentSoft(0.10f);
            moduleIconOutline.effectDistance = new Vector2(0f, -1f);
            RectTransform iconGlyph = Rect(canvas.transform, "IconGlyph");
            if (iconGlyph != null)
                iconGlyph.gameObject.SetActive(false);

            SetRect(moduleName, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -18f), new Vector2(320f, 42f));
            ConfigureText(moduleName.GetComponent<TextMeshProUGUI>(), "Effects System", 28f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);
            SetRect(variantName, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -58f), new Vector2(360f, 28f));
            ConfigureText(variantName.GetComponent<TextMeshProUGUI>(), "<b>Chunk-based Variant</b>", 18f, ShowcasePalette.AccentMain, TextAlignmentOptions.Left);
            SetRect(inputHints, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116f, -88f), new Vector2(240f, 18f));
            ConfigureText(inputHints.GetComponent<TextMeshProUGUI>(), "Realtime architecture preview", 12f, ShowcasePalette.TextMuted, TextAlignmentOptions.Left);

            SetRect(infoDivider, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(-44f, 1f));
            ConfigureImage(GetOrAdd<Image>(infoDivider.gameObject), lineSprite, ShowcasePalette.BorderMain(0.075f), Image.Type.Sliced, false);

            string[] statLabels = { "MODULE", "VARIANT", "LOADED", "ORBIT", "ZOOM" };
            string[] statValues = { "Effects", "Chunk-based", "12 / 25", "RMB", "Wheel" };
            Sprite[] statIcons = { moduleIconSprite, variantIconSprite, loadedIconSprite, orbitIconSprite, zoomIconSprite };
            for (int i = 0; i < statLabels.Length; i++)
            {
                RectTransform labelRect = Rect(canvas.transform, "StatLabel_" + i);
                RectTransform valueRect = Rect(canvas.transform, "StatValue_" + i);
                float x = 30f + i * 108f;
                SetRect(labelRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(x + 20f, 22f), new Vector2(86f, 14f));
                ConfigureText(labelRect.GetComponent<TextMeshProUGUI>(), statLabels[i], 11f, ShowcasePalette.TextMuted, TextAlignmentOptions.Left);
                SetRect(valueRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(x + 20f, 6f), new Vector2(92f, 20f));
                ConfigureText(valueRect.GetComponent<TextMeshProUGUI>(), statValues[i], 15f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);

                GameObject icon = EnsureChild(headerPanel, "StatIcon_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                SetRect(icon.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(x, 18f), new Vector2(14f, 14f));
                ConfigureImage(icon.GetComponent<Image>(), statIcons[i], ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.68f), Image.Type.Simple, false);
            }

            SetRect(performanceCard, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18f, 196f), new Vector2(398f, 334f));
            ConfigureCard(performanceCard, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.94f), 0.030f, 0.28f);
            SetRect(metricsOverlay, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(20f, -18f), new Vector2(-40f, 148f));
            ConfigureText(metricsOverlay.GetComponent<TextMeshProUGUI>(), "<size=72%><color=#" + ShowcasePalette.AccentHex + "><b>PERFORMANCE</b></color></size>\n\n<color=#" + ShowcasePalette.TextSecondaryHex + ">FPS</color>               <size=140%><b>142</b></size>  <size=75%>FPS</size>\n\n<color=#" + ShowcasePalette.TextSecondaryHex + ">Frame Time</color>     7.0 ms\n\n<color=#" + ShowcasePalette.TextSecondaryHex + ">Particles</color>      10,000", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.TopLeft);
            GameObject perfIcon = EnsureChild(performanceCard, "PerformanceIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            SetRect(perfIcon.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(14f, 14f));
            ConfigureImage(perfIcon.GetComponent<Image>(), perfIconSprite, ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.78f), Image.Type.Simple, false);
            SetRect(chartPlaceholder, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(-36f, 102f));
            ConfigureImage(GetOrAdd<Image>(chartPlaceholder.gameObject), lineSprite, ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.055f), Image.Type.Simple, false);

            StretchInside(previewFrame, 286f, 514f, 138f, 74f);
            ConfigureImage(GetOrAdd<Image>(previewFrame.gameObject), null, new Color(1f, 1f, 1f, 0f), Image.Type.Simple, false);
            previewLabel.gameObject.SetActive(false);

            descriptionPanel.anchorMin = new Vector2(1f, 0f);
            descriptionPanel.anchorMax = new Vector2(1f, 1f);
            descriptionPanel.pivot = new Vector2(1f, 0.5f);
            descriptionPanel.offsetMin = new Vector2(-516f, 138f);
            descriptionPanel.offsetMax = new Vector2(-18f, -48f);
            ConfigureCard(descriptionPanel, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.95f), 0.030f, 0.30f);

            StretchTop(tabsBar, 12f, 12f, 12f, 76f);
            ConfigureImage(GetOrAdd<Image>(tabsBar.gameObject), frame8WhiteSprite, new Color(0.043f, 0.053f, 0.067f, 0.94f), Image.Type.Sliced, false);
            string[] tabs = { "Overview", "Architecture", "Trade-offs" };
            for (int i = 0; i < 3; i++)
            {
                RectTransform tabRect = Rect(canvas.transform, "Tab_" + i);
                SetRect(tabRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f + 138f * i, -2f), new Vector2(126f, 28f));
                ConfigureText(tabRect.GetComponent<TextMeshProUGUI>(), tabs[i], 16f, i == 0 ? ShowcasePalette.AccentMain : ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);
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

            descriptionText.anchorMin = new Vector2(0f, 1f);
            descriptionText.anchorMax = new Vector2(1f, 1f);
            descriptionText.pivot = new Vector2(0.5f, 1f);
            descriptionText.anchoredPosition = Vector2.zero;
            descriptionText.sizeDelta = new Vector2(0f, 1200f);
            ConfigureText(descriptionText.GetComponent<TextMeshProUGUI>(), "<color=#" + ShowcasePalette.AccentHex + "><size=74%><b>ABOUT</b></size></color>\nChunk-based processing groups particles into spatial chunks to improve cache locality and reduce draw calls.\n\n<color=#" + ShowcasePalette.AccentHex + "><size=74%><b>ARCHITECTURE</b></size></color>\nBatched data-oriented variant. Stores positions and velocities in arrays and updates them with a simple for loop.\n\n<color=#" + ShowcasePalette.AccentHex + "><size=74%><b>TRADE-OFFS</b></size></color>\nMuch better scaling than per-object behaviour, but less direct to inspect in the hierarchy.\n\n<color=#" + ShowcasePalette.SuccessHex + "><size=74%><b>PROS</b></size></color>\n\u2022 One batched update loop\n\u2022 Simulates more effects than it renders\n\u2022 Clearer path toward data-oriented optimization\n\n<color=#" + ShowcasePalette.ErrorHex + "><size=74%><b>CONS</b></size></color>\n\u2022 Less direct than object-per-effect\n\u2022 Needs custom visualization\n\u2022 More bookkeeping than the indie variant", 18f, new Color(0.830f, 0.850f, 0.880f, 1f), TextAlignmentOptions.TopLeft);

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

            ModuleManager moduleManager = prefabRoot.GetComponent<ModuleManager>();
            HubUI hubUI = prefabRoot.GetComponent<HubUI>();
            DescriptionPanel description = prefabRoot.GetComponent<DescriptionPanel>();
            MetricsOverlay metrics = prefabRoot.GetComponent<MetricsOverlay>();
            StressTestControls stress = prefabRoot.GetComponent<StressTestControls>();
            ModuleNavigationControls navigation = prefabRoot.GetComponent<ModuleNavigationControls>();
            GetOrAdd<ShowcaseLocalization>(prefabRoot);

            hubUI.moduleName = moduleName.GetComponent<TextMeshProUGUI>();
            hubUI.variantName = variantName.GetComponent<TextMeshProUGUI>();
            hubUI.inputHints = inputHints.GetComponent<TextMeshProUGUI>();
            hubUI.moduleSelectorName = Rect(canvas.transform, "ModuleSelectorName").GetComponent<TextMeshProUGUI>();
            hubUI.variantSelectorName = Rect(canvas.transform, "VariantSelectorName").GetComponent<TextMeshProUGUI>();
            hubUI.moduleColor = ShowcasePalette.TextPrimary;
            hubUI.variantColor = ShowcasePalette.AccentMain;
            hubUI.hintColor = ShowcasePalette.TextMuted;
            hubUI.pulseColor = ShowcasePalette.AccentStrong;

            ScrollRect scrollRect = GetOrAdd<ScrollRect>(descriptionPanel.gameObject);
            scrollRect.viewport = viewport;
            scrollRect.content = descriptionText;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;
            description.descriptionText = descriptionText.GetComponent<TextMeshProUGUI>();
            description.scrollRect = scrollRect;
            description.sectionTitleColor = ShowcasePalette.AccentMain;
            description.positiveTextColor = ShowcasePalette.Success;
            description.negativeTextColor = ShowcasePalette.Error;

            metrics.fpsText = metricsOverlay.GetComponent<TextMeshProUGUI>();
            metrics.titleColor = ShowcasePalette.AccentMain;
            metrics.labelColor = ShowcasePalette.TextSecondary;
            metrics.valueColor = ShowcasePalette.TextPrimary;
            metrics.graphBackgroundColor = ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.055f);
            stress.oneKButton = Rect(canvas.transform, "StressButton_1K").GetComponent<Button>();
            stress.fiveKButton = Rect(canvas.transform, "StressButton_5K").GetComponent<Button>();
            stress.tenKButton = Rect(canvas.transform, "StressButton_10K").GetComponent<Button>();
            stress.statusText = Rect(canvas.transform, "StressStatusText").GetComponent<TextMeshProUGUI>();
            stress.inactiveLabelColor = ShowcasePalette.TextSecondary;
            stress.hoverBackgroundColor = new Color(0.070f, 0.083f, 0.102f, 0.96f);
            stress.pressedBackgroundColor = new Color(0.145f, 0.083f, 0.045f, 0.98f);

            navigation.manager = moduleManager;
            navigation.hubUI = hubUI;
            navigation.panelSprite = frame8WhiteSprite;
            navigation.buttonSprite = frame8WhiteSprite;

            moduleManager.hubUI = hubUI;
            moduleManager.descriptionPanel = description;
            moduleManager.transitionOverlay = transitionOverlay.GetComponent<CanvasGroup>();
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
            RectTransform moduleTitle = Rect(canvas.transform, "ModuleSectionTitle");
            RectTransform moduleSelector = Rect(canvas.transform, "ModuleSelector");
            RectTransform moduleSelectorName = Rect(canvas.transform, "ModuleSelectorName");
            RectTransform variantTitle = Rect(canvas.transform, "VariantSectionTitle");
            RectTransform variantSelector = Rect(canvas.transform, "VariantSelector");
            RectTransform variantSelectorName = Rect(canvas.transform, "VariantSelectorName");
            RectTransform variantSelectorIcon = FindDeep(variantSelector, "Image") as RectTransform;
            RectTransform previousVariantButton = FindDeep(variantSelector, "PreviousVariantButton") as RectTransform;
            RectTransform nextVariantButton = FindDeep(variantSelector, "NextVariantButton") as RectTransform;
            RectTransform stressTitle = Rect(canvas.transform, "StressSectionTitle");
            RectTransform stressRow = Rect(canvas.transform, "StressButtonsRow");
            RectTransform stressStatusText = Rect(canvas.transform, "StressStatusText");

            SetRect(moduleTitle, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -18f), new Vector2(180f, 18f));
            ConfigureText(moduleTitle.GetComponent<TextMeshProUGUI>(), "SELECT MODULE", 16f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

            SetRect(moduleSelector, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 20f), new Vector2(330f, 58f));
            ConfigureImage(GetOrAdd<Image>(moduleSelector.gameObject), frame8WhiteSprite, new Color(0.070f, 0.083f, 0.102f, 0.96f), Image.Type.Sliced, false);
            SetRect(moduleSelectorName, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, 0f), new Vector2(210f, 22f));
            ConfigureText(moduleSelectorName.GetComponent<TextMeshProUGUI>(), "Effects", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);
            GameObject moduleSelectIcon = EnsureChild(moduleSelector, "ModuleSelectorIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            SetRect(moduleSelectIcon.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(20f, 20f));
            ConfigureImage(moduleSelectIcon.GetComponent<Image>(), moduleIconSprite, ShowcasePalette.TextPrimary, Image.Type.Simple, false);

            SetRect(variantTitle, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(384f, -18f), new Vector2(180f, 18f));
            ConfigureText(variantTitle.GetComponent<TextMeshProUGUI>(), "SELECT VARIANT", 16f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);
            SetRect(variantSelector, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(384f, 20f), new Vector2(330f, 58f));
            ConfigureImage(GetOrAdd<Image>(variantSelector.gameObject), frame8WhiteSprite, new Color(0.070f, 0.083f, 0.102f, 0.96f), Image.Type.Sliced, false);
            SetRect(variantSelectorName, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, 0f), new Vector2(180f, 22f));
            ConfigureText(variantSelectorName.GetComponent<TextMeshProUGUI>(), "Chunk-based", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);
            SetRect(variantSelectorIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(20f, 20f));
            ConfigureImage(GetOrAdd<Image>(variantSelectorIcon.gameObject), variantIconSprite, ShowcasePalette.TextPrimary, Image.Type.Simple, false);

            SetRect(previousVariantButton, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-68f, 0f), new Vector2(36f, 36f));
            ConfigureImage(GetOrAdd<Image>(previousVariantButton.gameObject), frame8WhiteSprite, new Color(0.045f, 0.055f, 0.070f, 0.96f), Image.Type.Sliced, true);
            ConfigureText(previousVariantButton.Find("Label").GetComponent<TextMeshProUGUI>(), "\u2039", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Center);
            SetRect(nextVariantButton, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(36f, 36f));
            ConfigureImage(GetOrAdd<Image>(nextVariantButton.gameObject), frame8WhiteSprite, new Color(0.045f, 0.055f, 0.070f, 0.96f), Image.Type.Sliced, true);
            ConfigureText(nextVariantButton.Find("Label").GetComponent<TextMeshProUGUI>(), "\u203A", 18f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Center);

            SetRect(stressTitle, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(748f, -18f), new Vector2(180f, 18f));
            ConfigureText(stressTitle.GetComponent<TextMeshProUGUI>(), "STRESS TEST", 16f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);
            SetRect(stressRow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(748f, 20f), new Vector2(-766f, 58f));

            string[] labels = { "1K", "5K", "10K" };
            string[] buttonNames = { "StressButton_1K", "StressButton_5K", "StressButton_10K" };
            for (int i = 0; i < buttonNames.Length; i++)
            {
                RectTransform button = Rect(canvas.transform, buttonNames[i]);
                SetRect(button, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(i * 78f, 0f), new Vector2(68f, 50f));
                Color buttonColor = i == 2
                    ? new Color(0.145f, 0.083f, 0.045f, 0.98f)
                    : new Color(0.045f, 0.055f, 0.070f, 0.96f);
                Color labelColor = i == 2
                    ? new Color(1f, 0.765f, 0.550f, 1f)
                    : ShowcasePalette.TextSecondary;
                ConfigureImage(GetOrAdd<Image>(button.gameObject), i == 2 ? frame8YellowSprite : frame8WhiteSprite, buttonColor, Image.Type.Sliced, true);
                ConfigureText(button.Find("Label").GetComponent<TextMeshProUGUI>(), labels[i], 18f, labelColor, TextAlignmentOptions.Center);
            }

            SetRect(stressStatusText, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(244f, 44f));
            ConfigureText(stressStatusText.GetComponent<TextMeshProUGUI>(), "<size=72%><color=#" + ShowcasePalette.TextSecondaryHex + ">Stress Load</color></size>\n<color=#" + ShowcasePalette.AccentHex + "><size=118%><b>10,000</b></size></color>", 17f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);

            GameObject stressStatusIcon = EnsureChild(stressRow, "StressStatusIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            SetRect(stressStatusIcon.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-192f, 0f), new Vector2(18f, 18f));
            ConfigureImage(stressStatusIcon.GetComponent<Image>(), stressIconSprite, ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.78f), Image.Type.Simple, false);
        }

        private static void ConfigureStatusCard(Canvas canvas, RectTransform systemStatusCard, Sprite frame16Sprite, Sprite frame8WhiteSprite, Sprite lineGreenSprite)
        {
            SetRect(systemStatusCard, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f), new Vector2(500f, 118f));
            ConfigureCard(systemStatusCard, frame16Sprite, new Color(0.050f, 0.061f, 0.075f, 0.95f), 0.025f, 0.28f, false);

            RectTransform statusTitle = Rect(canvas.transform, "StatusTitle");
            SetRect(statusTitle, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -18f), new Vector2(220f, 18f));
            ConfigureText(statusTitle.GetComponent<TextMeshProUGUI>(), "SYSTEM STATUS", 16f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Left);

            GameObject statusDot = EnsureChild(systemStatusCard, "StatusDot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            SetRect(statusDot.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -20f), new Vector2(12f, 12f));
            ConfigureImage(statusDot.GetComponent<Image>(), null, ShowcasePalette.Success, Image.Type.Simple, false);

            string[] statusTexts = { "CPU 38%", "GPU 52%", "MEM 6.1 GB" };
            for (int i = 0; i < statusTexts.Length; i++)
            {
                RectTransform metric = Rect(canvas.transform, "StatusMetric_" + i);
                RectTransform bar = Rect(canvas.transform, "StatusBar_" + i);
                RectTransform fill = bar.Find("Fill") as RectTransform;
                float startX = 18f + i * 154f;
                SetRect(metric, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(startX, -42f), new Vector2(132f, 16f));
                ConfigureText(metric.GetComponent<TextMeshProUGUI>(), statusTexts[i], 14f, ShowcasePalette.TextSecondary, TextAlignmentOptions.Left);

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

            RectTransform previousModuleButton = Rect(canvas.transform, "PreviousModuleButton");
            RectTransform nextModuleButton = Rect(canvas.transform, "NextModuleButton");
            SetRect(previousModuleButton, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(510f, -28f), new Vector2(44f, 44f));
            ConfigureImage(GetOrAdd<Image>(previousModuleButton.gameObject), frame8WhiteSprite, new Color(0.055f, 0.067f, 0.082f, 0.96f), Image.Type.Sliced, true);
            ConfigureText(previousModuleButton.Find("Label").GetComponent<TextMeshProUGUI>(), "\u2039", 22f, ShowcasePalette.TextPrimary, TextAlignmentOptions.Center);

            SetRect(nextModuleButton, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(562f, -28f), new Vector2(44f, 44f));
            ConfigureImage(GetOrAdd<Image>(nextModuleButton.gameObject), frame8YellowSprite, new Color(0.110f, 0.071f, 0.047f, 0.98f), Image.Type.Sliced, true);
            ConfigureText(nextModuleButton.Find("Label").GetComponent<TextMeshProUGUI>(), "\u203A", 22f, ShowcasePalette.AccentMain, TextAlignmentOptions.Center);
            AddAccentGlow(nextModuleButton, "ActiveGlow", ShowcasePalette.AccentSoft(0.28f), new Vector2(58f, 58f));
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

        private static GameObject EnsureChild(Transform parent, string name, params System.Type[] components)
        {
            Transform child = parent.Find(name);
            if (child != null)
                return child.gameObject;

            GameObject go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
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
    }
}
