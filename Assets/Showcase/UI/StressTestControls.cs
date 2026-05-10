using System;
using System.Collections.Generic;
using System.Globalization;
using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LearningArchitect.UI
{
    public sealed class StressTestControls : MonoBehaviour
    {
        private static readonly int[] DefaultStressPresets = { 1000, 5000, 10000 };
        private const float FeedbackDuration = 0.18f;

        private sealed class PresetButtonView
        {
            public PresetButtonView(Button button, TextMeshProUGUI label)
            {
                Button = button;
                Label = label;
            }

            public Button Button { get; }

            public TextMeshProUGUI Label { get; }
        }

        public event Action<int> StressRequested;

        [SerializeField] private Button oneKButton;
        [SerializeField] private Button fiveKButton;
        [SerializeField] private Button tenKButton;
        [SerializeField] private TextMeshProUGUI statusText;

        [SerializeField] private Color activeColor = default;
        [SerializeField] private Color activeBackgroundColor = default;
        [SerializeField] private Color inactiveColor = default;
        [SerializeField] private Color textColor = default;
        [SerializeField] private Color inactiveLabelColor = default;
        [SerializeField] private Color noteColor = default;
        [SerializeField] private Color hoverBackgroundColor = default;
        [SerializeField] private Color pressedBackgroundColor = default;
        [SerializeField] private float activeScale = 1.04f;
        [SerializeField] private float clickScale = 1.08f;

        private readonly List<PresetButtonView> presetButtons = new(4);
        private readonly List<UnityAction> buttonListeners = new(4);

        private int[] activePresets = Array.Empty<int>();
        private string[] activePresetLabels;
        private string contextNote = string.Empty;
        private int displayedStressLevel = DefaultStressPresets[0];
        private int displayedSimulationCount = DefaultStressPresets[0];
        private int displayedVisibleCount;
        private Button feedbackButton;
        private float feedbackTimer;
        private bool isInitialized;

        public Button OneKButton
        {
            get => oneKButton;
            set => oneKButton = value;
        }

        public Button FiveKButton
        {
            get => fiveKButton;
            set => fiveKButton = value;
        }

        public Button TenKButton
        {
            get => tenKButton;
            set => tenKButton = value;
        }

        public TextMeshProUGUI StatusText
        {
            get => statusText;
            set => statusText = value;
        }

        public Color InactiveLabelColor
        {
            get => inactiveLabelColor;
            set => inactiveLabelColor = value;
        }

        public Color HoverBackgroundColor
        {
            get => hoverBackgroundColor;
            set => hoverBackgroundColor = value;
        }

        public Color PressedBackgroundColor
        {
            get => pressedBackgroundColor;
            set => pressedBackgroundColor = value;
        }

        public Color ActiveColor
        {
            get => activeColor;
            set => activeColor = value;
        }

        public Color InactiveColor
        {
            get => inactiveColor;
            set => inactiveColor = value;
        }

        public Color TextColor
        {
            get => textColor;
            set => textColor = value;
        }

        public Color NoteColor
        {
            get => noteColor;
            set => noteColor = value;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            if (!EnsureInitialized())
                return;

            if (WasPressed(KeyCode.Alpha1))
                TriggerPreset(0);

            if (WasPressed(KeyCode.Alpha2))
                TriggerPreset(1);

            if (WasPressed(KeyCode.Alpha3))
                TriggerPreset(2);

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
            UnbindButtons();
        }

        public void SetStressLevel(int count)
        {
            if (!EnsureInitialized())
                return;

            StressRequested?.Invoke(count);

            feedbackButton = GetButtonForCount(count);
            feedbackTimer = FeedbackDuration;
            Refresh();
        }

        public void ConfigurePresets(int[] presets, string[] presetLabels = null)
        {
            if (!EnsureInitialized())
                return;

            activePresets = NormalizePresets(presets);
            if (activePresets.Length > presetButtons.Count)
            {
                throw new InvalidOperationException(
                    $"{nameof(StressTestControls)} found {presetButtons.Count} preset buttons, but {activePresets.Length} presets were requested.");
            }

            activePresetLabels = AreCustomLabelsValid(presetLabels, activePresets.Length)
                ? (string[])presetLabels.Clone()
                : null;

            ApplyPresetLabels();
            Refresh();
        }

        public void ShowStressState(int level, int activeCount, ShowcaseMetricsSnapshot metrics)
        {
            if (!EnsureInitialized())
                return;

            displayedStressLevel = level;
            displayedSimulationCount = metrics.HasSimulationCount ? metrics.SimulationCount : level;
            displayedVisibleCount = metrics.HasVisibleCount ? metrics.VisibleCount : activeCount;
            Refresh();
        }

        public void ConfigureModule(ModuleDefinitionSO module)
        {
            if (!EnsureInitialized())
                return;

            contextNote = ShowcaseLocalization.GetModuleWebGlPresetNote(module);
            Refresh();
        }

        private void Refresh()
        {
            if (!EnsureInitialized())
                return;

            int visiblePresetCount = Mathf.Min(activePresets.Length, presetButtons.Count);
            for (int i = 0; i < presetButtons.Count; i++)
            {
                PresetButtonView view = presetButtons[i];
                bool isVisible = i < visiblePresetCount;
                view.Button.gameObject.SetActive(isVisible);

                if (!isVisible)
                    continue;

                SetButtonState(view.Button, displayedStressLevel == activePresets[i], view.Label);
            }

            if (statusText == null)
                return;

            string prefix = feedbackTimer > 0f
                ? ShowcaseLocalization.GetText("load_selected")
                : ShowcaseLocalization.GetText("stress_load");
            statusText.text = BuildStatusText(prefix, displayedSimulationCount, displayedVisibleCount);
            statusText.color = Color.white;
        }

        private void SetButtonState(Button button, bool active, TextMeshProUGUI label)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = active ? activeBackgroundColor : inactiveColor;
            colors.highlightedColor = hoverBackgroundColor;
            colors.pressedColor = pressedBackgroundColor;
            colors.selectedColor = active ? activeBackgroundColor : inactiveColor;
            colors.disabledColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0.42f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.transform.localScale = Vector3.one * GetButtonScale(button, active);

            if (label != null)
                label.color = active ? activeColor : inactiveLabelColor;
        }

        private Button GetButtonForCount(int count)
        {
            int buttonCount = Mathf.Min(activePresets.Length, presetButtons.Count);
            for (int i = 0; i < buttonCount; i++)
            {
                if (activePresets[i] == count)
                    return presetButtons[i].Button;
            }

            throw new ArgumentOutOfRangeException(nameof(count), $"No stress button is configured for count {count}.");
        }

        private float GetButtonScale(Button button, bool active)
        {
            if (button != feedbackButton)
                return active ? activeScale : 1f;

            float pulse = feedbackTimer <= 0f ? 0f : Mathf.Clamp01(feedbackTimer / FeedbackDuration);
            return Mathf.Lerp(active ? activeScale : 1f, clickScale, pulse);
        }

        private void TriggerPreset(int presetIndex)
        {
            if (!EnsureInitialized())
                return;

            if ((uint)presetIndex >= (uint)activePresets.Length)
                return;

            SetStressLevel(activePresets[presetIndex]);
        }

        private void ApplyPresetLabels()
        {
            for (int i = 0; i < presetButtons.Count; i++)
            {
                bool isVisible = i < activePresets.Length;
                presetButtons[i].Button.gameObject.SetActive(isVisible);

                if (!isVisible || presetButtons[i].Label == null)
                    continue;

                presetButtons[i].Label.text = GetPresetLabel(i);
            }
        }

        private string GetPresetLabel(int index)
        {
            if (activePresetLabels != null)
                return activePresetLabels[index];

            return FormatCount(activePresets[index]);
        }

        private bool EnsureInitialized()
        {
            if (isInitialized)
                return true;

            ApplyPaletteDefaults();
            if (!TryResolveUi())
                return false;

            if (statusText != null)
            {
                statusText.richText = true;
                statusText.enableAutoSizing = false;
            }

            activePresets = (int[])DefaultStressPresets.Clone();
            BindButtons();
            isInitialized = true;
            ApplyPresetLabels();
            Refresh();
            return true;
        }

        private void ApplyPaletteDefaults()
        {
            if (activeColor == default)
                activeColor = ShowcasePalette.AccentMain;

            if (activeBackgroundColor == default)
                activeBackgroundColor = ShowcasePalette.PanelHover;

            if (inactiveColor == default)
                inactiveColor = ShowcasePalette.PanelSoft;

            if (textColor == default)
                textColor = ShowcasePalette.TextPrimary;

            if (inactiveLabelColor == default)
                inactiveLabelColor = ShowcasePalette.TextSecondary;

            if (noteColor == default)
                noteColor = ShowcasePalette.TextMuted;

            if (hoverBackgroundColor == default)
                hoverBackgroundColor = ShowcasePalette.PanelHover;

            if (pressedBackgroundColor == default)
                pressedBackgroundColor = ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.22f);
        }

        private bool TryResolveUi()
        {
            ClearResolvedButtons();

            if (TryResolveStructuredUi())
                return true;

            return TryResolveLegacyUi();
        }

        private bool TryResolveStructuredUi()
        {
            Canvas canvas = FindCanvasByChild(transform, "Container - StressControls");
            if (canvas == null)
                return false;

            Transform panel = FindDeep(canvas.transform, "Container - StressControls");
            if (panel == null)
                return false;

            statusText = statusText != null
                ? statusText
                : FindDeep(panel, "Text - StressSummary")?.GetComponent<TextMeshProUGUI>()
                  ?? FindDeep(panel, "StressStatusText")?.GetComponent<TextMeshProUGUI>()
                  ?? FindDeep(panel, "Text - StressStatus")?.GetComponent<TextMeshProUGUI>();

            Transform presetRoot = FindDeep(panel, "HorizontalLayout - StressPresets")
                                   ?? FindDeep(panel, "Layout - StressPresets")
                                   ?? FindDeep(panel, "HorizontalLayout - StressButtons")
                                   ?? FindDeep(panel, "StressButtonsRow");
            CollectButtons(presetRoot);

            if (presetButtons.Count == 0)
                CollectLegacyButtons();

            return statusText != null && presetButtons.Count > 0;
        }

        private bool TryResolveLegacyUi()
        {
            if (statusText == null)
                return false;

            CollectLegacyButtons();
            return presetButtons.Count > 0;
        }

        private void CollectLegacyButtons()
        {
            AppendButton(oneKButton);
            AppendButton(fiveKButton);
            AppendButton(tenKButton);
        }

        private void CollectButtons(Transform root)
        {
            if (root == null)
                return;

            for (int i = 0; i < root.childCount; i++)
            {
                Button button = root.GetChild(i).GetComponent<Button>();
                if (button == null)
                    continue;

                AppendButton(button);
            }
        }

        private void AppendButton(Button button)
        {
            if (button == null)
                return;

            for (int i = 0; i < presetButtons.Count; i++)
            {
                if (presetButtons[i].Button == button)
                    return;
            }

            TextMeshProUGUI label = FindButtonLabel(button.transform);
            if (label != null)
                label.richText = true;

            presetButtons.Add(new PresetButtonView(button, label));
        }

        private void BindButtons()
        {
            UnbindButtons();

            for (int i = 0; i < presetButtons.Count; i++)
            {
                int presetIndex = i;
                UnityAction listener = delegate { TriggerPreset(presetIndex); };
                presetButtons[i].Button.onClick.AddListener(listener);
                buttonListeners.Add(listener);
            }
        }

        private void UnbindButtons()
        {
            int count = Mathf.Min(presetButtons.Count, buttonListeners.Count);
            for (int i = 0; i < count; i++)
            {
                if (presetButtons[i].Button != null)
                    presetButtons[i].Button.onClick.RemoveListener(buttonListeners[i]);
            }

            buttonListeners.Clear();
        }

        private void ClearResolvedButtons()
        {
            UnbindButtons();
            presetButtons.Clear();
        }

        private static TextMeshProUGUI FindButtonLabel(Transform buttonTransform)
        {
            if (buttonTransform == null)
                return null;

            string[] names = { "Text - PresetValue", "Text - Label", "Label", "Text", "Text - Value" };
            for (int i = 0; i < names.Length; i++)
            {
                Transform labelTransform = FindDeep(buttonTransform, names[i]);
                if (labelTransform == null)
                    continue;

                TextMeshProUGUI label = labelTransform.GetComponent<TextMeshProUGUI>();
                if (label != null)
                    return label;
            }

            return buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
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

        private string BuildStatusText(string prefix, int simulationCount, int visibleCount)
        {
            string titleHex = ToHex(feedbackTimer > 0f ? activeColor : inactiveLabelColor);
            string valueHex = ToHex(activeColor);
            string bodyHex = ToHex(textColor);
            string noteHex = ToHex(noteColor);

            string text =
                "<size=72%><color=" + titleHex + ">" + prefix + "</color></size>\n" +
                "<color=" + valueHex + "><size=118%><b>" + FormatCount(simulationCount) + "</b></size></color>" +
                "  <size=84%><color=" + bodyHex + ">Visible " + FormatCount(visibleCount) + "</color></size>";

            if (string.IsNullOrWhiteSpace(contextNote))
                return text;

            return text + "\n<size=74%><color=" + noteHex + ">" + contextNote + "</color></size>";
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

        private static string FormatCount(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
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

        private static string ToHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
