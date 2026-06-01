using System;
using System.Collections.Generic;
using System.Globalization;
using LearningArchitect.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StressTestControlsView))]
    public sealed class StressTestControls : MonoBehaviour
    {
        private const float FeedbackDuration = 0.18f;
        private static readonly int[] DefaultStressPresets = ShowcaseStressSpawn.DefaultPresets;

        [SerializeField] private Color _activeColor = default;
        [SerializeField] private Color _activeBackgroundColor = default;
        [SerializeField] private Color _inactiveColor = default;
        [SerializeField] private Color _inactiveLabelColor = default;
        [SerializeField] private Color _hoverBackgroundColor = default;
        [SerializeField] private Color _pressedBackgroundColor = default;
        [SerializeField] private float _activeScale = 1.04f;
        [SerializeField] private float _clickScale = 1.08f;

        [SerializeField] private StressTestControlsView _view;

        private readonly List<UnityAction> _buttonListeners = new(4);

        private int[] _activePresets = Array.Empty<int>();
        private string[] _activePresetLabels;
        private int _displayedStressLevel = DefaultStressPresets[0];
        private Button _feedbackButton;
        private float _feedbackTimer;
        private bool _isInitialized;

        public event Action<int> StressRequested;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>EditMode tests: runs the same setup as <see cref="Awake"/> without relying on reflection.</summary>
        internal void RunInitializeForEditModeTests()
        {
            Initialize();
        }

        private void Update()
        {
            Tick(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;

            ApplyPaletteDefaults();
            ValidateReferences();

            _activePresets = (int[])DefaultStressPresets.Clone();
            BindButtons();
            _isInitialized = true;

            ApplyPresetLabels();
            RefreshView();
        }

        private void ValidateReferences()
        {
            _view = _view != null ? _view : GetComponent<StressTestControlsView>();
            if (_view == null)
                throw new InvalidOperationException($"{nameof(StressTestControlsView)} is required.");

            _view.ValidateReferences();

            if (_activeScale <= 0f)
                throw new ArgumentOutOfRangeException(nameof(_activeScale));

            if (_clickScale <= 0f)
                throw new ArgumentOutOfRangeException(nameof(_clickScale));
        }

        private void Tick(float deltaTime)
        {
            EnsureInitialized();

            if (WasPressed(KeyCode.Alpha1))
                TriggerPreset(0);

            if (WasPressed(KeyCode.Alpha2))
                TriggerPreset(1);

            if (WasPressed(KeyCode.Alpha3))
                TriggerPreset(2);

            if (_feedbackTimer > 0f)
            {
                _feedbackTimer -= deltaTime;
                if (_feedbackTimer <= 0f)
                    _feedbackButton = null;
            }

            Refresh();
        }

        private void ApplyPaletteDefaults()
        {
            if (_activeColor == default)
                _activeColor = ShowcasePalette.AccentMain;

            if (_activeBackgroundColor == default)
                _activeBackgroundColor = ShowcasePalette.PanelHover;

            if (_inactiveColor == default)
                _inactiveColor = ShowcasePalette.PanelSoft;

            if (_inactiveLabelColor == default)
                _inactiveLabelColor = ShowcasePalette.TextSecondary;

            if (_hoverBackgroundColor == default)
                _hoverBackgroundColor = ShowcasePalette.PanelHover;

            if (_pressedBackgroundColor == default)
                _pressedBackgroundColor = ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.22f);
        }

        private void Refresh()
        {
            EnsureInitialized();
            RefreshView();
        }

        private void RefreshView()
        {
            int visiblePresetCount = Mathf.Min(_activePresets.Length, _view.PresetCount);
            for (int i = 0; i < _view.PresetCount; i++)
            {
                bool isVisible = i < visiblePresetCount;
                _view.SetPresetVisible(i, isVisible);

                if (!isVisible)
                    continue;

                bool isActive = _displayedStressLevel == _activePresets[i];
                Button button = _view.GetButton(i);
                _view.SetPresetState(
                    i,
                    GetButtonScale(button, isActive),
                    isActive ? _activeBackgroundColor : _inactiveColor,
                    _hoverBackgroundColor,
                    _pressedBackgroundColor,
                    new Color(_inactiveColor.r, _inactiveColor.g, _inactiveColor.b, 0.42f),
                    isActive ? _activeColor : _inactiveLabelColor);
            }
        }

        private void BindButtons()
        {
            UnbindButtons();

            for (int i = 0; i < _view.PresetCount; i++)
            {
                int presetIndex = i;
                UnityAction listener = delegate { TriggerPreset(presetIndex); };
                _view.GetButton(i).onClick.AddListener(listener);
                _buttonListeners.Add(listener);
            }
        }

        private void UnbindButtons()
        {
            if (_view == null)
            {
                _buttonListeners.Clear();
                return;
            }

            int count = Mathf.Min(_view.PresetCount, _buttonListeners.Count);
            for (int i = 0; i < count; i++)
            {
                Button button = _view.GetButton(i);
                if (button != null)
                    button.onClick.RemoveListener(_buttonListeners[i]);
            }

            _buttonListeners.Clear();
        }

        private void ApplyPresetLabels()
        {
            for (int i = 0; i < _view.PresetCount; i++)
            {
                bool isVisible = i < _activePresets.Length;
                _view.SetPresetVisible(i, isVisible);

                if (!isVisible)
                    continue;

                _view.SetPresetLabel(i, GetPresetLabel(i));
            }
        }

        private void TriggerPreset(int presetIndex)
        {
            EnsureInitialized();

            if ((uint)presetIndex >= (uint)_activePresets.Length)
                return;

            SetStressLevel(_activePresets[presetIndex]);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                Initialize();

            if (!_isInitialized)
                throw new InvalidOperationException($"{nameof(StressTestControls)} failed to initialize.");
        }

        private Button GetButtonForCount(int count)
        {
            int buttonCount = Mathf.Min(_activePresets.Length, _view.PresetCount);
            for (int i = 0; i < buttonCount; i++)
            {
                if (_activePresets[i] == count)
                    return _view.GetButton(i);
            }

            throw new ArgumentOutOfRangeException(nameof(count), $"No stress button is configured for count {count}.");
        }

        private float GetButtonScale(Button button, bool active)
        {
            if (button != _feedbackButton)
                return active ? _activeScale : 1f;

            float pulse = _feedbackTimer <= 0f ? 0f : Mathf.Clamp01(_feedbackTimer / FeedbackDuration);
            return Mathf.Lerp(active ? _activeScale : 1f, _clickScale, pulse);
        }

        private string GetPresetLabel(int index)
        {
            if (_activePresetLabels != null)
                return _activePresetLabels[index];

            return FormatPresetCount(_activePresets[index]);
        }

        private static string FormatPresetCount(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
        }

        private int[] NormalizePresets(int[] presets)
        {
            if (presets == null || presets.Length == 0)
                return (int[])DefaultStressPresets.Clone();

            int[] normalized = new int[presets.Length];
            for (int i = 0; i < presets.Length; i++)
            {
                if (presets[i] < 1)
                    throw new ArgumentOutOfRangeException(nameof(presets), "Stress presets must be positive.");

                normalized[i] = presets[i];
            }

            return normalized;
        }

        public void SetStressLevel(int count)
        {
            EnsureInitialized();

            StressRequested?.Invoke(count);

            _feedbackButton = GetButtonForCount(count);
            _feedbackTimer = FeedbackDuration;
            Refresh();
        }

        public void ConfigurePresets(int[] presets, string[] presetLabels = null)
        {
            EnsureInitialized();

            _activePresets = NormalizePresets(presets);
            if (_activePresets.Length > _view.PresetCount)
            {
                throw new InvalidOperationException(
                    $"{nameof(StressTestControls)} found {_view.PresetCount} preset buttons, but {_activePresets.Length} presets were requested.");
            }

            _activePresetLabels = AreCustomLabelsValid(presetLabels, _activePresets.Length)
                ? (string[])presetLabels.Clone()
                : null;

            ApplyPresetLabels();
            Refresh();
        }

        public void ShowStressState(int stressLevel)
        {
            EnsureInitialized();

            _displayedStressLevel = stressLevel;
            Refresh();
        }

        private static bool AreCustomLabelsValid(string[] presetLabels, int expectedLength)
        {
            if (presetLabels == null || presetLabels.Length == 0 || presetLabels.Length != expectedLength)
                return false;

            for (int i = 0; i < presetLabels.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(presetLabels[i]))
                    return false;
            }

            return true;
        }

        private static bool WasPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                switch (key)
                {
                    case KeyCode.Alpha1:
                        return keyboard.digit1Key.wasPressedThisFrame;
                    case KeyCode.Alpha2:
                        return keyboard.digit2Key.wasPressedThisFrame;
                    case KeyCode.Alpha3:
                        return keyboard.digit3Key.wasPressedThisFrame;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }
    }
}
