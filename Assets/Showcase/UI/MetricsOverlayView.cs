using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    public sealed class MetricsOverlayView : MonoBehaviour
    {
        [Serializable]
        public struct MetricRowView
        {
            [SerializeField] private GameObject root;
            [SerializeField] private TextMeshProUGUI label;
            [SerializeField] private TextMeshProUGUI value;

            public void Validate(int index)
            {
                if (!root)
                    throw new InvalidOperationException($"Metric row root {index} is required.");

                if (!label)
                    throw new InvalidOperationException($"Metric row label {index} is required.");

                if (!value)
                    throw new InvalidOperationException($"Metric row value {index} is required.");
            }

            public void SetLabel(string labelText, Color labelColor)
            {
                if (!label)
                    return;

                label.text = labelText;
                label.color = labelColor;
            }

            public void SetValue(string valueText, Color valueColor, bool visible)
            {
                if (!root)
                    return;

                root.SetActive(visible);
                if (!visible || !value)
                    return;

                value.text = valueText;
                value.color = valueColor;
            }
        }

        [SerializeField] private TextMeshProUGUI performanceHeaderText;
        [SerializeField] private TextMeshProUGUI[] chartValueTexts = Array.Empty<TextMeshProUGUI>();
        [SerializeField] private Image chartBackgroundImage;
        [SerializeField] private PerformanceGraph performanceGraph;
        [SerializeField] private MetricRowView[] metricRows = Array.Empty<MetricRowView>();

        public void ValidateReferences()
        {
            if (performanceHeaderText == null)
                throw new InvalidOperationException($"{nameof(performanceHeaderText)} is required.");

            if (chartBackgroundImage == null)
                throw new InvalidOperationException($"{nameof(chartBackgroundImage)} is required.");

            if (performanceGraph == null)
                throw new InvalidOperationException($"{nameof(performanceGraph)} is required.");

            if (chartValueTexts == null || chartValueTexts.Length < 3)
                throw new InvalidOperationException("Three chart value labels are required.");

            for (int i = 0; i < 3; i++)
            {
                if (chartValueTexts[i] == null)
                    throw new InvalidOperationException($"Chart value label {i} is required.");
            }

            if (metricRows == null || metricRows.Length < 3)
                throw new InvalidOperationException("Three metric rows are required.");

            for (int i = 0; i < metricRows.Length; i++)
                metricRows[i].Validate(i);
        }

        public void SetHeader(string text, Color color)
        {
            performanceHeaderText.text = text;
            performanceHeaderText.color = color;
        }

        public void SetChartValue(int index, string text, Color color)
        {
            if (index < 0 || index >= chartValueTexts.Length)
                return;

            TextMeshProUGUI target = chartValueTexts[index];
            if (target == null)
                return;

            target.text = text;
            target.color = color;
        }

        public void ConfigureGraph(
            int sampleCapacity,
            float lineThickness,
            float fillAlpha,
            float glowAlpha,
            float glowThicknessMultiplier,
            Color accentColor,
            Color backgroundColor)
        {
            chartBackgroundImage.color = backgroundColor;
            chartBackgroundImage.raycastTarget = false;

            performanceGraph.SampleCapacity = sampleCapacity;
            performanceGraph.LineThickness = lineThickness;
            performanceGraph.FillAlpha = fillAlpha;
            performanceGraph.GlowAlpha = glowAlpha;
            performanceGraph.GlowThicknessMultiplier = glowThicknessMultiplier;
            performanceGraph.raycastTarget = false;
            performanceGraph.color = accentColor;
        }

        public void AddGraphSample(float normalizedSample)
        {
            performanceGraph.AddSample(normalizedSample);
        }

        public void SetMetricLabel(int index, string labelText, Color labelColor)
        {
            if (index < 0 || index >= metricRows.Length)
                return;

            metricRows[index].SetLabel(labelText, labelColor);
        }

        public void SetMetricValue(int index, string valueText, Color valueColor, bool visible)
        {
            if (index < 0 || index >= metricRows.Length)
                return;

            metricRows[index].SetValue(valueText, valueColor, visible);
        }
    }
}
