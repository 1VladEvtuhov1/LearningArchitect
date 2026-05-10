using System.Globalization;
using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    public sealed class MetricsOverlay : MonoBehaviour
    {
        public sealed class ReferenceMetricsProfile
        {
            public ReferenceMetricsProfile(
                float fps,
                float frameTimeMs,
                int simulationCount,
                int visibleCount,
                float moduleCpuMs,
                float graphMaxValue,
                float graphMidValue,
                float[] graphSamples,
                Color accentColor,
                string insightEn,
                string insightRu,
                string trendEn,
                string trendRu)
            {
                Fps = fps;
                FrameTimeMs = frameTimeMs;
                SimulationCount = simulationCount;
                VisibleCount = visibleCount;
                ModuleCpuMs = moduleCpuMs;
                GraphMaxValue = graphMaxValue;
                GraphMidValue = graphMidValue;
                GraphSamples = graphSamples;
                AccentColor = accentColor;
                InsightEn = insightEn;
                InsightRu = insightRu;
                TrendEn = trendEn;
                TrendRu = trendRu;
            }

            public float Fps { get; }
            public float FrameTimeMs { get; }
            public int SimulationCount { get; }
            public int VisibleCount { get; }
            public float ModuleCpuMs { get; }
            public float GraphMaxValue { get; }
            public float GraphMidValue { get; }
            public float[] GraphSamples { get; }
            public Color AccentColor { get; }
            public string InsightEn { get; }
            public string InsightRu { get; }
            public string TrendEn { get; }
            public string TrendRu { get; }
        }

        private sealed class MetricRow
        {
            public MetricRow(TextMeshProUGUI label, TextMeshProUGUI value)
            {
                Label = label;
                Value = value;
            }

            public TextMeshProUGUI Label { get; }
            public TextMeshProUGUI Value { get; }
        }

        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private float refreshInterval = 0.25f;
        [SerializeField] private float warningFps = 45f;
        [SerializeField] private float criticalFps = 30f;
        [SerializeField] private Color healthyColor = default;
        [SerializeField] private Color warningColor = default;
        [SerializeField] private Color criticalColor = default;
        [SerializeField] private Color graphBackgroundColor = default;
        [SerializeField] private Color titleColor = default;
        [SerializeField] private Color labelColor = default;
        [SerializeField] private Color valueColor = default;
        [SerializeField] private int graphSampleCount = 64;
        [SerializeField] private float graphMaxFps = 180f;
        private const int VisibleMetricRowCount = 5;

        private float nextRefreshTime;
        private float smoothedFps;
        private float smoothedFrameMs;
        private TextMeshProUGUI performanceHeaderText;
        private TextMeshProUGUI performanceInsightText;
        private TextMeshProUGUI chartSummaryText;
        private TextMeshProUGUI[] chartValueTexts;
        private MetricRow[] metricRows;
        private RectTransform chartPlaceholder;
        private Image chartBackgroundImage;
        private PerformanceGraph performanceGraph;
        private int activeCount;
        private ShowcaseMetricsSnapshot currentMetrics = ShowcaseMetricsSnapshot.Empty;
        private ReferenceMetricsProfile referenceProfile;
        private bool referenceGraphDirty;
        private bool isInitialized;

        public TextMeshProUGUI FpsText
        {
            get => fpsText;
            set => fpsText = value;
        }

        public Color TitleColor
        {
            get => titleColor;
            set => titleColor = value;
        }

        public Color LabelColor
        {
            get => labelColor;
            set => labelColor = value;
        }

        public Color ValueColor
        {
            get => valueColor;
            set => valueColor = value;
        }

        public Color GraphBackgroundColor
        {
            get => graphBackgroundColor;
            set => graphBackgroundColor = value;
        }

        public Color HealthyColor
        {
            get => healthyColor;
            set => healthyColor = value;
        }

        public Color WarningColor
        {
            get => warningColor;
            set => warningColor = value;
        }

        public Color CriticalColor
        {
            get => criticalColor;
            set => criticalColor = value;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            if (!EnsureInitialized())
                return;

            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
                return;

            if (HasReferenceProfile())
            {
                smoothedFps = referenceProfile.Fps;
                smoothedFrameMs = referenceProfile.FrameTimeMs;
            }
            else
            {
                float fps = 1f / dt;
                smoothedFps = smoothedFps <= 0f ? fps : Mathf.Lerp(smoothedFps, fps, 0.1f);
                float frameMs = dt * 1000f;
                smoothedFrameMs = smoothedFrameMs <= 0f ? frameMs : Mathf.Lerp(smoothedFrameMs, frameMs, 0.1f);
            }

            UpdateGraph(smoothedFps);

            if (Time.unscaledTime < nextRefreshTime)
                return;

            nextRefreshTime = Time.unscaledTime + refreshInterval;

            string metricColor = ColorUtility.ToHtmlStringRGB(GetMetricColor(smoothedFps));
            string titleHex = ColorUtility.ToHtmlStringRGB(titleColor);
            string labelHex = ColorUtility.ToHtmlStringRGB(labelColor);
            string valueHex = ColorUtility.ToHtmlStringRGB(valueColor);
            float moduleCpuMs = GetModuleCpuMs();

            string metricsText =
                "<size=78%><color=#" + titleHex + "><b>" + ShowcaseLocalization.GetText("performance") + "</b></color></size>\n" +
                "<line-height=102%><size=84%><color=#" + labelHex + ">" + ShowcaseLocalization.GetText("fps") + "</color>    <size=122%><color=#" + metricColor + "><b>" + smoothedFps.ToString("0") + "</b></color></size>\n" +
                "<color=#" + labelHex + ">" + GetFrameMetricLabel() + "</color>    <color=#" + valueHex + ">" + smoothedFrameMs.ToString("0.0") + " ms</color>\n" +
                "<color=#" + labelHex + ">" + GetModuleCpuLabel() + "</color>    <color=#" + valueHex + ">" + FormatCpuValue(moduleCpuMs) + "</color>\n" +
                "<color=#" + labelHex + ">" + GetSimulationLabel() + "</color>    <color=#" + valueHex + ">" + FormatCount(GetSimulationCount()) + "</color>\n" +
                "<color=#" + labelHex + ">" + GetVisibleLabel() + "</color>    <color=#" + valueHex + ">" + FormatCount(GetVisibleCount()) + "</color>";

            if (HasStructuredCardUi())
            {
                UpdateStructuredCardUi();
                return;
            }

            fpsText.text = metricsText + "</size>";
        }

        public void ShowActiveCount(int count)
        {
            activeCount = count < 0 ? 0 : count;
        }

        public void ShowMetrics(ShowcaseMetricsSnapshot metrics)
        {
            currentMetrics = metrics;
        }

        public void SetReferenceProfile(ReferenceMetricsProfile profile)
        {
            referenceProfile = profile;
            referenceGraphDirty = true;
        }

        public void ConfigureModule(ModuleDefinitionSO module)
        {
        }

        private bool EnsureInitialized()
        {
            if (isInitialized)
            {
                if (!HasStructuredCardUi())
                    TryResolveStructuredCard();

                if (performanceGraph == null)
                    TryResolveGraph();

                return true;
            }

            if (healthyColor == default)
                healthyColor = ShowcasePalette.AccentMain;

            if (warningColor == default)
                warningColor = ShowcasePalette.Warning;

            if (criticalColor == default)
                criticalColor = ShowcasePalette.Error;

            if (graphBackgroundColor == default)
                graphBackgroundColor = ShowcasePalette.WithAlpha(ShowcasePalette.AccentMain, 0.055f);

            if (titleColor == default)
                titleColor = ShowcasePalette.AccentMain;

            if (labelColor == default)
                labelColor = ShowcasePalette.TextSecondary;

            if (valueColor == default)
                valueColor = ShowcasePalette.TextPrimary;

            TryResolveStructuredCard();

            if (fpsText == null)
                fpsText = GetComponentInChildren<TextMeshProUGUI>(true);

            if (fpsText == null && !HasStructuredCardUi())
                return false;

            TryResolveGraph();
            isInitialized = true;
            return true;
        }

        private Color GetMetricColor(float fps)
        {
            if (fps < criticalFps)
                return criticalColor;

            if (fps < warningFps)
                return warningColor;

            return healthyColor;
        }

        private void TryResolveGraph()
        {
            RectTransform placeholder = chartPlaceholder;
            if (placeholder == null)
            {
                Canvas canvas = FindCanvasByChild(transform, "Container - Root");
                if (canvas == null)
                    return;

                placeholder = FindDeep(canvas.transform, "Container - ChartPlaceholder") as RectTransform;
                if (placeholder == null)
                    placeholder = FindDeep(canvas.transform, "ChartPlaceholder") as RectTransform;
            }

            if (placeholder == null)
                return;

            chartBackgroundImage = placeholder.GetComponent<Image>();
            if (chartBackgroundImage == null)
                return;

            chartBackgroundImage.sprite = null;
            chartBackgroundImage.type = Image.Type.Simple;
            chartBackgroundImage.color = GetGraphBackgroundTint();
            chartBackgroundImage.raycastTarget = false;

            Transform graphTransform = placeholder.Find("Graph - Performance");
            if (graphTransform == null)
                graphTransform = placeholder.Find("PerformanceGraph");
            if (graphTransform == null)
                return;

            performanceGraph = graphTransform.GetComponent<PerformanceGraph>();
            if (performanceGraph == null)
                return;

            performanceGraph.SampleCapacity = graphSampleCount;
            performanceGraph.LineThickness = 3.6f;
            performanceGraph.FillAlpha = 0.16f;
            performanceGraph.GlowAlpha = 0.26f;
            performanceGraph.GlowThicknessMultiplier = 2.9f;
            performanceGraph.raycastTarget = false;
            performanceGraph.color = GetGraphAccentColor();
            referenceGraphDirty = true;
        }

        private bool HasStructuredCardUi()
        {
            return performanceHeaderText != null &&
                   metricRows != null &&
                   metricRows.Length > 0;
        }

        private void UpdateStructuredCardUi()
        {
            if (performanceHeaderText != null)
            {
                performanceHeaderText.text = GetDisplayPerformanceHeaderText();
                performanceHeaderText.color = GetGraphAccentColor();
            }

            if (performanceInsightText != null)
            {
                performanceInsightText.text = GetDisplayPerformanceInsight();
                performanceInsightText.color = ShowcasePalette.TextSecondary;
            }

            if (chartBackgroundImage != null)
                chartBackgroundImage.color = GetGraphBackgroundTint();

            Color metricColor = GetMetricColor(smoothedFps);
            Color accentColor = GetGraphAccentColor();
            SetMetricRow(0, "FPS", smoothedFps.ToString("0"), metricColor);
            SetMetricRow(1, GetFrameMetricLabel(), smoothedFrameMs.ToString("0.0") + " ms");
            SetMetricRow(2, GetModuleCpuLabel(), FormatCpuValue(GetModuleCpuMs()));
            SetMetricRow(3, GetSimulationLabel(), FormatCount(GetSimulationCount()));
            SetMetricRow(4, GetVisibleLabel(), FormatCount(GetVisibleCount()));

            for (int i = 0; i < metricRows.Length; i++)
                SetMetricRowVisible(i, i < VisibleMetricRowCount);

            if (chartSummaryText != null)
            {
                chartSummaryText.text = BuildChartSummary();
                chartSummaryText.color = ShowcasePalette.WithAlpha(accentColor, 0.94f);
            }

            UpdateChartValueTexts();
        }

        private void SetMetricRow(int index, string label, string value)
        {
            SetMetricRow(index, label, value, valueColor);
        }

        private void SetMetricRow(int index, string label, string value, Color rowValueColor)
        {
            if (metricRows == null || index < 0 || index >= metricRows.Length)
                return;

            MetricRow row = metricRows[index];
            if (row == null)
                return;

            if (row.Label != null)
            {
                row.Label.text = label;
                row.Label.color = labelColor;
            }

            if (row.Value != null)
            {
                row.Value.text = value;
                row.Value.color = rowValueColor;
            }
        }

        private string BuildChartSummary()
        {
            if (HasReferenceProfile())
                return GetLocalized(referenceProfile.TrendEn, referenceProfile.TrendRu) + "\n" + BuildCompactMetricSummary();

            return BuildCompactMetricSummary();
        }

        private string BuildCompactMetricSummary()
        {
            string summary = GetVisibleLabel() + " " + FormatCount(GetVisibleCount()) +
                             "  |  " + GetSimulationLabel() + " " + FormatCount(GetSimulationCount());

            if (GetModuleCpuMs() >= 0f)
                summary += "  |  CPU " + GetModuleCpuMs().ToString("0.00") + " ms";

            return summary;
        }

        private string BuildPerformanceInsight()
        {
            if (HasReferenceProfile())
                return GetLocalized(referenceProfile.InsightEn, referenceProfile.InsightRu);

            return ShowcaseLocalization.CurrentLanguage == ShowcaseLanguage.Russian
                ? "Live-метрики показывают текущий runtime. Для WebGL proof важнее смотреть тренд между вариантами."
                : "Live metrics show the current runtime. For the WebGL proof, the trend between variants matters most.";
        }

        private static string GetPerformanceHeaderText()
        {
            return ShowcaseLocalization.CurrentLanguage == ShowcaseLanguage.Russian
                ? "PERFORMANCE PROFILE"
                : "PERFORMANCE PROFILE";
        }

        private static string GetShortFrameLabel()
        {
            return ShowcaseLocalization.CurrentLanguage == ShowcaseLanguage.Russian ? "КАДР" : "FRAME";
        }

        private static string GetShortVisibleLabel()
        {
            if (ShowcaseLocalization.CurrentLanguage == ShowcaseLanguage.Russian)
                return "ВИДНО";

            return "VISIBLE";
        }

        private static string GetLocalized(string english, string russian)
        {
            return ShowcaseLocalization.CurrentLanguage == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(russian)
                ? russian
                : english;
        }

        private string GetDisplayPerformanceInsight()
        {
            if (HasReferenceProfile())
                return GetLocalized(referenceProfile.InsightEn, referenceProfile.InsightRu);

            return "Live metrics show frame-level behavior and module-local CPU cost.";
        }

        private static string GetDisplayPerformanceHeaderText()
        {
            return "PERFORMANCE";
        }

        private static string GetFrameMetricLabel()
        {
            return "Frame ms";
        }

        private static string GetModuleCpuLabel()
        {
            return "Module CPU";
        }

        private static string GetSimulationLabel()
        {
            return "Simulated";
        }

        private static string GetVisibleLabel()
        {
            return "Visible";
        }

        private void TryResolveStructuredCard()
        {
            Transform performanceCard = FindDeep(transform, "Container - Performance");
            if (performanceCard == null)
                return;

            performanceHeaderText = FindDeep(performanceCard, "Text - PerformanceHeader")?.GetComponent<TextMeshProUGUI>();
            if (performanceHeaderText == null)
                performanceHeaderText = FindDeep(performanceCard, "Text - Header")?.GetComponent<TextMeshProUGUI>();

            performanceInsightText = FindDeep(performanceCard, "Text - PerformanceInsight")?.GetComponent<TextMeshProUGUI>();
            chartPlaceholder = FindDeep(performanceCard, "Container - ChartPlaceholder") as RectTransform;
            chartSummaryText = FindDeep(chartPlaceholder, "Label")?.GetComponent<TextMeshProUGUI>();
            chartValueTexts = ResolveChartValueTexts(chartPlaceholder);
            metricRows = ResolveMetricRows(performanceCard);
        }

        private void UpdateChartValueTexts()
        {
            if (chartValueTexts == null || chartValueTexts.Length == 0)
                return;

            Color accentColor = GetGraphAccentColor();
            Color subduedAccent = ShowcasePalette.WithAlpha(accentColor, 0.76f);
            Color lowerAccent = ShowcasePalette.WithAlpha(labelColor, 0.92f);

            float topScale = HasReferenceProfile()
                ? Mathf.Max(1f, referenceProfile.GraphMaxValue)
                : graphMaxFps;
            float middleScale = HasReferenceProfile()
                ? Mathf.Clamp(referenceProfile.GraphMidValue, 0f, topScale)
                : warningFps;

            int topValue = Mathf.RoundToInt(topScale);
            int middleValue = Mathf.RoundToInt(middleScale);

            if (chartValueTexts.Length > 0 && chartValueTexts[0] != null)
            {
                chartValueTexts[0].text = topValue.ToString();
                chartValueTexts[0].color = accentColor;
            }

            if (chartValueTexts.Length > 1 && chartValueTexts[1] != null)
            {
                chartValueTexts[1].text = middleValue.ToString();
                chartValueTexts[1].color = subduedAccent;
            }

            if (chartValueTexts.Length > 2 && chartValueTexts[2] != null)
            {
                chartValueTexts[2].text = "0";
                chartValueTexts[2].color = lowerAccent;
            }
        }

        private static TextMeshProUGUI[] ResolveChartValueTexts(Transform chartRoot)
        {
            Transform valuesRoot = FindDeep(chartRoot, "Container - PerformanceValue");
            if (valuesRoot == null)
                return null;

            TextMeshProUGUI[] texts = valuesRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts == null || texts.Length == 0)
                return null;

            return texts;
        }

        private MetricRow[] ResolveMetricRows(Transform performanceCard)
        {
            Transform statsContainer =
                FindDeep(performanceCard, "Layout - PerformanceStats") ??
                FindDeep(performanceCard, "Container - PerformanceStats") ??
                FindDeep(performanceCard, "Container - PerfomanceStats");
            if (statsContainer == null)
                return null;

            MetricRow[] rows = new MetricRow[statsContainer.childCount];
            int rowCount = 0;
            for (int i = 0; i < statsContainer.childCount; i++)
            {
                MetricRow row = ResolveMetricRow(statsContainer.GetChild(i));
                if (row == null)
                    continue;

                rows[rowCount++] = row;
            }

            if (rowCount == 0)
                return null;

            if (rowCount == rows.Length)
                return rows;

            MetricRow[] compactRows = new MetricRow[rowCount];
            for (int i = 0; i < rowCount; i++)
                compactRows[i] = rows[i];

            return compactRows;
        }

        private static MetricRow ResolveMetricRow(Transform rowRoot)
        {
            if (rowRoot == null)
                return null;

            TextMeshProUGUI label = null;
            TextMeshProUGUI value = null;
            TextMeshProUGUI[] texts = rowRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshProUGUI text = texts[i];
                string textName = text.name;
                if (label == null &&
                    (textName.IndexOf("Label", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                     textName.IndexOf("Name", System.StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    label = text;
                    continue;
                }

                if (value == null && textName.IndexOf("Value", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    value = text;
            }

            if (label == null && texts.Length > 0)
                label = texts[0];

            if (value == null && texts.Length > 1)
                value = texts[1];

            if (label == null || value == null)
                return null;

            return new MetricRow(label, value);
        }

        private void SetMetricRowVisible(int index, bool visible)
        {
            if (metricRows == null || index < 0 || index >= metricRows.Length)
                return;

            MetricRow row = metricRows[index];
            RectTransform rowRoot = GetMetricRowRoot(row);
            if (rowRoot != null)
                rowRoot.gameObject.SetActive(visible);
        }

        private static RectTransform GetMetricRowRoot(MetricRow row)
        {
            if (row?.Label != null && row.Label.rectTransform.parent is RectTransform labelParent)
                return labelParent;

            if (row?.Value != null && row.Value.rectTransform.parent is RectTransform valueParent)
                return valueParent;

            return null;
        }

        private void UpdateGraph(float fps)
        {
            if (performanceGraph == null)
                return;

            performanceGraph.color = GetGraphAccentColor();

            if (HasReferenceProfile())
            {
                if (!referenceGraphDirty)
                    return;

                performanceGraph.SetSamples(NormalizeReferenceGraph(referenceProfile));
                referenceGraphDirty = false;
                return;
            }

            performanceGraph.AddSample(Mathf.Clamp01(fps / Mathf.Max(1f, graphMaxFps)));
        }

        private static string FormatCount(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
        }

        private int GetSimulationCount()
        {
            if (HasReferenceProfile())
                return referenceProfile.SimulationCount;

            return currentMetrics.HasSimulationCount ? currentMetrics.SimulationCount : activeCount;
        }

        private int GetVisibleCount()
        {
            if (HasReferenceProfile())
                return referenceProfile.VisibleCount;

            return currentMetrics.HasVisibleCount ? currentMetrics.VisibleCount : activeCount;
        }

        private float GetModuleCpuMs()
        {
            if (HasReferenceProfile())
                return referenceProfile.ModuleCpuMs;

            return currentMetrics.HasModuleCpuMs ? currentMetrics.ModuleCpuMs : -1f;
        }

        private static string FormatCpuValue(float cpuMs)
        {
            return cpuMs >= 0f ? cpuMs.ToString("0.00") + " ms" : "\u2014";
        }

        private bool HasReferenceProfile()
        {
            return referenceProfile != null;
        }

        private float[] NormalizeReferenceGraph(ReferenceMetricsProfile profile)
        {
            if (profile == null || profile.GraphSamples == null || profile.GraphSamples.Length == 0)
                return null;

            float[] normalized = new float[profile.GraphSamples.Length];
            float maxFps = Mathf.Max(1f, profile.GraphMaxValue);
            for (int i = 0; i < normalized.Length; i++)
                normalized[i] = Mathf.Clamp01(profile.GraphSamples[i] / maxFps);

            return normalized;
        }

        public static bool TryResolveWebDemoProfile(VariantDefinitionSO variant, int stressLevel, out ReferenceMetricsProfile profile)
        {
            profile = null;
            if (variant == null)
                return false;

            if (MatchesVariant(variant, "UpdateLoop_PerObjectVariant", "Per-Object Update Variant"))
            {
                profile = BuildPerObjectProfile(stressLevel);
                return profile != null;
            }

            if (MatchesVariant(variant, "UpdateLoop_CentralizedVariant", "Centralized Update Variant"))
            {
                profile = BuildCentralizedProfile(stressLevel);
                return profile != null;
            }

            return false;
        }

        private static ReferenceMetricsProfile BuildPerObjectProfile(int stressLevel)
        {
            switch (stressLevel)
            {
                case 25:
                    return BuildProfile(148f, 6.7f, 25, 25, 0.12f, 165f, 80f, new Color(1f, 0.56f, 0.24f, 1f),
                        "Readable object ownership. Callback cost is still small at this preset.",
                        "Понятное владение объектами. Цена callback'ов пока почти не видна.",
                        "Trend: stable while callback count stays low.",
                        "Тренд: стабильно, пока callback'ов мало.",
                        154f, 153f, 151f, 149f, 147f, 145f, 143f, 141f, 142f, 144f, 146f, 149f);
                case 100:
                    return BuildProfile(86f, 11.6f, 100, 100, 1.08f, 120f, 60f, new Color(1f, 0.56f, 0.24f, 1f),
                        "Callback fan-out is visible. Each item still owns its own Update path.",
                        "Fan-out callback'ов уже виден. Каждый элемент всё ещё владеет своим Update.",
                        "Trend: frame time rises with object count.",
                        "Тренд: frame time растёт вместе с числом объектов.",
                        104f, 101f, 98f, 95f, 91f, 88f, 84f, 80f, 77f, 79f, 82f, 86f, 89f, 92f);
                case 250:
                    return BuildProfile(43f, 23.3f, 250, 250, 4.62f, 70f, 35f, new Color(1f, 0.56f, 0.24f, 1f),
                        "Simple ownership becomes expensive: many isolated callbacks now shape the frame.",
                        "Простое владение стало дорогим: кадр формируют сотни отдельных callback'ов.",
                        "Trend: drops below the smooth demo target.",
                        "Тренд: падает ниже комфортной цели demo.",
                        59f, 56f, 53f, 49f, 46f, 42f, 39f, 35f, 31f, 29f, 32f, 36f, 40f, 43f);
                default:
                    return null;
            }
        }

        private static ReferenceMetricsProfile BuildCentralizedProfile(int stressLevel)
        {
            switch (stressLevel)
            {
                case 100:
                    return BuildProfile(166f, 6.1f, 100, 100, 0.08f, 180f, 90f, new Color(0.24f, 0.86f, 0.82f, 1f),
                        "One owner loop keeps callback overhead flat while behavior stays comparable.",
                        "Один owner loop держит callback overhead ровным при том же поведении.",
                        "Trend: stable baseline for small and medium stress.",
                        "Тренд: стабильный baseline для малой и средней нагрузки.",
                        172f, 171f, 170f, 169f, 168f, 167f, 166f, 165f, 165f, 166f, 167f, 168f);
                case 1000:
                    return BuildProfile(124f, 8.0f, 1000, 260, 0.46f, 150f, 75f, new Color(0.24f, 0.86f, 0.82f, 1f),
                        "The simulation grows, but presentation stays capped and the update path remains centralized.",
                        "Симуляция растёт, но rendering cap и централизованный update держат картину стабильной.",
                        "Trend: graceful cost growth instead of callback spikes.",
                        "Тренд: плавный рост цены вместо callback spikes.",
                        132f, 131f, 130f, 128f, 126f, 124f, 122f, 121f, 120f, 121f, 123f, 124f, 126f, 127f);
                case 5000:
                    return BuildProfile(72f, 13.9f, 5000, 260, 1.94f, 100f, 50f, new Color(0.24f, 0.86f, 0.82f, 1f),
                        "Centralized ownership still degrades, but the cost stays legible and easier to tune.",
                        "Централизованный вариант тоже деградирует, но цена читаема и проще тюнится.",
                        "Trend: controlled decline under browser-safe stress.",
                        "Тренд: контролируемое снижение на browser-safe stress.",
                        83f, 81f, 79f, 77f, 75f, 73f, 71f, 69f, 67f, 66f, 67f, 69f, 71f, 73f);
                default:
                    return null;
            }
        }

        private static ReferenceMetricsProfile BuildProfile(
            float fps,
            float frameTimeMs,
            int simulationCount,
            int visibleCount,
            float moduleCpuMs,
            float graphMaxValue,
            float graphMidValue,
            Color accentColor,
            string insightEn,
            string insightRu,
            string trendEn,
            string trendRu,
            params float[] anchors)
        {
            return new ReferenceMetricsProfile(
                fps,
                frameTimeMs,
                simulationCount,
                visibleCount,
                moduleCpuMs,
                graphMaxValue,
                graphMidValue,
                ExpandAnchors(anchors, 24),
                accentColor,
                insightEn,
                insightRu,
                trendEn,
                trendRu);
        }

        private Color GetGraphAccentColor()
        {
            if (HasReferenceProfile())
                return referenceProfile.AccentColor;

            return GetMetricColor(smoothedFps);
        }

        private Color GetGraphBackgroundTint()
        {
            return HasReferenceProfile()
                ? ShowcasePalette.WithAlpha(GetGraphAccentColor(), 0.09f)
                : graphBackgroundColor;
        }

        private static float[] ExpandAnchors(float[] anchors, int sampleCount)
        {
            if (anchors == null || anchors.Length == 0)
                return new float[0];

            if (anchors.Length == 1 || sampleCount <= 1)
                return new[] { anchors[0] };

            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)(sampleCount - 1);
                float scaled = t * (anchors.Length - 1);
                int leftIndex = Mathf.FloorToInt(scaled);
                int rightIndex = Mathf.Min(leftIndex + 1, anchors.Length - 1);
                float lerpT = scaled - leftIndex;
                samples[i] = Mathf.Lerp(anchors[leftIndex], anchors[rightIndex], lerpT);
            }

            return samples;
        }

        private static bool MatchesVariant(VariantDefinitionSO variant, string assetName, string variantName)
        {
            return string.Equals(variant.name, assetName, System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(variant.GetLocalizationKey(), assetName, System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(variant.VariantName, variantName, System.StringComparison.OrdinalIgnoreCase);
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
    }
}
