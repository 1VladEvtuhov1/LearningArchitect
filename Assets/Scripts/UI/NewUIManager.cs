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
        }

        private void PrepareComponents()
        {
            HubUI hubUi = GetComponent<HubUI>();
            if (hubUi != null)
            {
                hubUi.moduleColor = primaryTextColor;
                hubUi.variantColor = primaryTextColor;
                hubUi.hintColor = secondaryTextColor;
                hubUi.pulseColor = accentColor;
            }

            MetricsOverlay metrics = GetComponent<MetricsOverlay>();
            if (metrics != null)
            {
                metrics.healthyColor = metricsColor;
                metrics.warningColor = new Color(0.95f, 0.75f, 0.42f, 1f);
                metrics.criticalColor = new Color(0.95f, 0.45f, 0.42f, 1f);
            }

            StressTestControls stress = GetComponent<StressTestControls>();
            if (stress != null)
            {
                stress.activeColor = accentColor;
                stress.inactiveColor = softCardColor;
                stress.textColor = primaryTextColor;
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

            LayoutHeader(headerBackdrop, headerPanel);
            LayoutPreview(previewFrame);
            LayoutDescription(descriptionBackdrop, descriptionPanel);
            LayoutFooter(footerPanel);
            LayoutNavigation(navigationPanel, headerPanel);
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

            HubUI hubUi = GetComponent<HubUI>();
            if (hubUi != null)
            {
                hubUi.moduleName = moduleText.GetComponent<TextMeshProUGUI>();
                hubUi.variantName = variantText.GetComponent<TextMeshProUGUI>();
                hubUi.inputHints = inputHints.GetComponent<TextMeshProUGUI>();
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

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            viewportObject.transform.SetParent(panel, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            StretchInside(viewport, 14f, 14f, 14f, 14f);

            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.001f);
            viewportImage.raycastTarget = true;

            RectTransform descriptionText = CreateText("DescriptionText", viewport, "Loading description...", 18f, primaryTextColor, TextAlignmentOptions.TopLeft);
            descriptionText.anchorMin = new Vector2(0f, 1f);
            descriptionText.anchorMax = new Vector2(1f, 1f);
            descriptionText.pivot = new Vector2(0.5f, 1f);
            descriptionText.anchoredPosition = Vector2.zero;
            descriptionText.sizeDelta = new Vector2(0f, 1200f);

            ScrollRect scrollRect = panel.gameObject.GetComponent<ScrollRect>();
            if (scrollRect == null)
                scrollRect = panel.gameObject.AddComponent<ScrollRect>();

            scrollRect.viewport = viewport;
            scrollRect.content = descriptionText;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 24f;

            DescriptionPanel description = GetComponent<DescriptionPanel>();
            if (description != null)
            {
                description.descriptionText = descriptionText.GetComponent<TextMeshProUGUI>();
                description.scrollRect = scrollRect;
            }
        }

        private void LayoutFooter(RectTransform panel)
        {
            StretchBottom(panel, SideMargin, SideMargin, BottomMargin, FooterHeight);

            RectTransform oneK = CreateButton("StressButton_1K", panel, "1K");
            RectTransform fiveK = CreateButton("StressButton_5K", panel, "5K");
            RectTransform tenK = CreateButton("StressButton_10K", panel, "10K");
            SetRect(oneK, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(34f, 0f), new Vector2(54f, 28f));
            SetRect(fiveK, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(100f, 0f), new Vector2(54f, 28f));
            SetRect(tenK, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(166f, 0f), new Vector2(54f, 28f));

            RectTransform status = CreateText("StressStatusText", panel, "Stress load  1K  \u2022  Active 1K", 14f, secondaryTextColor, TextAlignmentOptions.Left);
            SetRect(status, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(224f, 0f), new Vector2(360f, 18f));

            RectTransform metrics = CreateText("MetricsOverlay", panel, "<color=#8A8F99>\u25A5</color> <color=#2DD4BF>FPS 60</color>  <color=#8A8F99>|</color>  Frame 16.0 ms  <color=#8A8F99>|</color>  Active 1K", 14f, primaryTextColor, TextAlignmentOptions.Right);
            SetRect(metrics, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(360f, 18f));

            StressTestControls stress = GetComponent<StressTestControls>();
            if (stress != null)
            {
                stress.oneKButton = oneK.GetComponent<Button>();
                stress.fiveKButton = fiveK.GetComponent<Button>();
                stress.tenKButton = tenK.GetComponent<Button>();
                stress.statusText = status.GetComponent<TextMeshProUGUI>();
            }

            MetricsOverlay overlay = GetComponent<MetricsOverlay>();
            if (overlay != null)
                overlay.fpsText = metrics.GetComponent<TextMeshProUGUI>();
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
            tmp.enableWordWrapping = true;

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
    }
}
