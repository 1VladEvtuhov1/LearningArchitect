using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    public sealed class ModuleNavigationControls : MonoBehaviour
    {
        public event Action PreviousModuleRequested;
        public event Action NextModuleRequested;
        public event Action NextCategoryRequested;
        public event Action PreviousVariantRequested;
        public event Action NextVariantRequested;

        [Header("References")]
        public Sprite panelSprite;
        public Sprite buttonSprite;

        [Header("Layout")]
        public Vector2 panelSize = new(212f, 212f);
        public Vector2 panelOffset = new(0f, 132f);
        public Vector2 buttonSize = new(60f, 60f);
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

        private readonly Dictionary<Button, Color> buttonBaseColors = new();
        private readonly Dictionary<TextMeshProUGUI, Color> labelBaseColors = new();

        private bool canSwitchCategories;
        private bool canSwitchModules;
        private bool canSwitchVariants;
        private Button feedbackButton;
        private float feedbackTimer;
        private bool isInitialized;
        private Button moduleSelectorButton;
        private RectTransform panelRoot;
        private Button nextModuleButton;
        private TextMeshProUGUI nextModuleLabel;
        private Button nextVariantButton;
        private TextMeshProUGUI nextVariantLabel;
        private Button previousModuleButton;
        private TextMeshProUGUI previousModuleLabel;
        private Button previousVariantButton;
        private TextMeshProUGUI previousVariantLabel;
        private Button variantSelectorButton;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.unscaledDeltaTime;
                if (feedbackTimer <= 0f)
                {
                    feedbackButton = null;
                }
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
            PreviousModuleRequested?.Invoke();
            PlayFeedback(previousModuleButton);
        }

        public void NextModulePressed()
        {
            NextModuleRequested?.Invoke();
            PlayFeedback(nextModuleButton);
        }

        public void NextCategoryPressed()
        {
            NextCategoryRequested?.Invoke();
            PlayFeedback(moduleSelectorButton);
        }

        public void PreviousVariantPressed()
        {
            PreviousVariantRequested?.Invoke();
            PlayFeedback(previousVariantButton);
        }

        public void NextVariantPressed()
        {
            NextVariantRequested?.Invoke();
            PlayFeedback(nextVariantButton);
        }

        public void ShowNavigationState(bool canSwitchModules, bool canSwitchCategories, bool canSwitchVariants)
        {
            EnsureInitialized();
            this.canSwitchModules = canSwitchModules;
            this.canSwitchCategories = canSwitchCategories;
            this.canSwitchVariants = canSwitchVariants;
            Refresh();
        }

        private void EnsureInitialized()
        {
            if (isInitialized)
            {
                return;
            }

            ApplyRuntimeDefaults();
            ResolveUi();
            CacheBaseStyles();
            BindButtons();
            isInitialized = true;
            ShowNavigationState(false, false, false);
        }

        private void ApplyRuntimeDefaults()
        {
            if (pressedScale <= 0f)
            {
                pressedScale = 0.95f;
            }

            if (buttonFadeDuration <= 0f)
            {
                buttonFadeDuration = 0.08f;
            }

            if (panelColor == default)
            {
                panelColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.84f);
            }

            if (panelGlowTopColor == default)
            {
                panelGlowTopColor = ShowcasePalette.AccentSoft(0.18f);
            }

            if (panelGlowBottomColor == default)
            {
                panelGlowBottomColor = ShowcasePalette.AccentSoft(0.12f);
            }

            if (buttonColor == default)
            {
                buttonColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelSoft, 0.98f);
            }

            if (buttonHoverColor == default)
            {
                buttonHoverColor = ShowcasePalette.PanelHover;
            }

            if (buttonPressedColor == default)
            {
                buttonPressedColor = ShowcasePalette.AccentStrong;
            }

            if (buttonDisabledColor == default)
            {
                buttonDisabledColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelSoft, 0.42f);
            }

            if (buttonGlowColor == default)
            {
                buttonGlowColor = ShowcasePalette.AccentSoft(0.14f);
            }

            if (iconColor == default)
            {
                iconColor = ShowcasePalette.TextPrimary;
            }

            if (tooltipColor == default)
            {
                tooltipColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.96f);
            }

            if (tooltipTextColor == default)
            {
                tooltipTextColor = ShowcasePalette.TextPrimary;
            }
        }

        private void ResolveUi()
        {
            Canvas canvas = FindCanvasByChild(transform, "Container - Root");
            if (canvas == null)
            {
                throw new InvalidOperationException($"{nameof(ModuleNavigationControls)} requires a canvas containing Container - Root.");
            }

            Transform existing = FindDescendant(canvas.transform, "Container - NavigationControls");
            if (existing == null)
            {
                throw new InvalidOperationException($"{nameof(ModuleNavigationControls)} requires Container - NavigationControls.");
            }

            panelRoot = existing.GetComponent<RectTransform>();
            if (panelRoot == null)
            {
                throw new InvalidOperationException("Container - NavigationControls requires RectTransform.");
            }

            previousModuleButton = FindButton(panelRoot, "Button - PrevModule", out previousModuleLabel);
            nextModuleButton = FindButton(panelRoot, "Button - NextModule", out nextModuleLabel);
            previousVariantButton = FindButton(canvas.transform, "Button - PrevVariant", out previousVariantLabel);
            nextVariantButton = FindButton(canvas.transform, "Button - NextVariant", out nextVariantLabel);
            moduleSelectorButton = FindButton(canvas.transform, "Button - ModuleSelector", out _);
            variantSelectorButton = FindButton(canvas.transform, "Button - VariantSelector", out _);
        }

        private void BindButtons()
        {
            AddListener(previousModuleButton, PreviousModulePressed);
            AddListener(nextModuleButton, NextModulePressed);
            AddListener(previousVariantButton, PreviousVariantPressed);
            AddListener(nextVariantButton, NextVariantPressed);
            AddListener(moduleSelectorButton, NextCategoryPressed);
            AddListener(variantSelectorButton, NextVariantPressed);
        }

        private void Refresh()
        {
            EnsureBaseStylesCached();
            SetButtonState(previousModuleButton, previousModuleLabel, canSwitchModules);
            SetButtonState(nextModuleButton, nextModuleLabel, canSwitchModules);
            SetButtonState(previousVariantButton, previousVariantLabel, canSwitchVariants);
            SetButtonState(nextVariantButton, nextVariantLabel, canSwitchVariants);
            SetButtonState(moduleSelectorButton, null, canSwitchCategories);
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

        private void EnsureBaseStylesCached()
        {
            if (buttonBaseColors.Count != 0)
            {
                return;
            }

            CacheBaseStyles();
        }

        private void CacheBaseStyle(Button button, TextMeshProUGUI label)
        {
            Graphic targetGraphic = button.targetGraphic;
            buttonBaseColors[button] = targetGraphic != null ? targetGraphic.color : buttonColor;

            if (label != null)
            {
                labelBaseColors[label] = label.color;
            }
        }

        private void SetButtonState(Button button, TextMeshProUGUI label, bool enabled)
        {
            button.interactable = enabled;

            Color baseButtonColor = buttonBaseColors[button];
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
            {
                scale = pressedScale;
            }

            button.transform.localScale = Vector3.one * scale;

            if (label != null)
            {
                Color baseLabelColor = labelBaseColors[label];
                label.color = enabled ? baseLabelColor : new Color(baseLabelColor.r, baseLabelColor.g, baseLabelColor.b, 0.45f);
            }
        }

        private void PlayFeedback(Button button)
        {
            feedbackButton = button;
            feedbackTimer = 0.12f;
            button.transform.localScale = Vector3.one * pressedScale;
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void RemoveListeners(Button button)
        {
            button.onClick.RemoveAllListeners();
        }

        private static Button FindButton(Transform root, string name, out TextMeshProUGUI label)
        {
            Transform buttonTransform = FindDescendant(root, name);
            if (buttonTransform == null)
            {
                throw new InvalidOperationException($"{nameof(ModuleNavigationControls)} requires {name}.");
            }

            Button button = buttonTransform.GetComponent<Button>();
            if (button == null)
            {
                throw new InvalidOperationException($"{name} requires {nameof(Button)}.");
            }

            Transform labelTransform = buttonTransform.Find("Label");
            label = labelTransform == null ? null : labelTransform.GetComponent<TextMeshProUGUI>();
            return button;
        }

        private static Canvas FindCanvasByChild(Transform root, string childName)
        {
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (FindDescendant(canvases[i].transform, childName) != null)
                {
                    return canvases[i];
                }
            }

            return null;
        }

        private static Transform FindDescendant(Transform root, string targetName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == targetName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDescendant(root.GetChild(i), targetName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
