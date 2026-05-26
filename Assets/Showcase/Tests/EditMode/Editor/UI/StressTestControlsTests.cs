using System;
using System.Reflection;
using LearningArchitect.Core;
using LearningArchitect.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Tests.UI
{
    [Category("LearningArchitect.UI.Edit")]
    public sealed class StressTestControlsTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void ButtonClick_UsesExplicitViewBinding_ToRaiseDefaultStressPreset()
        {
            StressTestControlsHarness harness = null;

            try
            {
                harness = StressTestControlsHarness.Create();
                int requestedStress = 0;
                harness.Controls.StressRequested += value => requestedStress = value;

                harness.Buttons[1].onClick.Invoke();

                Assert.That(requestedStress, Is.EqualTo(160));
            }
            finally
            {
                harness?.Dispose();
            }
        }

        [Test]
        public void ConfigurePresets_UpdatesDedicatedLabels_AndHighlightsActivePreset()
        {
            StressTestControlsHarness harness = null;

            try
            {
                harness = StressTestControlsHarness.Create();
                harness.Controls.ConfigurePresets(
                    new[] { 1000, 5000, 25000 },
                    new[] { "1K", "5K", "25K" });
                harness.Controls.ShowStressState(25000);

                Assert.That(harness.Labels[0].text, Is.EqualTo("1K"));
                Assert.That(harness.Labels[1].text, Is.EqualTo("5K"));
                Assert.That(harness.Labels[2].text, Is.EqualTo("25K"));
                Assert.That(harness.Buttons[2].transform.localScale.x, Is.GreaterThan(1f));
            }
            finally
            {
                harness?.Dispose();
            }
        }

        [Test]
        public void ShowStressState_BeforeAwake_InitializesLazily_WithoutThrowing()
        {
            StressTestControlsHarness harness = null;

            try
            {
                harness = StressTestControlsHarness.Create(invokeAwake: false);

                Assert.DoesNotThrow(() => harness.Controls.ShowStressState(160));
            }
            finally
            {
                harness?.Dispose();
            }
        }

        private sealed class StressTestControlsHarness
        {
            private readonly GameObject _root;

            private StressTestControlsHarness(
                GameObject root,
                StressTestControls controls,
                Button[] buttons,
                TextMeshProUGUI[] labels)
            {
                _root = root;
                Controls = controls;
                Buttons = buttons;
                Labels = labels;
            }

            public StressTestControls Controls { get; }
            public Button[] Buttons { get; }
            public TextMeshProUGUI[] Labels { get; }

            public static StressTestControlsHarness Create(bool invokeAwake = true)
            {
                GameObject root = new("StressTestControls Test Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject host = new("Container - StressControls", typeof(RectTransform), typeof(StressTestControlsView), typeof(StressTestControls));
                host.transform.SetParent(root.transform, false);

                StressTestControls controls = host.GetComponent<StressTestControls>();
                StressTestControlsView view = host.GetComponent<StressTestControlsView>();
                if (controls == null || view == null)
                    throw new InvalidOperationException("Stress test controls could not be created.");

                Button[] buttons = new Button[3];
                TextMeshProUGUI[] labels = new TextMeshProUGUI[3];
                for (int i = 0; i < buttons.Length; i++)
                {
                    GameObject buttonObject = new("Button - StressPreset0" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    buttonObject.transform.SetParent(host.transform, false);
                    buttons[i] = buttonObject.GetComponent<Button>();
                    labels[i] = CreateText(buttonObject.transform, "Text - PresetValue");
                }

                SerializedObject viewSo = new(view);
                SerializedProperty presetButtonsProp = viewSo.FindProperty("_presetButtons");
                presetButtonsProp.arraySize = buttons.Length;
                for (int i = 0; i < buttons.Length; i++)
                {
                    SerializedProperty buttonSlot = presetButtonsProp.GetArrayElementAtIndex(i);
                    buttonSlot.FindPropertyRelative("_button").objectReferenceValue = buttons[i];
                    buttonSlot.FindPropertyRelative("_label").objectReferenceValue = labels[i];
                }

                viewSo.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject controlsSo = new(controls);
                controlsSo.FindProperty("_view").objectReferenceValue = view;
                controlsSo.ApplyModifiedPropertiesWithoutUndo();

                if (invokeAwake)
                    controls.RunInitializeForEditModeTests();

                return new StressTestControlsHarness(root, controls, buttons, labels);
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
