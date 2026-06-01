using System;
using LearningArchitect.UI;
using UnityEngine;

namespace LearningArchitect.Core
{
    public enum RecruiterDemoStressMode
    {
        LowestPreset = 0,
        MediumPreset = 1,
        HighestPreset = 2,
        ExplicitValue = 3
    }

    [CreateAssetMenu(menuName = "Learning Architect/Recruiter Demo Scenario", fileName = "RecruiterDemoScenario")]
    public sealed class RecruiterDemoScenarioSO : ScriptableObject
    {
        [Serializable]
        public struct CameraCue
        {
            [SerializeField] private bool enabled;
            [SerializeField] private float yawOffset;
            [SerializeField] private float pitchOffset;
            [SerializeField] private float distanceOffset;

            public bool Enabled => enabled;
            public float YawOffset => yawOffset;
            public float PitchOffset => pitchOffset;
            public float DistanceOffset => distanceOffset;
        }

        [Serializable]
        public struct StepDefinition
        {
            [SerializeField] private ModuleDefinitionSO module;
            [SerializeField] private VariantDefinitionSO variant;
            [SerializeField] private RecruiterDemoStressMode stressMode;
            [SerializeField] private int explicitStressLevel;
            [SerializeField] private float duration;
            [SerializeField] private string noteLocalizationKey;
            [SerializeField] private CameraCue cameraCue;

            public ModuleDefinitionSO Module => module;
            public VariantDefinitionSO Variant => variant;
            public RecruiterDemoStressMode StressMode => stressMode;
            public int ExplicitStressLevel => explicitStressLevel;
            public float Duration => duration;
            public string NoteLocalizationKey => noteLocalizationKey;
            public CameraCue Cue => cameraCue;
            public bool HasNoteLocalizationKey => !string.IsNullOrWhiteSpace(noteLocalizationKey);

            public string GetNote(ShowcaseLanguage language)
            {
                if (!HasNoteLocalizationKey)
                    return ShowcaseLocalizationContent.BuildMissingMarker("ShowcaseContent.recruiter_demo.note");

                return ShowcaseLocalizationContent.GetRequiredContentText(noteLocalizationKey, language);
            }
        }

        [Serializable]
        public struct SelectionCueDefinition
        {
            [SerializeField] private VariantDefinitionSO variant;
            [SerializeField] private CameraCue cameraCue;

            public VariantDefinitionSO Variant => variant;
            public CameraCue Cue => cameraCue;
        }

        [SerializeField] private float defaultStepDuration = 6.5f;
        [SerializeField] private float introOverlayDuration = 1.8f;
        [SerializeField] private float outroOverlayDuration = 2.2f;
        [SerializeField] private StepDefinition[] steps = Array.Empty<StepDefinition>();
        [SerializeField] private SelectionCueDefinition[] selectionCues = Array.Empty<SelectionCueDefinition>();

        public float DefaultStepDuration => defaultStepDuration;
        public float IntroOverlayDuration => introOverlayDuration;
        public float OutroOverlayDuration => outroOverlayDuration;
        public StepDefinition[] Steps => steps;
        public SelectionCueDefinition[] SelectionCues => selectionCues;

        public bool TryGetSelectionCue(VariantDefinitionSO variant, out CameraCue cue)
        {
            if (variant != null)
            {
                for (int i = 0; i < selectionCues.Length; i++)
                {
                    if (selectionCues[i].Variant != variant)
                        continue;

                    cue = selectionCues[i].Cue;
                    return cue.Enabled;
                }
            }

            cue = default;
            return false;
        }
    }
}
