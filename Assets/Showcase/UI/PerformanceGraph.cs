using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PerformanceGraph : MaskableGraphic
    {
        [SerializeField] private int sampleCapacity = 64;
        [SerializeField] private float lineThickness = 2.5f;
        [SerializeField] [Range(0f, 1f)] private float fillAlpha = 0.08f;
        [SerializeField] [Range(1f, 4f)] private float glowThicknessMultiplier = 2.4f;
        [SerializeField] [Range(0f, 1f)] private float glowAlpha = 0.24f;
        [SerializeField] [Range(0f, 1f)] private float guidelineAlpha = 0.1f;
        [SerializeField] [Range(0, 4)] private int guidelineCount = 2;

        private readonly List<float> samples = new(64);

        public int SampleCapacity
        {
            get => Mathf.Max(2, sampleCapacity);
            set
            {
                sampleCapacity = Mathf.Max(2, value);
                TrimSamples();
                SetVerticesDirty();
            }
        }

        public float LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = Mathf.Max(1f, value);
                SetVerticesDirty();
            }
        }

        public float FillAlpha
        {
            get => fillAlpha;
            set
            {
                fillAlpha = Mathf.Clamp01(value);
                SetVerticesDirty();
            }
        }

        public float GlowThicknessMultiplier
        {
            get => glowThicknessMultiplier;
            set
            {
                glowThicknessMultiplier = Mathf.Clamp(value, 1f, 4f);
                SetVerticesDirty();
            }
        }

        public float GlowAlpha
        {
            get => glowAlpha;
            set
            {
                glowAlpha = Mathf.Clamp01(value);
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

        public void SetSamples(IReadOnlyList<float> normalizedSamples)
        {
            samples.Clear();
            if (normalizedSamples != null)
            {
                for (int i = 0; i < normalizedSamples.Count; i++)
                    samples.Add(Mathf.Clamp01(normalizedSamples[i]));
            }

            TrimSamples();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (samples.Count < 2)
                return;

            Rect rect = rectTransform.rect;
            float step = rect.width / (SampleCapacity - 1f);
            Color lineColor = color;
            Color fillColor = new(lineColor.r, lineColor.g, lineColor.b, lineColor.a * fillAlpha);
            Color glowColor = new(lineColor.r, lineColor.g, lineColor.b, lineColor.a * glowAlpha);
            DrawGuidelines(vertexHelper, rect, lineColor);

            for (int i = 1; i < samples.Count; i++)
            {
                Vector2 previous = new(rect.xMin + step * (i - 1), rect.yMin + rect.height * samples[i - 1]);
                Vector2 current = new(rect.xMin + step * i, rect.yMin + rect.height * samples[i]);

                AddFillQuad(vertexHelper, previous, current, rect.yMin, fillColor);
                AddLineQuad(vertexHelper, previous, current, lineThickness * glowThicknessMultiplier, glowColor);
                AddLineQuad(vertexHelper, previous, current, lineThickness, lineColor);
            }
        }

        private void TrimSamples()
        {
            int overflow = samples.Count - SampleCapacity;
            if (overflow <= 0)
                return;

            samples.RemoveRange(0, overflow);
        }

        private static void AddFillQuad(VertexHelper vertexHelper, Vector2 previous, Vector2 current, float bottomY, Color color)
        {
            int startIndex = vertexHelper.currentVertCount;
            vertexHelper.AddVert(new Vector3(previous.x, bottomY), color, Vector2.zero);
            vertexHelper.AddVert(previous, color, Vector2.zero);
            vertexHelper.AddVert(current, color, Vector2.zero);
            vertexHelper.AddVert(new Vector3(current.x, bottomY), color, Vector2.zero);
            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private static void AddLineQuad(VertexHelper vertexHelper, Vector2 previous, Vector2 current, float thickness, Color color)
        {
            Vector2 direction = (current - previous).normalized;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            Vector2 normal = new(-direction.y, direction.x);
            normal *= thickness * 0.5f;

            int startIndex = vertexHelper.currentVertCount;
            vertexHelper.AddVert(previous - normal, color, Vector2.zero);
            vertexHelper.AddVert(previous + normal, color, Vector2.zero);
            vertexHelper.AddVert(current + normal, color, Vector2.zero);
            vertexHelper.AddVert(current - normal, color, Vector2.zero);
            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private void DrawGuidelines(VertexHelper vertexHelper, Rect rect, Color lineColor)
        {
            if (guidelineCount <= 0 || guidelineAlpha <= 0f)
                return;

            Color guideColor = new(lineColor.r, lineColor.g, lineColor.b, lineColor.a * guidelineAlpha);
            for (int i = 1; i <= guidelineCount; i++)
            {
                float normalizedHeight = i / (float)(guidelineCount + 1);
                float y = rect.yMin + rect.height * normalizedHeight;
                AddLineQuad(
                    vertexHelper,
                    new Vector2(rect.xMin, y),
                    new Vector2(rect.xMax, y),
                    1f,
                    guideColor);
            }
        }
    }
}
