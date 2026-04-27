using LearningArchitect.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    public sealed class MetricsOverlay : MonoBehaviour
    {
        public TextMeshProUGUI fpsText;
        public ModuleManager manager;
        public float refreshInterval = 0.25f;
        public float warningFps = 45f;
        public float criticalFps = 30f;
        public Color healthyColor = default;
        public Color warningColor = default;
        public Color criticalColor = default;
        public Color graphBackgroundColor = default;
        public Color titleColor = default;
        public Color labelColor = default;
        public Color valueColor = default;
        public int graphSampleCount = 64;
        public float graphMaxFps = 180f;

        private float nextRefreshTime;
        private float smoothedFps;
        private float smoothedFrameMs;
        private PerformanceGraph performanceGraph;

        private void Awake()
        {
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

            if (manager == null)
                manager = GetComponent<ModuleManager>();

            EnsureGraph();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
                return;

            float fps = 1f / dt;
            smoothedFps = smoothedFps <= 0f ? fps : Mathf.Lerp(smoothedFps, fps, 0.1f);
            float frameMs = dt * 1000f;
            smoothedFrameMs = smoothedFrameMs <= 0f ? frameMs : Mathf.Lerp(smoothedFrameMs, frameMs, 0.1f);
            UpdateGraph(smoothedFps);

            if (Time.unscaledTime < nextRefreshTime)
                return;

            nextRefreshTime = Time.unscaledTime + refreshInterval;

            if (fpsText != null)
            {
                int activeCount = manager == null ? 0 : manager.ActiveItemCount;
                string metricColor = ColorUtility.ToHtmlStringRGB(GetMetricColor(smoothedFps));
                string titleHex = ColorUtility.ToHtmlStringRGB(titleColor);
                string labelHex = ColorUtility.ToHtmlStringRGB(labelColor);
                string valueHex = ColorUtility.ToHtmlStringRGB(valueColor);
                fpsText.text =
                    "<size=78%><color=#" + titleHex + "><b>" + ShowcaseLocalization.GetText("performance") + "</b></color></size>\n" +
                    "<line-height=110%><size=92%><color=#" + labelHex + ">" + ShowcaseLocalization.GetText("fps") + "</color>    <size=130%><color=#" + metricColor + "><b>" + smoothedFps.ToString("0") + "</b></color></size>\n" +
                    "<color=#" + labelHex + ">" + ShowcaseLocalization.GetText("frame_time") + "</color>    <color=#" + valueHex + ">" + smoothedFrameMs.ToString("0.0") + " ms</color>\n" +
                    "<color=#" + labelHex + ">" + ShowcaseLocalization.GetText("particles") + "</color>    <color=#" + valueHex + ">" + FormatCount(activeCount) + "</color></size>";
            }
        }

        private Color GetMetricColor(float fps)
        {
            if (fps < criticalFps)
                return criticalColor;

            if (fps < warningFps)
                return warningColor;

            return healthyColor;
        }

        private void EnsureGraph()
        {
            Canvas canvas = FindCanvasByChild(transform, "RootFrame");
            if (canvas == null)
                return;

            RectTransform placeholder = FindDeep(canvas.transform, "ChartPlaceholder") as RectTransform;
            if (placeholder == null)
                return;

            Image placeholderImage = placeholder.GetComponent<Image>();
            if (placeholderImage != null)
            {
                placeholderImage.sprite = null;
                placeholderImage.type = Image.Type.Simple;
                placeholderImage.color = graphBackgroundColor;
                placeholderImage.raycastTarget = false;
            }

            Transform graphTransform = placeholder.Find("PerformanceGraph");
            if (graphTransform == null)
            {
                GameObject graphObject = new GameObject("PerformanceGraph", typeof(RectTransform), typeof(CanvasRenderer), typeof(PerformanceGraph));
                graphObject.transform.SetParent(placeholder, false);
                graphTransform = graphObject.transform;
            }

            RectTransform graphRect = graphTransform as RectTransform;
            graphRect.anchorMin = Vector2.zero;
            graphRect.anchorMax = Vector2.one;
            graphRect.offsetMin = new Vector2(8f, 8f);
            graphRect.offsetMax = new Vector2(-8f, -8f);

            performanceGraph = graphTransform.GetComponent<PerformanceGraph>();
            performanceGraph.SampleCapacity = graphSampleCount;
            performanceGraph.raycastTarget = false;
            performanceGraph.color = healthyColor;
        }

        private void UpdateGraph(float fps)
        {
            if (performanceGraph == null)
                EnsureGraph();

            if (performanceGraph == null)
                return;

            performanceGraph.color = GetMetricColor(fps);
            performanceGraph.AddSample(Mathf.Clamp01(fps / Mathf.Max(1f, graphMaxFps)));
        }

        private static string FormatCount(int value)
        {
            if (value >= 1000)
                return (value / 1000).ToString() + "K";

            return value.ToString();
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

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PerformanceGraph : MaskableGraphic
    {
        [SerializeField] private int sampleCapacity = 64;
        [SerializeField] private float lineThickness = 2.5f;
        [SerializeField] [Range(0f, 1f)] private float fillAlpha = 0.08f;

        private readonly List<float> samples = new List<float>(64);

        public int SampleCapacity
        {
            get { return Mathf.Max(2, sampleCapacity); }
            set
            {
                sampleCapacity = Mathf.Max(2, value);
                TrimSamples();
                SetVerticesDirty();
            }
        }

        public void AddSample(float normalizedValue)
        {
            samples.Add(Mathf.Clamp01(normalizedValue));
            TrimSamples();
            SetVerticesDirty();
        }

        public void Clear()
        {
            samples.Clear();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (samples.Count < 2)
                return;

            Rect rect = rectTransform.rect;
            float step = rect.width / (SampleCapacity - 1f);
            Color lineColor = color;
            Color fillColor = new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a * fillAlpha);

            for (int i = 1; i < samples.Count; i++)
            {
                Vector2 previous = new Vector2(rect.xMin + step * (i - 1), rect.yMin + rect.height * samples[i - 1]);
                Vector2 current = new Vector2(rect.xMin + step * i, rect.yMin + rect.height * samples[i]);

                AddFillQuad(vh, previous, current, rect.yMin, fillColor);
                AddLineQuad(vh, previous, current, lineThickness, lineColor);
            }
        }

        private void TrimSamples()
        {
            int overflow = samples.Count - SampleCapacity;
            if (overflow <= 0)
                return;

            samples.RemoveRange(0, overflow);
        }

        private static void AddFillQuad(VertexHelper vh, Vector2 previous, Vector2 current, float bottomY, Color color)
        {
            int startIndex = vh.currentVertCount;
            vh.AddVert(new Vector3(previous.x, bottomY), color, Vector2.zero);
            vh.AddVert(previous, color, Vector2.zero);
            vh.AddVert(current, color, Vector2.zero);
            vh.AddVert(new Vector3(current.x, bottomY), color, Vector2.zero);
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private static void AddLineQuad(VertexHelper vh, Vector2 previous, Vector2 current, float thickness, Color color)
        {
            Vector2 direction = (current - previous).normalized;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);
            int startIndex = vh.currentVertCount;
            vh.AddVert(previous - normal, color, Vector2.zero);
            vh.AddVert(previous + normal, color, Vector2.zero);
            vh.AddVert(current + normal, color, Vector2.zero);
            vh.AddVert(current - normal, color, Vector2.zero);
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }
    }
}
