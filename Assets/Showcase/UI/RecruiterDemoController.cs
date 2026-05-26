using System;
using System.Collections.Generic;
using LearningArchitect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShowcaseCommandRouter))]
    [RequireComponent(typeof(ShowcaseCompositionRoot))]
    [RequireComponent(typeof(ShowcaseRuntimeController))]
    [RequireComponent(typeof(ShowcaseStateHub))]
    [RequireComponent(typeof(HubUI))]
    public sealed class RecruiterDemoController : MonoBehaviour
    {
        [SerializeField] private ShowcaseCommandRouter commands;
        [SerializeField] private ShowcaseCompositionRoot compositionRoot;
        [SerializeField] private ShowcaseRuntimeController runtimeController;
        [SerializeField] private ShowcaseStateHub stateHub;
        [SerializeField] private HubUI hubUI;
        [SerializeField] private RecruiterDemoScenarioSO scenario;
        [SerializeField] private Button demoButton;
        [SerializeField] private TextMeshProUGUI demoButtonLabel;
        [SerializeField] private CanvasGroup demoOverlay;
        [SerializeField] private TextMeshProUGUI demoOverlayTitle;
        [SerializeField] private TextMeshProUGUI demoOverlayBody;

        private readonly HashSet<VariantDefinitionSO> _duplicateSelectionCueGuard = new();

        private HubCameraDrift _cameraDrift;
        private CompiledDemoStep[] _compiledSteps = Array.Empty<CompiledDemoStep>();
        private int _currentStepIndex = -1;
        private bool _isRunning;
        private bool _overlayVisible;
        private float _overlayFadeVelocity;
        private float _overlayTargetAlpha;
        private float _nextAdvanceTime;
        private float _nextOverlayHideTime;

        private void Awake()
        {
            commands = commands != null ? commands : GetComponent<ShowcaseCommandRouter>();
            compositionRoot = compositionRoot != null ? compositionRoot : GetComponent<ShowcaseCompositionRoot>();
            runtimeController = runtimeController != null ? runtimeController : GetComponent<ShowcaseRuntimeController>();
            stateHub = stateHub != null ? stateHub : GetComponent<ShowcaseStateHub>();
            hubUI = hubUI != null ? hubUI : GetComponent<HubUI>();

            ValidateConfiguration();

            _cameraDrift = FindFirstObjectByType<HubCameraDrift>();
            CompileScenario();
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

        private void Update()
        {
            UpdateOverlay();

            if (!_isRunning || _compiledSteps.Length == 0)
                return;

            if (Time.unscaledTime >= _nextAdvanceTime)
                AdvanceToNextStep();
        }

        private void OnDisable()
        {
            if (demoButton != null)
                demoButton.onClick.RemoveListener(ToggleDemo);

            stateHub.SelectionChanged -= HandleSelectionChanged;
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
        }

        private void ValidateConfiguration()
        {
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

            if (scenario == null)
                throw new InvalidOperationException($"{nameof(RecruiterDemoController)} requires {nameof(scenario)}.");

            ValidateUiReferences();
            ValidateScenarioReferences();
        }

        private void ValidateUiReferences()
        {
            if (demoButton != null && demoButtonLabel == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(RecruiterDemoController)} requires {nameof(demoButtonLabel)} when {nameof(demoButton)} is assigned.");
            }

            if (demoOverlay != null && (demoOverlayTitle == null || demoOverlayBody == null))
            {
                throw new InvalidOperationException(
                    $"{nameof(RecruiterDemoController)} requires overlay title and body when {nameof(demoOverlay)} is assigned.");
            }
        }

        private void ValidateScenarioReferences()
        {
            ModuleDefinitionSO[] modules = compositionRoot.Modules;
            if (modules == null || modules.Length == 0)
                throw new InvalidOperationException($"{nameof(RecruiterDemoController)} requires composition root modules.");

            RecruiterDemoScenarioSO.StepDefinition[] steps = scenario.Steps;
            if (steps == null || steps.Length == 0)
                throw new InvalidOperationException($"{nameof(RecruiterDemoScenarioSO)} requires at least one step.");

            for (int i = 0; i < steps.Length; i++)
                ValidateScenarioStep(modules, steps[i], i);

            _duplicateSelectionCueGuard.Clear();
            RecruiterDemoScenarioSO.SelectionCueDefinition[] selectionCues = scenario.SelectionCues;
            for (int i = 0; i < selectionCues.Length; i++)
            {
                VariantDefinitionSO variant = selectionCues[i].Variant;
                if (variant == null)
                    throw new InvalidOperationException($"Selection cue {i} requires a variant reference.");

                if (!_duplicateSelectionCueGuard.Add(variant))
                    throw new InvalidOperationException($"Selection cue {i} duplicates variant '{variant.name}'.");
            }
        }

        private void ValidateScenarioStep(ModuleDefinitionSO[] modules, RecruiterDemoScenarioSO.StepDefinition step, int index)
        {
            if (step.Module == null)
                throw new InvalidOperationException($"Recruiter demo step {index} requires a module reference.");

            if (step.Variant == null)
                throw new InvalidOperationException($"Recruiter demo step {index} requires a variant reference.");

            if (!TryFindModuleIndex(modules, step.Module, out _))
            {
                throw new InvalidOperationException(
                    $"Recruiter demo step {index} references module '{step.Module.name}', which is not part of the showcase composition root.");
            }

            if (FindVariantIndex(step.Module, step.Variant) < 0)
            {
                throw new InvalidOperationException(
                    $"Recruiter demo step {index} references variant '{step.Variant.name}', which is not owned by module '{step.Module.name}'.");
            }

            if (!step.HasNoteLocalizationKey)
                throw new InvalidOperationException($"Recruiter demo step {index} requires an explicit note localization key.");

            if (step.StressMode == RecruiterDemoStressMode.ExplicitValue && step.ExplicitStressLevel <= 0)
                throw new InvalidOperationException($"Recruiter demo step {index} requires a positive explicit stress level.");
        }

        private void CompileScenario()
        {
            ModuleDefinitionSO[] modules = compositionRoot.Modules;
            RecruiterDemoScenarioSO.StepDefinition[] steps = scenario.Steps;
            List<CompiledDemoStep> compiled = new(steps.Length);

            for (int i = 0; i < steps.Length; i++)
            {
                RecruiterDemoScenarioSO.StepDefinition definition = steps[i];
                TryFindModuleIndex(modules, definition.Module, out int moduleIndex);
                int variantIndex = FindVariantIndex(definition.Module, definition.Variant);
                int stressLevel = ResolveStressLevel(definition);
                compiled.Add(new CompiledDemoStep(definition, moduleIndex, variantIndex, stressLevel));
            }

            _compiledSteps = compiled.ToArray();
        }

        private int ResolveStressLevel(RecruiterDemoScenarioSO.StepDefinition definition)
        {
            int[] presets = definition.Variant.GetStressPresets();
            if (presets == null || presets.Length == 0)
                throw new InvalidOperationException($"Variant '{definition.Variant.name}' does not expose stress presets.");

            return definition.StressMode switch
            {
                RecruiterDemoStressMode.LowestPreset => presets[0],
                RecruiterDemoStressMode.HighestPreset => presets[presets.Length - 1],
                RecruiterDemoStressMode.ExplicitValue => definition.ExplicitStressLevel,
                _ => presets.Length >= 2 ? presets[1] : presets[0]
            };
        }

        private void ToggleDemo()
        {
            if (_isRunning)
            {
                StopDemo(false);
                return;
            }

            StartDemo();
        }

        private void StartDemo()
        {
            CompileScenario();
            if (_compiledSteps.Length == 0)
                return;

            _isRunning = true;
            _currentStepIndex = -1;
            ShowOverlay(
                ShowcaseLocalization.GetText("recruiter_demo_title"),
                ShowcaseLocalization.GetText("recruiter_demo_intro"),
                scenario.IntroOverlayDuration);
            AdvanceToStep(0);
            RefreshButtonLabel();
        }

        private void StopDemo(bool completed)
        {
            _isRunning = false;
            _nextAdvanceTime = 0f;
            _currentStepIndex = -1;
            _overlayVisible = false;
            _overlayTargetAlpha = 0f;

            RefreshCurrentSelection();
            ResetCameraCue();

            if (completed)
            {
                ShowOverlay(
                    ShowcaseLocalization.GetText("demo_complete_title"),
                    ShowcaseLocalization.GetText("demo_complete_body"),
                    scenario.OutroOverlayDuration);
            }

            RefreshButtonLabel();
        }

        private void AdvanceToNextStep()
        {
            int nextIndex = _currentStepIndex + 1;
            if (nextIndex >= _compiledSteps.Length)
            {
                StopDemo(true);
                return;
            }

            AdvanceToStep(nextIndex);
        }

        private void AdvanceToStep(int stepIndex)
        {
            if ((uint)stepIndex >= (uint)_compiledSteps.Length)
                return;

            _currentStepIndex = stepIndex;
            CompiledDemoStep step = _compiledSteps[stepIndex];
            commands.LoadSelection(step.ModuleIndex, step.VariantIndex);
            commands.SetStressLevel(step.StressLevel);
            _nextAdvanceTime = Time.unscaledTime + Mathf.Max(2.5f, ResolveDuration(step.Definition));
            ApplyCameraCue(step.Definition.Cue);
            RefreshCurrentSelection();
            RefreshButtonLabel();
        }

        private float ResolveDuration(RecruiterDemoScenarioSO.StepDefinition definition)
        {
            if (definition.Duration > 0f)
                return definition.Duration;

            return scenario.DefaultStepDuration;
        }

        private void HandleSelectionChanged(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (!_isRunning)
            {
                ApplySelectionCameraCue(variant);
                return;
            }

            RefreshCurrentSelection();
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            if (_isRunning)
                RefreshCurrentSelection();

            RefreshButtonLabel();
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
                                   (_isRunning
                                       ? ShowcaseLocalization.GetText("stop_demo")
                                       : ShowcaseLocalization.GetText("start_demo")) +
                                   "</b>";
        }

        private void ApplyCameraCue(RecruiterDemoScenarioSO.CameraCue cue)
        {
            if (_cameraDrift == null)
                return;

            if (!cue.Enabled)
            {
                _cameraDrift.ResetDemoCue();
                return;
            }

            _cameraDrift.ApplyDemoCue(cue.YawOffset, cue.PitchOffset, cue.DistanceOffset);
        }

        private void ApplySelectionCameraCue(VariantDefinitionSO variant)
        {
            if (_cameraDrift == null)
                return;

            if (scenario.TryGetSelectionCue(variant, out RecruiterDemoScenarioSO.CameraCue cue))
            {
                ApplyCameraCue(cue);
                return;
            }

            _cameraDrift.ResetDemoCue();
        }

        private void ResetCameraCue()
        {
            if (_cameraDrift == null)
                return;

            _cameraDrift.ResetDemoCue();
        }

        private void ShowOverlay(string title, string body, float duration)
        {
            if (demoOverlay == null)
                return;

            if (demoOverlayTitle != null)
                demoOverlayTitle.text = title;

            if (demoOverlayBody != null)
                demoOverlayBody.text = body;

            _overlayVisible = true;
            _overlayTargetAlpha = 1f;
            _nextOverlayHideTime = Time.unscaledTime + Mathf.Max(0.8f, duration);
            demoOverlay.blocksRaycasts = true;
        }

        private void UpdateOverlay()
        {
            if (demoOverlay == null)
                return;

            if (_overlayVisible && Time.unscaledTime >= _nextOverlayHideTime)
            {
                _overlayVisible = false;
                _overlayTargetAlpha = 0f;
                demoOverlay.blocksRaycasts = false;
            }

            demoOverlay.alpha = Mathf.SmoothDamp(
                demoOverlay.alpha,
                _overlayTargetAlpha,
                ref _overlayFadeVelocity,
                0.18f,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }

        private static bool TryFindModuleIndex(ModuleDefinitionSO[] modules, ModuleDefinitionSO module, out int moduleIndex)
        {
            moduleIndex = -1;
            if (modules == null || module == null)
                return false;

            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] != module)
                    continue;

                moduleIndex = i;
                return true;
            }

            return false;
        }

        private static int FindVariantIndex(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            VariantDefinitionSO[] variants = module?.Variants;
            if (variants == null || variant == null)
                return -1;

            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i] == variant)
                    return i;
            }

            return -1;
        }

        public void ToggleDemoExternal()
        {
            ToggleDemo();
        }

        private readonly struct CompiledDemoStep
        {
            public CompiledDemoStep(
                RecruiterDemoScenarioSO.StepDefinition definition,
                int moduleIndex,
                int variantIndex,
                int stressLevel)
            {
                Definition = definition;
                ModuleIndex = moduleIndex;
                VariantIndex = variantIndex;
                StressLevel = stressLevel;
            }

            public RecruiterDemoScenarioSO.StepDefinition Definition { get; }
            public int ModuleIndex { get; }
            public int VariantIndex { get; }
            public int StressLevel { get; }
        }
    }
}
