using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    public sealed class ModuleNavigationControls : MonoBehaviour
    {
        private const float ButtonFeedbackDuration = 0.12f;

        [Header("References")]
        [SerializeField, FormerlySerializedAs("panelSprite")] private Sprite _panelSprite;
        [SerializeField, FormerlySerializedAs("buttonSprite")] private Sprite _buttonSprite;

        [Header("Layout")]
        [SerializeField, FormerlySerializedAs("panelSize")] private Vector2 _panelSize = new(212f, 212f);
        [SerializeField, FormerlySerializedAs("panelOffset")] private Vector2 _panelOffset = new(0f, 132f);
        [SerializeField, FormerlySerializedAs("buttonSize")] private Vector2 _buttonSize = new(60f, 60f);

        [Header("Colors")]
        [SerializeField, FormerlySerializedAs("panelColor")] private Color _panelColor = default;
        [SerializeField, FormerlySerializedAs("panelGlowTopColor")] private Color _panelGlowTopColor = default;
        [SerializeField, FormerlySerializedAs("panelGlowBottomColor")] private Color _panelGlowBottomColor = default;
        [SerializeField, FormerlySerializedAs("buttonColor")] private Color _buttonColor = default;
        [SerializeField, FormerlySerializedAs("buttonHoverColor")] private Color _buttonHoverColor = default;
        [SerializeField, FormerlySerializedAs("buttonPressedColor")] private Color _buttonPressedColor = default;
        [SerializeField, FormerlySerializedAs("buttonDisabledColor")] private Color _buttonDisabledColor = default;
        [SerializeField, FormerlySerializedAs("buttonGlowColor")] private Color _buttonGlowColor = default;
        [SerializeField, FormerlySerializedAs("iconColor")] private Color _iconColor = default;
        [SerializeField, FormerlySerializedAs("tooltipColor")] private Color _tooltipColor = default;
        [SerializeField, FormerlySerializedAs("tooltipTextColor")] private Color _tooltipTextColor = default;

        [Header("Feedback")]
        [SerializeField, FormerlySerializedAs("pressedScale")] private float _pressedScale = 0.95f;
        [SerializeField, FormerlySerializedAs("buttonFadeDuration")] private float _buttonFadeDuration = 0.08f;

        [Header("Scene References")]
        [SerializeField] private Button _previousModuleButton;
        [SerializeField] private TextMeshProUGUI _previousModuleLabel;
        [SerializeField] private Button _nextModuleButton;
        [SerializeField] private TextMeshProUGUI _nextModuleLabel;
        [SerializeField] private Button _previousVariantButton;
        [SerializeField] private TextMeshProUGUI _previousVariantLabel;
        [SerializeField] private Button _nextVariantButton;
        [SerializeField] private TextMeshProUGUI _nextVariantLabel;
        [SerializeField] private Button _moduleSelectorButton;
        [SerializeField] private Button _variantSelectorButton;

        private readonly Dictionary<Button, Color> _buttonBaseColors = new();
        private readonly Dictionary<TextMeshProUGUI, Color> _labelBaseColors = new();

        private bool _canSwitchCategories;
        private bool _canSwitchModules;
        private bool _canSwitchVariants;
        private Button _feedbackButton;
        private float _feedbackTimer;
        private bool _isInitialized;

        public event Action PreviousModuleRequested;
        public event Action NextModuleRequested;
        public event Action NextCategoryRequested;
        public event Action PreviousVariantRequested;
        public event Action NextVariantRequested;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            TickFeedback(Time.unscaledDeltaTime);
            RefreshView();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            ApplyRuntimeDefaults();
            ValidateReferences();
            CacheBaseStyles();
            BindButtons();

            _isInitialized = true;
            _canSwitchModules = false;
            _canSwitchCategories = false;
            _canSwitchVariants = false;
            RefreshView();
        }

        private void ValidateReferences()
        {
            ValidateButtonReference(_previousModuleButton, nameof(_previousModuleButton));
            ValidateButtonReference(_nextModuleButton, nameof(_nextModuleButton));
            ValidateButtonReference(_previousVariantButton, nameof(_previousVariantButton));
            ValidateButtonReference(_nextVariantButton, nameof(_nextVariantButton));
            ValidateButtonReference(_moduleSelectorButton, nameof(_moduleSelectorButton));
            ValidateButtonReference(_variantSelectorButton, nameof(_variantSelectorButton));
            ValidateLabelReference(_previousVariantLabel, nameof(_previousVariantLabel));
            ValidateLabelReference(_nextVariantLabel, nameof(_nextVariantLabel));

            if (_pressedScale <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(_pressedScale));
            }

            if (_buttonFadeDuration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(_buttonFadeDuration));
            }
        }

        private void ApplyRuntimeDefaults()
        {
            if (_panelColor == default)
            {
                _panelColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.84f);
            }

            if (_panelGlowTopColor == default)
            {
                _panelGlowTopColor = ShowcasePalette.AccentSoft(0.18f);
            }

            if (_panelGlowBottomColor == default)
            {
                _panelGlowBottomColor = ShowcasePalette.AccentSoft(0.12f);
            }

            if (_buttonColor == default)
            {
                _buttonColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelSoft, 0.98f);
            }

            if (_buttonHoverColor == default)
            {
                _buttonHoverColor = ShowcasePalette.PanelHover;
            }

            if (_buttonPressedColor == default)
            {
                _buttonPressedColor = ShowcasePalette.AccentStrong;
            }

            if (_buttonDisabledColor == default)
            {
                _buttonDisabledColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelSoft, 0.42f);
            }

            if (_buttonGlowColor == default)
            {
                _buttonGlowColor = ShowcasePalette.AccentSoft(0.14f);
            }

            if (_iconColor == default)
            {
                _iconColor = ShowcasePalette.TextPrimary;
            }

            if (_tooltipColor == default)
            {
                _tooltipColor = ShowcasePalette.WithAlpha(ShowcasePalette.PanelMain, 0.96f);
            }

            if (_tooltipTextColor == default)
            {
                _tooltipTextColor = ShowcasePalette.TextPrimary;
            }
        }

        private void TickFeedback(float deltaTime)
        {
            if (_feedbackTimer <= 0f)
            {
                return;
            }

            _feedbackTimer -= deltaTime;
            if (_feedbackTimer <= 0f)
            {
                _feedbackButton = null;
            }
        }

        private void BindButtons()
        {
            AddListener(_previousModuleButton, PreviousModulePressed);
            AddListener(_nextModuleButton, NextModulePressed);
            AddListener(_previousVariantButton, PreviousVariantPressed);
            AddListener(_nextVariantButton, NextVariantPressed);
            AddListener(_moduleSelectorButton, NextCategoryPressed);
            AddListener(_variantSelectorButton, NextVariantPressed);
        }

        private void UnbindButtons()
        {
            RemoveListener(_previousModuleButton, PreviousModulePressed);
            RemoveListener(_nextModuleButton, NextModulePressed);
            RemoveListener(_previousVariantButton, PreviousVariantPressed);
            RemoveListener(_nextVariantButton, NextVariantPressed);
            RemoveListener(_moduleSelectorButton, NextCategoryPressed);
            RemoveListener(_variantSelectorButton, NextVariantPressed);
        }

        private void CacheBaseStyles()
        {
            _buttonBaseColors.Clear();
            _labelBaseColors.Clear();

            CacheBaseStyle(_previousModuleButton, _previousModuleLabel);
            CacheBaseStyle(_nextModuleButton, _nextModuleLabel);
            CacheBaseStyle(_previousVariantButton, _previousVariantLabel);
            CacheBaseStyle(_nextVariantButton, _nextVariantLabel);
            CacheBaseStyle(_moduleSelectorButton, null);
            CacheBaseStyle(_variantSelectorButton, null);
        }

        private void CacheBaseStyle(Button button, TextMeshProUGUI label)
        {
            Graphic targetGraphic = button.targetGraphic;
            _buttonBaseColors[button] = targetGraphic != null ? targetGraphic.color : _buttonColor;

            if (label != null)
            {
                _labelBaseColors[label] = label.color;
            }
        }

        private void RefreshView()
        {
            EnsureInitialized();
            EnsureBaseStylesCached();

            SetButtonState(_previousModuleButton, _previousModuleLabel, _canSwitchModules);
            SetButtonState(_nextModuleButton, _nextModuleLabel, _canSwitchModules);
            SetButtonState(_previousVariantButton, _previousVariantLabel, _canSwitchVariants);
            SetButtonState(_nextVariantButton, _nextVariantLabel, _canSwitchVariants);
            SetButtonState(_moduleSelectorButton, null, _canSwitchCategories);
            SetButtonState(_variantSelectorButton, null, _canSwitchVariants);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        private void EnsureBaseStylesCached()
        {
            if (_buttonBaseColors.Count == 0)
            {
                CacheBaseStyles();
            }
        }

        private void SetButtonState(Button button, TextMeshProUGUI label, bool isEnabled)
        {
            button.interactable = isEnabled;

            Color baseButtonColor = _buttonBaseColors[button];
            ColorBlock colors = button.colors;
            colors.normalColor = isEnabled ? baseButtonColor : _buttonDisabledColor;
            colors.highlightedColor = isEnabled ? _buttonHoverColor : _buttonDisabledColor;
            colors.pressedColor = isEnabled ? _buttonPressedColor : _buttonDisabledColor;
            colors.selectedColor = isEnabled ? baseButtonColor : _buttonDisabledColor;
            colors.disabledColor = _buttonDisabledColor;
            colors.fadeDuration = _buttonFadeDuration;
            button.colors = colors;

            float scale = button == _feedbackButton && _feedbackTimer > 0f ? _pressedScale : 1f;
            button.transform.localScale = Vector3.one * scale;

            if (label == null)
            {
                return;
            }

            Color baseLabelColor = _labelBaseColors[label];
            label.color = isEnabled
                ? baseLabelColor
                : new Color(baseLabelColor.r, baseLabelColor.g, baseLabelColor.b, 0.45f);
        }

        private void PlayFeedback(Button button)
        {
            _feedbackButton = button;
            _feedbackTimer = ButtonFeedbackDuration;
            button.transform.localScale = Vector3.one * _pressedScale;
        }

        private static void ValidateButtonReference(Button button, string fieldName)
        {
            if (button == null)
            {
                throw new InvalidOperationException($"{nameof(ModuleNavigationControls)} requires {fieldName}.");
            }
        }

        private static void ValidateLabelReference(TextMeshProUGUI label, string fieldName)
        {
            if (label == null)
            {
                throw new InvalidOperationException($"{nameof(ModuleNavigationControls)} requires {fieldName}.");
            }
        }

        private static void AddListener(Button button, UnityAction action)
        {
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static void RemoveListener(Button button, UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
        }

        public void PreviousModulePressed()
        {
            PreviousModuleRequested?.Invoke();
            PlayFeedback(_previousModuleButton);
        }

        public void NextModulePressed()
        {
            NextModuleRequested?.Invoke();
            PlayFeedback(_nextModuleButton);
        }

        public void NextCategoryPressed()
        {
            NextCategoryRequested?.Invoke();
            PlayFeedback(_moduleSelectorButton);
        }

        public void PreviousVariantPressed()
        {
            PreviousVariantRequested?.Invoke();
            PlayFeedback(_previousVariantButton);
        }

        public void NextVariantPressed()
        {
            NextVariantRequested?.Invoke();
            PlayFeedback(_nextVariantButton);
        }

        public void ShowNavigationState(bool canSwitchModules, bool canSwitchCategories, bool canSwitchVariants)
        {
            EnsureInitialized();
            _canSwitchModules = canSwitchModules;
            _canSwitchCategories = canSwitchCategories;
            _canSwitchVariants = canSwitchVariants;
            RefreshView();
        }
    }
}
