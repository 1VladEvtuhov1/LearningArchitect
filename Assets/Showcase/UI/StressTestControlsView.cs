using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    public sealed class StressTestControlsView : MonoBehaviour
    {
        [Serializable]
        public struct PresetButtonView
        {
            [SerializeField] private Button _button;
            [SerializeField] private TextMeshProUGUI _label;

            public Button Button => _button;

            public void Validate(int index)
            {
                if (_button == null)
                    throw new InvalidOperationException($"Stress preset button {index} is required.");

                if (_label == null)
                    throw new InvalidOperationException($"Stress preset label {index} is required.");
            }

            public void SetVisible(bool visible)
            {
                _button.gameObject.SetActive(visible);
            }

            public void SetLabel(string labelText)
            {
                _label.text = labelText;
            }

            public void SetState(
                float scale,
                Color normalColor,
                Color highlightedColor,
                Color pressedColor,
                Color disabledColor,
                Color labelColor)
            {
                ColorBlock colors = _button.colors;
                colors.normalColor = normalColor;
                colors.highlightedColor = highlightedColor;
                colors.pressedColor = pressedColor;
                colors.selectedColor = normalColor;
                colors.disabledColor = disabledColor;
                colors.fadeDuration = 0.08f;
                _button.colors = colors;
                _button.transform.localScale = Vector3.one * scale;
                _label.color = labelColor;
            }
        }

        [SerializeField] private PresetButtonView[] _presetButtons = Array.Empty<PresetButtonView>();

        public int PresetCount => _presetButtons.Length;

        public void ValidateReferences()
        {
            if (_presetButtons == null || _presetButtons.Length < 3)
                throw new InvalidOperationException("Three stress preset button slots are required.");

            for (int i = 0; i < _presetButtons.Length; i++)
                _presetButtons[i].Validate(i);
        }

        public Button GetButton(int index)
        {
            if (index < 0 || index >= _presetButtons.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _presetButtons[index].Button;
        }

        public void SetPresetVisible(int index, bool visible)
        {
            if ((uint)index >= (uint)_presetButtons.Length)
                return;

            _presetButtons[index].SetVisible(visible);
        }

        public void SetPresetLabel(int index, string labelText)
        {
            if ((uint)index >= (uint)_presetButtons.Length)
                return;

            _presetButtons[index].SetLabel(labelText);
        }

        public void SetPresetState(
            int index,
            float scale,
            Color normalColor,
            Color highlightedColor,
            Color pressedColor,
            Color disabledColor,
            Color labelColor)
        {
            if ((uint)index >= (uint)_presetButtons.Length)
                return;

            _presetButtons[index].SetState(
                scale,
                normalColor,
                highlightedColor,
                pressedColor,
                disabledColor,
                labelColor);
        }
    }
}
