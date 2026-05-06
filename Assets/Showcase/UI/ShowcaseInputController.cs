using System;
using System.Collections.Generic;
using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseCommandRouter))]
    [RequireComponent(typeof(HubUI))]
    public sealed class ShowcaseInputController : MonoBehaviour
    {
        [SerializeField] private ShowcaseCommandRouter commands;
        [SerializeField] private HubUI hubUI;

        [Header("Input")]
        [SerializeField] private KeyCode previousModuleKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode nextModuleKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode previousVariantKey = KeyCode.UpArrow;
        [SerializeField] private KeyCode nextVariantKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode toggleDemoKey = KeyCode.G;

        private RecruiterDemoController recruiterDemo;

        private void Awake()
        {
            commands = commands != null ? commands : GetComponent<ShowcaseCommandRouter>();
            hubUI = hubUI != null ? hubUI : GetComponent<HubUI>();
            recruiterDemo = GetComponent<RecruiterDemoController>();

            if (commands == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCommandRouter)} is required.");

            if (hubUI == null)
                throw new InvalidOperationException($"{nameof(HubUI)} is required.");
        }

        private void Update()
        {
            if (WasPressed(previousModuleKey))
            {
                hubUI.PlayModuleSwitchFeedback(-1);
                commands.PreviousModule();
            }

            if (WasPressed(nextModuleKey))
            {
                hubUI.PlayModuleSwitchFeedback(1);
                commands.NextModule();
            }

            if (WasPressed(previousVariantKey))
            {
                hubUI.PlayVariantSwitchFeedback(-1);
                commands.PreviousVariant();
            }

            if (WasPressed(nextVariantKey))
            {
                hubUI.PlayVariantSwitchFeedback(1);
                commands.NextVariant();
            }

            if (recruiterDemo != null && WasPressed(toggleDemoKey))
                recruiterDemo.ToggleDemoExternal();
        }

        private static bool WasPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                switch (key)
                {
                    case KeyCode.LeftArrow:
                        return keyboard.leftArrowKey.wasPressedThisFrame;
                    case KeyCode.RightArrow:
                        return keyboard.rightArrowKey.wasPressedThisFrame;
                    case KeyCode.UpArrow:
                        return keyboard.upArrowKey.wasPressedThisFrame;
                    case KeyCode.DownArrow:
                        return keyboard.downArrowKey.wasPressedThisFrame;
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

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseCommandRouter))]
    [RequireComponent(typeof(ShowcaseCompositionRoot))]
    [RequireComponent(typeof(ShowcaseRuntimeController))]
    [RequireComponent(typeof(ShowcaseStateHub))]
    [RequireComponent(typeof(HubUI))]
    public sealed class RecruiterDemoController : MonoBehaviour
    {
        private const float CharacterAnimationYawOffset = 180f;
        private const float CharacterAnimationPitchOffset = -7f;
        private const float CharacterAnimationDistanceOffset = -4.5f;

        [Serializable]
        private struct DemoStep
        {
            public int ModuleIndex;
            public int VariantIndex;
            public int StressLevel;
            public float Duration;
            public string Note;
            public float YawOffset;
            public float PitchOffset;
            public float DistanceOffset;
        }

        [SerializeField] private ShowcaseCommandRouter commands;
        [SerializeField] private ShowcaseCompositionRoot compositionRoot;
        [SerializeField] private ShowcaseRuntimeController runtimeController;
        [SerializeField] private ShowcaseStateHub stateHub;
        [SerializeField] private HubUI hubUI;
        [SerializeField] private float defaultStepDuration = 6.5f;
        [SerializeField] private float introOverlayDuration = 1.8f;
        [SerializeField] private float outroOverlayDuration = 2.2f;

        private HubCameraDrift cameraDrift;
        private CanvasGroup demoOverlay;
        private TextMeshProUGUI demoOverlayBody;
        private Button demoButton;
        private TextMeshProUGUI demoButtonLabel;
        private TextMeshProUGUI demoOverlayTitle;
        private int currentStepIndex = -1;
        private bool isRunning;
        private bool overlayVisible;
        private float overlayFadeVelocity;
        private float overlayTargetAlpha;
        private float nextAdvanceTime;
        private DemoStep[] steps = Array.Empty<DemoStep>();

        private void Awake()
        {
            commands = commands != null ? commands : GetComponent<ShowcaseCommandRouter>();
            compositionRoot = compositionRoot != null ? compositionRoot : GetComponent<ShowcaseCompositionRoot>();
            runtimeController = runtimeController != null ? runtimeController : GetComponent<ShowcaseRuntimeController>();
            stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();
            hubUI = hubUI != null ? hubUI : GetComponent<HubUI>();

            if (commands == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCommandRouter)} is required.");

            if (compositionRoot == null)
                throw new InvalidOperationException($"{nameof(ShowcaseCompositionRoot)} is required.");

            if (runtimeController == null)
                throw new InvalidOperationException($"{nameof(ShowcaseRuntimeController)} is required.");

            if (stateHub == null)
                throw new InvalidOperationException($"{nameof(ShowcaseStateHub)} is required.");

            if (hubUI == null)
                throw new InvalidOperationException($"{nameof(HubUI)} is required.");

            ResolveUi();
            cameraDrift = FindFirstObjectByType<HubCameraDrift>();
            BuildSteps();
            RefreshButtonLabel();
        }

        private void OnEnable()
        {
            if (demoButton != null)
            {
                demoButton.onClick.RemoveListener(ToggleDemo);
                demoButton.onClick.AddListener(ToggleDemo);
            }

            stateHub.SelectionChanged += HandleSelectionChanged;
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
            RefreshButtonLabel();
        }

        private void OnDisable()
        {
            if (demoButton != null)
                demoButton.onClick.RemoveListener(ToggleDemo);

            stateHub.SelectionChanged -= HandleSelectionChanged;
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
        }

        private void Update()
        {
            UpdateOverlay();

            if (!isRunning || steps.Length == 0)
                return;

            if (Time.unscaledTime >= nextAdvanceTime)
                AdvanceToNextStep();
        }

        public void ToggleDemoExternal()
        {
            ToggleDemo();
        }

        private void ToggleDemo()
        {
            if (isRunning)
            {
                StopDemo(false);
                return;
            }

            StartDemo();
        }

        private void StartDemo()
        {
            BuildSteps();
            if (steps.Length == 0)
                return;

            isRunning = true;
            currentStepIndex = -1;
            ShowOverlay("Recruiter Demo", "Guided walkthrough of architecture trade-offs and scalable runtime patterns.", introOverlayDuration);
            AdvanceToStep(0);
            RefreshButtonLabel();
        }

        private void StopDemo(bool completed)
        {
            isRunning = false;
            nextAdvanceTime = 0f;
            currentStepIndex = -1;
            overlayVisible = false;
            overlayTargetAlpha = 0f;

            hubUI.ClearHintOverride();
            RefreshCurrentSelection();
            ResetCameraCue();

            if (completed)
            {
                hubUI.ShowContextHint(ShowcaseLocalization.GetText("demo_complete"));
                ShowOverlay("Demo Complete", "The showcase is ready for free exploration or the next module pass.", outroOverlayDuration);
            }

            RefreshButtonLabel();
        }

        private void AdvanceToNextStep()
        {
            int nextIndex = currentStepIndex + 1;
            if (nextIndex >= steps.Length)
            {
                StopDemo(true);
                return;
            }

            AdvanceToStep(nextIndex);
        }

        private void AdvanceToStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= steps.Length)
                return;

            currentStepIndex = stepIndex;
            DemoStep step = steps[stepIndex];
            commands.LoadSelection(step.ModuleIndex, step.VariantIndex);
            commands.SetStressLevel(step.StressLevel);
            nextAdvanceTime = Time.unscaledTime + Mathf.Max(2.5f, step.Duration);
            ApplyCameraCue(step);
            UpdateHintOverride();
            RefreshButtonLabel();
        }

        private void HandleSelectionChanged(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (!isRunning)
            {
                ApplySelectionCameraCue(module, variant);
                return;
            }

            UpdateHintOverride();
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            if (isRunning)
                UpdateHintOverride();

            RefreshButtonLabel();
        }

        private void BuildSteps()
        {
            ModuleDefinitionSO[] modules = compositionRoot.Modules;
            if (modules == null || modules.Length == 0)
            {
                steps = Array.Empty<DemoStep>();
                return;
            }

            List<DemoStep> curatedSteps = new();

            AddCuratedStep(curatedSteps, modules, "UpdateLoopStrategiesModule", "UpdateLoop_PerObjectVariant", false, "Readable baseline: one Update per mover.", 5.5f, -8f, 2f, 0.8f);
            AddCuratedStep(curatedSteps, modules, "UpdateLoopStrategiesModule", "UpdateLoop_CentralizedVariant", true, "One owner loop replaces callback pressure.", 7f, 10f, -1f, -0.6f);
            AddCuratedStep(curatedSteps, modules, "ObjectPoolingModule", "Pooling_InstantiationVariant", false, "Spawn churn appears immediately under repeated load.", 5.5f, -14f, 1f, 0.4f);
            AddCuratedStep(curatedSteps, modules, "ObjectPoolingModule", "Pooling_PooledVariant", true, "Reuse stabilizes the steady-state runtime path.", 7f, 14f, -2f, -0.8f);
            AddCuratedStep(curatedSteps, modules, "EffectsModule", "Effects_IndieVariant", false, "Object-level logic is easy to read at small counts.", 5.5f, -18f, 4f, 0.6f);
            AddCuratedStep(curatedSteps, modules, "EffectsModule", "Effects_ChunkVariant", true, "Decoupling simulation from visuals unlocks scale.", 7f, 18f, -3f, -1.2f);
            AddCuratedStep(curatedSteps, modules, "AISystemModule", "AI_FsmVariant", false, "Explicit state is easiest to debug and explain.", 5.5f, -10f, 3f, 0.5f);
            AddCuratedStep(curatedSteps, modules, "AISystemModule", "AI_UtilityVariant", true, "Scoring trades traceability for adaptation.", 6f, 8f, 0f, -0.4f);
            AddCuratedStep(curatedSteps, modules, "AISystemModule", "AI_BehaviorTreeVariant", false, "Branch structure keeps intent readable.", 6f, 0f, -2f, 0f);
            AddCuratedStep(curatedSteps, modules, "InventoryModule", "Inventory_ObjectSlotsVariant", false, "Object-rich slots are readable but spend more work on ownership and mutation paths.", 5.5f, -12f, 2f, 0.3f);
            AddCuratedStep(curatedSteps, modules, "InventoryModule", "Inventory_PackedSlotsVariant", true, "Packed arrays keep the transfer loop tight under heavier load.", 6.5f, 14f, -1f, -0.6f);
            AddCuratedStep(curatedSteps, modules, "Animation3DModule", "Animation_RunVariant", false, "Baseline locomotion keeps the character readable before combat is layered in.", 5.5f, CharacterAnimationYawOffset, CharacterAnimationPitchOffset, CharacterAnimationDistanceOffset);
            AddCuratedStep(curatedSteps, modules, "Animation3DModule", "Animation_RunShootVariant", true, "Animator layers keep shooting responsive while locomotion continues underneath.", 6.2f, CharacterAnimationYawOffset, CharacterAnimationPitchOffset, CharacterAnimationDistanceOffset);
            AddCuratedStep(curatedSteps, modules, "VfxModule", "Vfx_EmitterBurstsVariant", false, "One emitter per node is straightforward but scales visible authoring cost quickly.", 5.5f, -16f, 2f, 0.7f);
            AddCuratedStep(curatedSteps, modules, "VfxModule", "Vfx_BatchedPulsesVariant", true, "Shared pulse rendering keeps the visible layer cheaper than emitter-per-event playback.", 6.4f, 16f, -2f, -0.8f);

            if (curatedSteps.Count == 0)
            {
                BuildFallbackSteps(modules);
                return;
            }

            steps = curatedSteps.ToArray();
        }

        private void BuildFallbackSteps(ModuleDefinitionSO[] modules)
        {
            List<DemoStep> fallbackSteps = new();
            for (int moduleIndex = 0; moduleIndex < modules.Length; moduleIndex++)
            {
                VariantDefinitionSO[] variants = modules[moduleIndex]?.Variants;
                if (variants == null)
                    continue;

                for (int variantIndex = 0; variantIndex < variants.Length; variantIndex++)
                {
                    VariantDefinitionSO variant = variants[variantIndex];
                    fallbackSteps.Add(new DemoStep
                    {
                        ModuleIndex = moduleIndex,
                        VariantIndex = variantIndex,
                        StressLevel = ChooseDemoStressLevel(variant, false),
                        Duration = Mathf.Max(2.5f, defaultStepDuration),
                        Note = string.IsNullOrWhiteSpace(variant?.Takeaway)
                            ? "Module walkthrough step."
                            : variant.Takeaway,
                        YawOffset = 0f,
                        PitchOffset = 0f,
                        DistanceOffset = 0f
                    });
                }
            }

            steps = fallbackSteps.ToArray();
        }

        private void AddCuratedStep(
            List<DemoStep> stepsBuffer,
            ModuleDefinitionSO[] modules,
            string moduleAssetName,
            string variantAssetName,
            bool preferHighStress,
            string note,
            float duration,
            float yawOffset,
            float pitchOffset,
            float distanceOffset)
        {
            if (!TryFindModuleIndex(modules, moduleAssetName, out int moduleIndex))
                return;

            ModuleDefinitionSO module = modules[moduleIndex];
            int variantIndex = FindVariantIndex(module, variantAssetName);
            if (variantIndex < 0)
                return;

            VariantDefinitionSO variant = module.Variants[variantIndex];
            stepsBuffer.Add(new DemoStep
            {
                ModuleIndex = moduleIndex,
                VariantIndex = variantIndex,
                StressLevel = ChooseDemoStressLevel(variant, preferHighStress),
                Duration = Mathf.Max(2.5f, duration),
                Note = note,
                YawOffset = yawOffset,
                PitchOffset = pitchOffset,
                DistanceOffset = distanceOffset
            });
        }

        private int ChooseDemoStressLevel(VariantDefinitionSO variant, bool preferHighStress)
        {
            if (variant == null)
                return 1000;

            int[] presets = variant.GetStressPresets();
            if (presets == null || presets.Length == 0)
                return 1000;

            if (preferHighStress)
                return presets[presets.Length - 1];

            if (presets.Length >= 2)
                return presets[1];

            return presets[0];
        }

        private void UpdateHintOverride()
        {
            if (!isRunning || currentStepIndex < 0 || currentStepIndex >= steps.Length)
                return;

            string text = ShowcaseLocalization.GetText("guided_demo") +
                          " | " +
                          ShowcaseLocalization.GetText("step") +
                          " " +
                          (currentStepIndex + 1) +
                          "/" +
                          steps.Length +
                          " | " +
                          steps[currentStepIndex].Note;

            hubUI.SetHintOverride(text);
            RefreshCurrentSelection();
        }

        private void RefreshCurrentSelection()
        {
            hubUI.ShowSelection(stateHub.CurrentModule, stateHub.CurrentVariant);
        }

        private void RefreshButtonLabel()
        {
            if (demoButtonLabel == null)
                return;

            demoButtonLabel.text = "<b>" +
                                   (isRunning
                                       ? ShowcaseLocalization.GetText("stop_demo")
                                       : ShowcaseLocalization.GetText("start_demo")) +
                                   "</b>";
        }

        private void ResolveUi()
        {
            demoButton = FindButton(transform, "RecruiterDemoButton");
            demoButtonLabel = demoButton == null ? null : demoButton.GetComponentInChildren<TextMeshProUGUI>(true);
            demoOverlay = FindCanvasGroup(transform, "DemoOverlay");
            demoOverlayTitle = FindText(transform, "DemoOverlayTitle");
            demoOverlayBody = FindText(transform, "DemoOverlayBody");
        }

        private void ApplyCameraCue(DemoStep step)
        {
            if (cameraDrift == null)
                return;

            cameraDrift.ApplyDemoCue(step.YawOffset, step.PitchOffset, step.DistanceOffset);
        }

        private void ApplySelectionCameraCue(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (cameraDrift == null)
                return;

            if (IsCharacterAnimationSelection(module, variant))
            {
                cameraDrift.ApplyDemoCue(CharacterAnimationYawOffset, CharacterAnimationPitchOffset, CharacterAnimationDistanceOffset);
                return;
            }

            cameraDrift.ResetDemoCue();
        }

        private static bool IsCharacterAnimationSelection(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (module == null || variant == null)
                return false;

            if (!string.Equals(module.name, "Animation3DModule", StringComparison.OrdinalIgnoreCase))
                return false;

            return string.Equals(variant.name, "Animation_RunVariant", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(variant.name, "Animation_ShootVariant", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(variant.name, "Animation_RunShootVariant", StringComparison.OrdinalIgnoreCase);
        }

        private void ResetCameraCue()
        {
            if (cameraDrift == null)
                return;

            cameraDrift.ResetDemoCue();
        }

        private void ShowOverlay(string title, string body, float duration)
        {
            if (demoOverlay == null)
                return;

            if (demoOverlayTitle != null)
                demoOverlayTitle.text = title;

            if (demoOverlayBody != null)
                demoOverlayBody.text = body;

            overlayVisible = true;
            overlayTargetAlpha = 1f;
            nextOverlayHideTime = Time.unscaledTime + Mathf.Max(0.8f, duration);
            demoOverlay.blocksRaycasts = true;
        }

        private void UpdateOverlay()
        {
            if (demoOverlay == null)
                return;

            if (overlayVisible && Time.unscaledTime >= nextOverlayHideTime)
            {
                overlayVisible = false;
                overlayTargetAlpha = 0f;
                demoOverlay.blocksRaycasts = false;
            }

            demoOverlay.alpha = Mathf.SmoothDamp(
                demoOverlay.alpha,
                overlayTargetAlpha,
                ref overlayFadeVelocity,
                0.18f,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }

        private float nextOverlayHideTime;

        private static CanvasGroup FindCanvasGroup(Transform root, string targetName)
        {
            Transform match = FindDescendant(root, targetName);
            return match == null ? null : match.GetComponent<CanvasGroup>();
        }

        private static TextMeshProUGUI FindText(Transform root, string targetName)
        {
            Transform match = FindDescendant(root, targetName);
            return match == null ? null : match.GetComponent<TextMeshProUGUI>();
        }

        private static bool TryFindModuleIndex(ModuleDefinitionSO[] modules, string moduleAssetName, out int moduleIndex)
        {
            moduleIndex = -1;
            if (modules == null || string.IsNullOrWhiteSpace(moduleAssetName))
                return false;

            for (int i = 0; i < modules.Length; i++)
            {
                if (string.Equals(modules[i]?.name, moduleAssetName, StringComparison.Ordinal))
                {
                    moduleIndex = i;
                    return true;
                }
            }

            return false;
        }

        private static int FindVariantIndex(ModuleDefinitionSO module, string variantAssetName)
        {
            VariantDefinitionSO[] variants = module?.Variants;
            if (variants == null || string.IsNullOrWhiteSpace(variantAssetName))
                return -1;

            for (int i = 0; i < variants.Length; i++)
            {
                if (string.Equals(variants[i]?.name, variantAssetName, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        private static Button FindButton(Transform root, string targetName)
        {
            Transform match = FindDescendant(root, targetName);
            return match == null ? null : match.GetComponent<Button>();
        }

        private static Transform FindDescendant(Transform root, string targetName)
        {
            if (root == null)
                return null;

            if (root.name == targetName)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDescendant(root.GetChild(i), targetName);
                if (match != null)
                    return match;
            }

            return null;
        }
    }
}
