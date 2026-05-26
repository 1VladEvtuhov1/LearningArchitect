using System;
using LearningArchitect.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Tests.UI
{
    [Category("LearningArchitect.UI.Edit")]
    public sealed class SystemStatusPresenterTests
    {
        [Test]
        public void SetSample_UpdatesMetricLabelsAndWarningColor()
        {
            SystemStatusPresenterHarness harness = null;

            try
            {
                harness = SystemStatusPresenterHarness.Create();
                SystemStatusPresenter presenter = new(
                    harness.View,
                    new SystemStatusTheme(Color.green, Color.yellow, Color.red, 70f, 90f),
                    8f);

                presenter.Initialize();
                presenter.SetSample(new SystemStatusSample(82f, 52f, 6.1f));

                Assert.That(harness.Labels[0].text, Is.EqualTo("CPU 82%"));
                Assert.That(harness.Labels[1].text, Is.EqualTo("GPU 52%"));
                Assert.That(harness.Labels[2].text, Is.EqualTo("MEM 6.1 GB"));
                Assert.That(harness.StatusDot.color, Is.EqualTo(Color.yellow));
                Assert.That(harness.Fills[0].sizeDelta.x, Is.GreaterThan(80f));
            }
            finally
            {
                harness?.Dispose();
            }
        }

        [Test]
        public void SetSample_UsesMemoryLoad_WhenCpuAndGpuAreUnavailable()
        {
            SystemStatusPresenterHarness harness = null;

            try
            {
                harness = SystemStatusPresenterHarness.Create();
                SystemStatusPresenter presenter = new(
                    harness.View,
                    new SystemStatusTheme(Color.green, Color.yellow, Color.red, 70f, 90f),
                    8f);

                presenter.Initialize();
                presenter.SetSample(new SystemStatusSample(-1f, -1f, 7.6f));

                Assert.That(harness.Labels[0].text, Is.EqualTo("CPU " + ShowcaseLocalization.GetText("na")));
                Assert.That(harness.Labels[1].text, Is.EqualTo("GPU " + ShowcaseLocalization.GetText("na")));
                Assert.That(harness.Labels[2].text, Is.EqualTo("MEM 7.6 GB"));
                Assert.That(harness.StatusDot.color, Is.EqualTo(Color.red));
            }
            finally
            {
                harness?.Dispose();
            }
        }

        private sealed class SystemStatusPresenterHarness
        {
            private readonly GameObject _root;

            private SystemStatusPresenterHarness(
                GameObject root,
                SystemStatusView view,
                TextMeshProUGUI[] labels,
                RectTransform[] fills,
                Image statusDot)
            {
                _root = root;
                View = view;
                Labels = labels;
                Fills = fills;
                StatusDot = statusDot;
            }

            public SystemStatusView View { get; }
            public TextMeshProUGUI[] Labels { get; }
            public RectTransform[] Fills { get; }
            public Image StatusDot { get; }

            public static SystemStatusPresenterHarness Create()
            {
                GameObject root = new("SystemStatus Presenter Test Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject viewObject = new("Container - SystemStatus", typeof(RectTransform), typeof(SystemStatusView));
                viewObject.transform.SetParent(root.transform, false);

                SystemStatusView view = viewObject.GetComponent<SystemStatusView>();
                if (view == null)
                    throw new InvalidOperationException("SystemStatusView could not be created.");

                TextMeshProUGUI[] labels = new TextMeshProUGUI[3];
                RectTransform[] barRoots = new RectTransform[3];
                RectTransform[] fills = new RectTransform[3];

                for (int i = 0; i < labels.Length; i++)
                {
                    labels[i] = CreateText(viewObject.transform, "Text - Metric" + i);

                    GameObject barRootObject = new("Image - Bar" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    barRootObject.transform.SetParent(viewObject.transform, false);
                    RectTransform barRoot = barRootObject.GetComponent<RectTransform>();
                    barRoot.sizeDelta = new Vector2(110f, 8f);
                    barRoots[i] = barRoot;

                    GameObject fillObject = new("Image - Fill" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    fillObject.transform.SetParent(barRootObject.transform, false);
                    fills[i] = fillObject.GetComponent<RectTransform>();
                }

                GameObject dotObject = new("StatusDot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                dotObject.transform.SetParent(viewObject.transform, false);
                Image statusDot = dotObject.GetComponent<Image>();

                SerializedObject viewSo = new(view);
                SerializedProperty metricSlotsProp = viewSo.FindProperty("metricSlots");
                metricSlotsProp.arraySize = labels.Length;
                for (int i = 0; i < labels.Length; i++)
                {
                    SerializedProperty slotProp = metricSlotsProp.GetArrayElementAtIndex(i);
                    slotProp.FindPropertyRelative("label").objectReferenceValue = labels[i];
                    slotProp.FindPropertyRelative("barRoot").objectReferenceValue = barRoots[i];
                    slotProp.FindPropertyRelative("fill").objectReferenceValue = fills[i];
                }

                viewSo.FindProperty("statusDot").objectReferenceValue = statusDot;
                viewSo.ApplyModifiedPropertiesWithoutUndo();

                return new SystemStatusPresenterHarness(root, view, labels, fills, statusDot);
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(_root);
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
        }
    }
}
