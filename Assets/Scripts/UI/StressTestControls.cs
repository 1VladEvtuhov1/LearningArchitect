using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LearningArchitect.UI
{
    public sealed class StressTestControls : MonoBehaviour
    {
        public ModuleManager manager;
        public Button oneKButton;
        public Button fiveKButton;
        public Button tenKButton;
        public TextMeshProUGUI statusText;

        public Color activeColor = default;
        public Color activeBackgroundColor = default;
        public Color inactiveColor = default;
        public Color textColor = default;
        public Color inactiveLabelColor = default;
        public Color hoverBackgroundColor = default;
        public Color pressedBackgroundColor = default;
        public float activeScale = 1.04f;
        public float clickScale = 1.08f;

        private float feedbackTimer;
        private Button feedbackButton;

        private void Awake()
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
                inactiveLabelColor = textColor;

            if (hoverBackgroundColor == default)
                hoverBackgroundColor = ShowcasePalette.PanelHover;

            if (pressedBackgroundColor == default)
                pressedBackgroundColor = ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.22f);

            if (manager == null)
                manager = GetComponent<ModuleManager>();

            AddListener(oneKButton, 1000);
            AddListener(fiveKButton, 5000);
            AddListener(tenKButton, 10000);
            Refresh();
        }

        private void Update()
        {
            if (WasPressed(KeyCode.Alpha1))
                SetStressLevel(1000);

            if (WasPressed(KeyCode.Alpha2))
                SetStressLevel(5000);

            if (WasPressed(KeyCode.Alpha3))
                SetStressLevel(10000);

            if (feedbackTimer > 0f)
                feedbackTimer -= Time.unscaledDeltaTime;
            else
                feedbackButton = null;

            Refresh();
        }

        private void OnDestroy()
        {
            RemoveListener(oneKButton);
            RemoveListener(fiveKButton);
            RemoveListener(tenKButton);
        }

        public void SetStressLevel(int count)
        {
            if (manager != null)
                manager.SetStressLevel(count);

            feedbackButton = GetButtonForCount(count);
            feedbackTimer = 0.18f;
            Refresh();
        }

        private void Refresh()
        {
            int level = manager == null ? 0 : manager.CurrentStressLevel;

            SetButtonState(oneKButton, level == 1000);
            SetButtonState(fiveKButton, level == 5000);
            SetButtonState(tenKButton, level == 10000);

            if (statusText != null)
            {
                int activeCount = manager == null ? 0 : manager.ActiveItemCount;
                string prefix = feedbackTimer > 0f ? ShowcaseLocalization.GetText("load_selected") : ShowcaseLocalization.GetText("stress_load");
                statusText.text = prefix + "  " + FormatCount(level) + "  \u2022  " + ShowcaseLocalization.GetText("active") + " " + FormatCount(activeCount);
                statusText.color = feedbackTimer > 0f ? activeColor : textColor;
            }
        }

        private void AddListener(Button button, int count)
        {
            if (button == null)
                return;

            button.onClick.AddListener(delegate { SetStressLevel(count); });
        }

        private static void RemoveListener(Button button)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }

        private void SetButtonState(Button button, bool active)
        {
            if (button == null)
                return;

            ColorBlock colors = button.colors;
            colors.normalColor = active ? activeBackgroundColor : inactiveColor;
            colors.highlightedColor = hoverBackgroundColor;
            colors.pressedColor = pressedBackgroundColor;
            colors.selectedColor = active ? activeBackgroundColor : inactiveColor;
            colors.disabledColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0.42f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.transform.localScale = Vector3.one * GetButtonScale(button, active);

            TextMeshProUGUI label = FindButtonLabel(button.transform);
            if (label != null)
                label.color = active ? activeColor : inactiveLabelColor;
        }

        private static TextMeshProUGUI FindButtonLabel(Transform buttonTransform)
        {
            if (buttonTransform == null)
                return null;

            Transform labelTransform = buttonTransform.Find("Label");
            if (labelTransform == null)
                labelTransform = buttonTransform.Find("Text");

            return labelTransform == null ? null : labelTransform.GetComponent<TextMeshProUGUI>();
        }

        private Button GetButtonForCount(int count)
        {
            if (count == 1000)
                return oneKButton;

            if (count == 5000)
                return fiveKButton;

            if (count == 10000)
                return tenKButton;

            return null;
        }

        private float GetButtonScale(Button button, bool active)
        {
            if (button != feedbackButton)
                return active ? activeScale : 1f;

            float pulse = feedbackTimer <= 0f ? 0f : Mathf.Clamp01(feedbackTimer / 0.18f);
            return Mathf.Lerp(active ? activeScale : 1f, clickScale, pulse);
        }

        private static string FormatCount(int value)
        {
            if (value >= 1000)
                return (value / 1000).ToString() + "K";

            return value.ToString();
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
