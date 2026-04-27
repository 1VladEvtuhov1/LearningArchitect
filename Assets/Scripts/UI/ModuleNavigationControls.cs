using LearningArchitect.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LearningArchitect.UI
{
    public sealed class ModuleNavigationControls : MonoBehaviour
    {
        [Header("References")]
        public ModuleManager manager;
        public HubUI hubUI;
        public Sprite panelSprite;
        public Sprite buttonSprite;

        [Header("Layout")]
        public Vector2 panelSize = new Vector2(212f, 212f);
        public Vector2 panelOffset = new Vector2(0f, 132f);
        public Vector2 buttonSize = new Vector2(60f, 60f);
        public float buttonSpacing = 46f;

        [Header("Colors")]
        public Color panelColor = default;
        public Color panelGlowTopColor = default;
        public Color panelGlowBottomColor = default;
        public Color buttonColor = default;
        public Color buttonHoverColor = default;
        public Color buttonPressedColor = default;
        public Color buttonDisabledColor = default;
        public Color buttonGlowColor = default;
        public Color iconColor = default;
        public Color tooltipColor = default;
        public Color tooltipTextColor = default;

        [Header("Feedback")]
        public float pressedScale = 0.95f;
        public float buttonFadeDuration = 0.08f;

        private RectTransform panelRoot;
        private Button previousModuleButton;
        private Button nextModuleButton;
        private Button previousVariantButton;
        private Button nextVariantButton;
        private Button moduleSelectorButton;
        private Button variantSelectorButton;
        private TextMeshProUGUI previousModuleLabel;
        private TextMeshProUGUI nextModuleLabel;
        private TextMeshProUGUI previousVariantLabel;
        private TextMeshProUGUI nextVariantLabel;
        private RectTransform tooltipRoot;
        private TextMeshProUGUI tooltipLabel;
        private Button feedbackButton;
        private float feedbackTimer;
        private Sprite resolvedPanelSprite;
        private Sprite resolvedButtonSprite;
        private readonly Dictionary<Button, Color> buttonBaseColors = new Dictionary<Button, Color>();
        private readonly Dictionary<TextMeshProUGUI, Color> labelBaseColors = new Dictionary<TextMeshProUGUI, Color>();

        private const string UiSpritePath = "Assets/Prefabs/Showcase/Visuals/RoundedPanel.png";
        private static readonly Vector2 DefaultPanelSize = new Vector2(212f, 212f);
        private static readonly Vector2 DefaultPanelOffset = new Vector2(0f, 132f);
        private static readonly Vector2 DefaultButtonSize = new Vector2(60f, 60f);
        private static readonly Color DefaultPanelColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.84f);
        private static readonly Color DefaultPanelGlowTopColor = ShowcasePalette.AccentSoft(0.18f);
        private static readonly Color DefaultPanelGlowBottomColor = ShowcasePalette.AccentSoft(0.12f);
        private static readonly Color DefaultButtonColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelSoft, 0.98f);
        private static readonly Color DefaultButtonHoverColor = ShowcasePalette.PanelHover;
        private static readonly Color DefaultButtonPressedColor = ShowcasePalette.AccentStrong;
        private static readonly Color DefaultButtonDisabledColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelSoft, 0.42f);
        private static readonly Color DefaultButtonGlowColor = ShowcasePalette.AccentSoft(0.14f);
        private static readonly Color DefaultIconColor = ShowcasePalette.TextPrimary;
        private static readonly Color DefaultTooltipColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.96f);
        private static readonly Color DefaultTooltipTextColor = ShowcasePalette.TextPrimary;
        private const float DefaultButtonSpacing = 46f;
        private const float DefaultPressedScale = 0.95f;
        private const float DefaultButtonFadeDuration = 0.08f;

        private void Awake()
        {
            ApplyRuntimeDefaults();
            ResolveSprites();

            if (manager == null)
                manager = GetComponent<ModuleManager>();

            if (hubUI == null)
                hubUI = GetComponent<HubUI>();

            EnsureUi();
            CacheBaseStyles();
            BindButtons();
            Refresh();
        }

        private void Update()
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.unscaledDeltaTime;
                if (feedbackTimer <= 0f)
                    feedbackButton = null;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            RemoveListeners(previousModuleButton);
            RemoveListeners(nextModuleButton);
            RemoveListeners(previousVariantButton);
            RemoveListeners(nextVariantButton);
            RemoveListeners(moduleSelectorButton);
            RemoveListeners(variantSelectorButton);
        }

        public void PreviousModulePressed()
        {
            if (manager == null)
                return;

            if (hubUI != null)
                hubUI.PlayModuleSwitchFeedback(-1);

            manager.PreviousModule();
            PlayFeedback(previousModuleButton);
        }

        public void NextModulePressed()
        {
            if (manager == null)
                return;

            if (hubUI != null)
                hubUI.PlayModuleSwitchFeedback(1);

            manager.NextModule();
            PlayFeedback(nextModuleButton);
        }

        public void PreviousVariantPressed()
        {
            if (manager == null)
                return;

            if (hubUI != null)
                hubUI.PlayVariantSwitchFeedback(-1);

            manager.PreviousVariant();
            PlayFeedback(previousVariantButton);
        }

        public void NextVariantPressed()
        {
            if (manager == null)
                return;

            if (hubUI != null)
                hubUI.PlayVariantSwitchFeedback(1);

            manager.NextVariant();
            PlayFeedback(nextVariantButton);
        }

        private void EnsureUi()
        {
            if (panelRoot != null)
                return;

            Canvas canvas = FindCanvasByChild(transform, "RootFrame");
            if (canvas == null)
                return;

            Transform existing = FindDescendant(canvas.transform, "NavigationControlsPanel");
            if (existing != null)
            {
                panelRoot = existing.GetComponent<RectTransform>();
                previousModuleButton = FindButton(panelRoot, "PreviousModuleButton", ref previousModuleLabel);
                nextModuleButton = FindButton(panelRoot, "NextModuleButton", ref nextModuleLabel);
                previousVariantButton = FindButton(panelRoot, "PreviousVariantButton", ref previousVariantLabel);
                nextVariantButton = FindButton(panelRoot, "NextVariantButton", ref nextVariantLabel);
                moduleSelectorButton = EnsureSelectorButton(canvas.transform, "ModuleSelector");
                variantSelectorButton = EnsureSelectorButton(canvas.transform, "VariantSelector");
                EnsureTooltip(panelRoot);
                return;
            }

            GameObject panelObject = new GameObject("NavigationControlsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(canvas.transform, false);

            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(0.5f, 0f);
            panelRoot.anchorMax = new Vector2(0.5f, 0f);
            panelRoot.pivot = new Vector2(0.5f, 0f);
            panelRoot.sizeDelta = panelSize;
            panelRoot.anchoredPosition = panelOffset;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.sprite = resolvedPanelSprite;
            panelImage.type = panelImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            panelImage.color = panelColor;

            CreateGlow(panelRoot, "TopGlow", panelGlowTopColor, new Vector2(136f, 90f), new Vector2(0f, 58f));
            CreateGlow(panelRoot, "BottomGlow", panelGlowBottomColor, new Vector2(152f, 78f), new Vector2(0f, -58f));

            previousModuleButton = CreateButton(panelRoot, "PreviousModuleButton", "\u2190", "tooltip_prev_module", new Vector2(-buttonSpacing, 0f));
            nextModuleButton = CreateButton(panelRoot, "NextModuleButton", "\u2192", "tooltip_next_module", new Vector2(buttonSpacing, 0f));
            previousVariantButton = CreateButton(panelRoot, "PreviousVariantButton", "\u2191", "tooltip_prev_variant", new Vector2(0f, buttonSpacing));
            nextVariantButton = CreateButton(panelRoot, "NextVariantButton", "\u2193", "tooltip_next_variant", new Vector2(0f, -buttonSpacing));
            moduleSelectorButton = EnsureSelectorButton(canvas.transform, "ModuleSelector");
            variantSelectorButton = EnsureSelectorButton(canvas.transform, "VariantSelector");

            EnsureTooltip(panelRoot);
        }

        private static Transform FindDescendant(Transform root, string targetName)
        {
            if (root == null)
                return null;

            if (root.name == targetName)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDescendant(root.GetChild(i), targetName);
                if (match != null)
                    return match;
            }

            return null;
        }

        private void BindButtons()
        {
            AddListener(previousModuleButton, PreviousModulePressed);
            AddListener(nextModuleButton, NextModulePressed);
            AddListener(previousVariantButton, PreviousVariantPressed);
            AddListener(nextVariantButton, NextVariantPressed);
            AddListener(moduleSelectorButton, NextModulePressed);
            AddListener(variantSelectorButton, NextVariantPressed);
        }

        private void Refresh()
        {
            bool canSwitchModules = manager != null && manager.modules != null && manager.modules.Length > 1;
            bool canSwitchVariants = manager != null && manager.CurrentModule != null && manager.CurrentModule.variants != null && manager.CurrentModule.variants.Length > 1;

            SetButtonState(previousModuleButton, previousModuleLabel, canSwitchModules);
            SetButtonState(nextModuleButton, nextModuleLabel, canSwitchModules);
            SetButtonState(previousVariantButton, previousVariantLabel, canSwitchVariants);
            SetButtonState(nextVariantButton, nextVariantLabel, canSwitchVariants);
            SetButtonState(moduleSelectorButton, null, canSwitchModules);
            SetButtonState(variantSelectorButton, null, canSwitchVariants);
        }

        private void CacheBaseStyles()
        {
            CacheBaseStyle(previousModuleButton, previousModuleLabel);
            CacheBaseStyle(nextModuleButton, nextModuleLabel);
            CacheBaseStyle(previousVariantButton, previousVariantLabel);
            CacheBaseStyle(nextVariantButton, nextVariantLabel);
            CacheBaseStyle(moduleSelectorButton, null);
            CacheBaseStyle(variantSelectorButton, null);
        }

        private void CacheBaseStyle(Button button, TextMeshProUGUI label)
        {
            if (button != null && !buttonBaseColors.ContainsKey(button))
            {
                Graphic targetGraphic = button.targetGraphic;
                buttonBaseColors[button] = targetGraphic != null ? targetGraphic.color : buttonColor;
            }

            if (label != null && !labelBaseColors.ContainsKey(label))
                labelBaseColors[label] = label.color;
        }

        private Button CreateButton(Transform parent, string name, string glyph, string hintKey, Vector2 position)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = buttonSize;
            rectTransform.anchoredPosition = position;

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = resolvedButtonSprite;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = buttonColor;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = buttonHoverColor;
            colors.pressedColor = buttonPressedColor;
            colors.selectedColor = buttonHoverColor;
            colors.disabledColor = buttonDisabledColor;
            colors.fadeDuration = buttonFadeDuration;
            button.colors = colors;

            CreateGlow(rectTransform, "Glow", buttonGlowColor, buttonSize * 0.92f, new Vector2(0f, 6f));

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = glyph;
            label.fontSize = 28f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = iconColor;
            label.raycastTarget = false;

            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;

            AddHoverEvents(button, hintKey);
            return button;
        }

        private static Button FindButton(RectTransform root, string name, ref TextMeshProUGUI label)
        {
            if (root == null)
                return null;

            Transform buttonTransform = root.Find(name);
            if (buttonTransform == null)
                return null;

            Button button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                Transform labelTransform = buttonTransform.Find("Label");
                if (labelTransform != null)
                    label = labelTransform.GetComponent<TextMeshProUGUI>();
            }

            return button;
        }

        private Button EnsureSelectorButton(Transform root, string name)
        {
            Transform target = FindDescendant(root, name);
            if (target == null)
                return null;

            Image image = target.GetComponent<Image>();
            if (image == null)
                image = target.gameObject.AddComponent<Image>();

            image.raycastTarget = true;
            Button button = target.GetComponent<Button>();
            if (button == null)
                button = target.gameObject.AddComponent<Button>();

            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(buttonHoverColor.r, buttonHoverColor.g, buttonHoverColor.b, image.color.a);
            colors.pressedColor = new Color(buttonPressedColor.r, buttonPressedColor.g, buttonPressedColor.b, Mathf.Max(0.92f, image.color.a));
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = new Color(image.color.r, image.color.g, image.color.b, 0.42f);
            colors.fadeDuration = buttonFadeDuration;
            button.colors = colors;
            return button;
        }

        private void EnsureTooltip(RectTransform parent)
        {
            if (tooltipRoot != null && tooltipLabel != null)
                return;

            Transform existing = parent.Find("Tooltip");
            if (existing != null)
            {
                tooltipRoot = existing.GetComponent<RectTransform>();
                Transform existingLabel = existing.Find("Label");
                if (existingLabel != null)
                    tooltipLabel = existingLabel.GetComponent<TextMeshProUGUI>();

                SetTooltipVisible(false);
                return;
            }

            GameObject tooltipObject = new GameObject("Tooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tooltipObject.transform.SetParent(parent, false);

            tooltipRoot = tooltipObject.GetComponent<RectTransform>();
            tooltipRoot.anchorMin = new Vector2(0.5f, 1f);
            tooltipRoot.anchorMax = new Vector2(0.5f, 1f);
            tooltipRoot.pivot = new Vector2(0.5f, 0f);
            tooltipRoot.sizeDelta = new Vector2(150f, 34f);
            tooltipRoot.anchoredPosition = new Vector2(0f, 18f);

            Image tooltipImage = tooltipObject.GetComponent<Image>();
            tooltipImage.sprite = resolvedPanelSprite;
            tooltipImage.type = tooltipImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            tooltipImage.color = tooltipColor;
            tooltipImage.raycastTarget = false;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(tooltipObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 0f);
            labelRect.offsetMax = new Vector2(-10f, 0f);

            tooltipLabel = labelObject.GetComponent<TextMeshProUGUI>();
            tooltipLabel.fontSize = 16f;
            tooltipLabel.alignment = TextAlignmentOptions.Center;
            tooltipLabel.color = tooltipTextColor;
            tooltipLabel.raycastTarget = false;

            if (TMP_Settings.defaultFontAsset != null)
                tooltipLabel.font = TMP_Settings.defaultFontAsset;

            SetTooltipVisible(false);
        }

        private void AddHoverEvents(Button button, string hintKey)
        {
            if (button == null)
                return;

            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();
            AddEventTrigger(trigger, EventTriggerType.PointerEnter, delegate { ShowTooltip(ShowcaseLocalization.GetText(hintKey)); });
            AddEventTrigger(trigger, EventTriggerType.PointerExit, delegate { HideTooltip(); });
        }

        private static void AddEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        private void ShowTooltip(string text)
        {
            if (tooltipRoot == null || tooltipLabel == null)
                return;

            tooltipLabel.text = text;
            SetTooltipVisible(true);
        }

        private void HideTooltip()
        {
            SetTooltipVisible(false);
        }

        private void SetTooltipVisible(bool visible)
        {
            if (tooltipRoot == null)
                return;

            tooltipRoot.gameObject.SetActive(visible);
        }

        private void SetButtonState(Button button, TextMeshProUGUI label, bool enabled)
        {
            if (button == null)
                return;

            button.interactable = enabled;
            Color baseButtonColor = buttonBaseColors.TryGetValue(button, out Color cachedButtonColor) ? cachedButtonColor : buttonColor;

            ColorBlock colors = button.colors;
            colors.normalColor = enabled ? baseButtonColor : buttonDisabledColor;
            colors.highlightedColor = enabled ? buttonHoverColor : buttonDisabledColor;
            colors.pressedColor = enabled ? buttonPressedColor : buttonDisabledColor;
            colors.selectedColor = enabled ? baseButtonColor : buttonDisabledColor;
            colors.disabledColor = buttonDisabledColor;
            colors.fadeDuration = buttonFadeDuration;
            button.colors = colors;

            float scale = 1f;
            if (button == feedbackButton && feedbackTimer > 0f)
                scale = pressedScale;

            button.transform.localScale = Vector3.one * scale;

            if (label != null)
            {
                Color baseLabelColor = labelBaseColors.TryGetValue(label, out Color cachedLabelColor) ? cachedLabelColor : iconColor;
                label.color = enabled ? baseLabelColor : new Color(baseLabelColor.r, baseLabelColor.g, baseLabelColor.b, 0.45f);
            }
        }

        private void PlayFeedback(Button button)
        {
            if (button == null)
                return;

            feedbackButton = button;
            feedbackTimer = 0.12f;
            button.transform.localScale = Vector3.one * pressedScale;
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void RemoveListeners(Button button)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }

        private Image CreateGlow(Transform parent, string name, Color color, Vector2 size, Vector2 position)
        {
            GameObject glowObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowObject.transform.SetParent(parent, false);
            glowObject.transform.SetAsFirstSibling();

            RectTransform glowRect = glowObject.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.pivot = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = size;
            glowRect.anchoredPosition = position;

            Image glowImage = glowObject.GetComponent<Image>();
            glowImage.sprite = resolvedButtonSprite;
            glowImage.type = glowImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            glowImage.color = color;
            glowImage.raycastTarget = false;
            return glowImage;
        }

        private void ApplyRuntimeDefaults()
        {
            if (panelSize.x <= 0f || panelSize.y <= 0f)
                panelSize = DefaultPanelSize;

            if (panelOffset == Vector2.zero)
                panelOffset = DefaultPanelOffset;

            if (buttonSize.x <= 0f || buttonSize.y <= 0f)
                buttonSize = DefaultButtonSize;

            if (buttonSpacing <= 0f)
                buttonSpacing = DefaultButtonSpacing;

            if (pressedScale <= 0f)
                pressedScale = DefaultPressedScale;

            if (buttonFadeDuration <= 0f)
                buttonFadeDuration = DefaultButtonFadeDuration;

            if (IsUnset(panelColor))
                panelColor = DefaultPanelColor;

            if (IsUnset(panelGlowTopColor))
                panelGlowTopColor = DefaultPanelGlowTopColor;

            if (IsUnset(panelGlowBottomColor))
                panelGlowBottomColor = DefaultPanelGlowBottomColor;

            if (IsUnset(buttonColor))
                buttonColor = DefaultButtonColor;

            if (IsUnset(buttonHoverColor))
                buttonHoverColor = DefaultButtonHoverColor;

            if (IsUnset(buttonPressedColor))
                buttonPressedColor = DefaultButtonPressedColor;

            if (IsUnset(buttonDisabledColor))
                buttonDisabledColor = DefaultButtonDisabledColor;

            if (IsUnset(buttonGlowColor))
                buttonGlowColor = DefaultButtonGlowColor;

            if (IsUnset(iconColor))
                iconColor = DefaultIconColor;

            if (IsUnset(tooltipColor))
                tooltipColor = DefaultTooltipColor;

            if (IsUnset(tooltipTextColor))
                tooltipTextColor = DefaultTooltipTextColor;
        }

        private void ResolveSprites()
        {
            resolvedPanelSprite = ResolveSprite(panelSprite);
            resolvedButtonSprite = ResolveSprite(buttonSprite != null ? buttonSprite : panelSprite);
        }

        private static Sprite ResolveSprite(Sprite preferredSprite)
        {
            if (preferredSprite != null)
                return preferredSprite;

#if UNITY_EDITOR
            Sprite assetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiSpritePath);
            if (assetSprite != null)
                return assetSprite;
#endif
            return GetFallbackSprite();
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackSprite == null)
            {
                Texture2D texture = Texture2D.whiteTexture;
                fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
                fallbackSprite.name = "ModuleNavigationFallbackSprite";
            }

            return fallbackSprite;
        }

        private static bool IsUnset(Color color)
        {
            return color == default;
        }

        private static Canvas FindCanvasByChild(Transform root, string childName)
        {
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (FindDescendant(canvases[i].transform, childName) != null)
                    return canvases[i];
            }

            return null;
        }

        private static Sprite fallbackSprite;
    }
}
