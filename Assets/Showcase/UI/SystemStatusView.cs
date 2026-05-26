using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    public sealed class SystemStatusView : MonoBehaviour
    {
        [Serializable]
        public struct MetricSlotView
        {
            [SerializeField] private TextMeshProUGUI label;
            [SerializeField] private RectTransform barRoot;
            [SerializeField] private RectTransform fill;

            public void Validate(int index)
            {
                if (label == null)
                    throw new InvalidOperationException($"System status label {index} is required.");

                if (barRoot == null)
                    throw new InvalidOperationException($"System status bar root {index} is required.");

                if (fill == null)
                    throw new InvalidOperationException($"System status fill {index} is required.");
            }

            public void SetLabel(string text)
            {
                label.text = text;
            }

            public void SetFill(float normalized)
            {
                float width = Mathf.Max(4f, (barRoot.rect.width - 2f) * Mathf.Clamp01(normalized));
                fill.anchorMin = new Vector2(0f, 0f);
                fill.anchorMax = new Vector2(0f, 1f);
                fill.pivot = new Vector2(0f, 0.5f);
                fill.anchoredPosition = new Vector2(1f, 0f);
                fill.sizeDelta = new Vector2(width, -2f);
            }
        }

        [SerializeField] private MetricSlotView[] metricSlots = Array.Empty<MetricSlotView>();
        [SerializeField] private Image statusDot;

        public void ValidateReferences()
        {
            if (metricSlots == null || metricSlots.Length < 3)
                throw new InvalidOperationException("Three system status metric slots are required.");

            for (int i = 0; i < metricSlots.Length; i++)
                metricSlots[i].Validate(i);

            if (statusDot == null)
                throw new InvalidOperationException($"{nameof(statusDot)} is required.");
        }

        public void SetMetric(int index, string text, float normalized)
        {
            if (index < 0 || index >= metricSlots.Length)
                return;

            metricSlots[index].SetLabel(text);
            metricSlots[index].SetFill(normalized);
        }

        public void SetStatusColor(Color color)
        {
            statusDot.color = color;
        }
    }
}
