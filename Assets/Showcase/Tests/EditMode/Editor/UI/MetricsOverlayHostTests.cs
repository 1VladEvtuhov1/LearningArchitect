using System;
using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using UnityEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Tests.UI
{
    [Category("LearningArchitect.UI.Edit")]
    public sealed class MetricsOverlayHostTests
    {
        [Test]
        public void ShowMetrics_PopulatesDedicatedRows_FromExplicitViewReferences()
        {
            MetricsOverlayHostHarness harness = null;

            try
            {
                harness = MetricsOverlayHostHarness.Create();
                harness.View.ValidateReferences();
                harness.Overlay.ShowActiveCount(5000);
                harness.Overlay.ShowMetrics(new ShowcaseMetricsSnapshot(20000, 1200, 2.5f));

                Assert.That(harness.Header.text, Is.EqualTo(ShowcaseLocalization.GetText("performance")));
                Assert.That(harness.RowLabels[0].text, Is.EqualTo(ShowcaseLocalization.GetText("fps")));
                Assert.That(harness.RowLabels[1].text, Is.EqualTo(ShowcaseLocalization.GetText("frame_time")));
                Assert.That(harness.RowLabels[2].text, Is.EqualTo(ShowcaseLocalization.GetText("active_items")));
                Assert.That(harness.RowValues[2].text, Is.EqualTo("1 200"));
            }
            finally
            {
                harness?.Dispose();
            }
        }

        [Test]
        public void ShowActiveCount_FallsBack_WhenVisibleCountIsUnavailable()
        {
            MetricsOverlayHostHarness harness = null;

            try
            {
                harness = MetricsOverlayHostHarness.Create();
                harness.Overlay.ShowMetrics(ShowcaseMetricsSnapshot.Empty);
                harness.Overlay.ShowActiveCount(4321);

                Assert.That(harness.RowValues[2].text, Is.EqualTo("4 321"));
            }
            finally
            {
                harness?.Dispose();
            }
        }

        private sealed class MetricsOverlayHostHarness
        {
            private readonly GameObject root;

            private MetricsOverlayHostHarness(
                GameObject root,
                MetricsOverlayHost overlay,
                MetricsOverlayView view,
                TextMeshProUGUI header,
                TextMeshProUGUI[] rowLabels,
                TextMeshProUGUI[] rowValues)
            {
                this.root = root;
                Overlay = overlay;
                View = view;
                Header = header;
                RowLabels = rowLabels;
                RowValues = rowValues;
            }

            public MetricsOverlayHost Overlay { get; }
            public MetricsOverlayView View { get; }
            public TextMeshProUGUI Header { get; }
            public TextMeshProUGUI[] RowLabels { get; }
            public TextMeshProUGUI[] RowValues { get; }

            public static MetricsOverlayHostHarness Create()
            {
                GameObject root = new("MetricsOverlayHost Test Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject host = new("MetricsOverlayHost Host", typeof(RectTransform));
                host.transform.SetParent(root.transform, false);

                MetricsOverlayHost overlay = host.AddComponent<MetricsOverlayHost>();
                MetricsOverlayView view = host.GetComponent<MetricsOverlayView>();
                if (view == null)
                    throw new InvalidOperationException("MetricsOverlayView could not be created.");

                TextMeshProUGUI header = CreateText(host.transform, "Text - PerformanceHeader");
                Image chartBackground = CreateImage(host.transform, "Container - ChartPlaceholder");

                GameObject graphObject = new("Graph - Performance", typeof(RectTransform), typeof(CanvasRenderer), typeof(PerformanceGraph));
                graphObject.transform.SetParent(chartBackground.transform, false);
                PerformanceGraph graph = graphObject.GetComponent<PerformanceGraph>();

                TextMeshProUGUI[] chartValues =
                {
                    CreateText(chartBackground.transform, "Text - ChartScaleMax"),
                    CreateText(chartBackground.transform, "Text - ChartScaleMid"),
                    CreateText(chartBackground.transform, "Text - ChartScaleMin")
                };

                TextMeshProUGUI[] rowLabels = new TextMeshProUGUI[3];
                TextMeshProUGUI[] rowValues = new TextMeshProUGUI[3];
                GameObject[] rowRoots = new GameObject[3];

                for (int i = 0; i < 3; i++)
                {
                    rowRoots[i] = new GameObject("Container - MetricRow" + i, typeof(RectTransform));
                    rowRoots[i].transform.SetParent(host.transform, false);
                    rowLabels[i] = CreateText(rowRoots[i].transform, "Text - MetricLabel");
                    rowValues[i] = CreateText(rowRoots[i].transform, "Text - MetricValue");
                }

                SerializedObject viewSo = new(view);
                viewSo.FindProperty("performanceHeaderText").objectReferenceValue = header;
                viewSo.FindProperty("chartBackgroundImage").objectReferenceValue = chartBackground;
                viewSo.FindProperty("performanceGraph").objectReferenceValue = graph;

                SerializedProperty chartValuesProp = viewSo.FindProperty("chartValueTexts");
                chartValuesProp.arraySize = chartValues.Length;
                for (int i = 0; i < chartValues.Length; i++)
                    chartValuesProp.GetArrayElementAtIndex(i).objectReferenceValue = chartValues[i];

                SerializedProperty metricRowsProp = viewSo.FindProperty("metricRows");
                metricRowsProp.arraySize = rowRoots.Length;
                for (int i = 0; i < rowRoots.Length; i++)
                {
                    SerializedProperty rowProp = metricRowsProp.GetArrayElementAtIndex(i);
                    rowProp.FindPropertyRelative("root").objectReferenceValue = rowRoots[i];
                    rowProp.FindPropertyRelative("label").objectReferenceValue = rowLabels[i];
                    rowProp.FindPropertyRelative("value").objectReferenceValue = rowValues[i];
                }

                viewSo.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject overlaySo = new(overlay);
                overlaySo.FindProperty("_view").objectReferenceValue = view;
                overlaySo.ApplyModifiedPropertiesWithoutUndo();

                overlay.RunBootstrapForEditModeTests();

                return new MetricsOverlayHostHarness(root, overlay, view, header, rowLabels, rowValues);
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            private static TextMeshProUGUI CreateText(Transform parent, string name)
            {
                GameObject node = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                node.transform.SetParent(parent, false);
                TextMeshProUGUI label = node.GetComponent<TextMeshProUGUI>();
                label.raycastTarget = false;
                label.richText = false;
                if (TMP_Settings.defaultFontAsset != null)
                    label.font = TMP_Settings.defaultFontAsset;

                return label;
            }

            private static Image CreateImage(Transform parent, string name)
            {
                GameObject node = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                node.transform.SetParent(parent, false);
                return node.GetComponent<Image>();
            }
        }
    }
}
