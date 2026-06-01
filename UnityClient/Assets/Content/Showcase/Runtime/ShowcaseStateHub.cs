using System;
using UnityEngine;

namespace LearningArchitect.Core
{
    [DisallowMultipleComponent]
    public sealed class ShowcaseStateHub : MonoBehaviour
    {
        public event Action<ModuleDefinitionSO, VariantDefinitionSO> SelectionChanged;
        public event Action<int, int> StressStateChanged;
        public event Action<ShowcaseMetricsSnapshot> MetricsChanged;

        public ModuleDefinitionSO CurrentModule { get; private set; }
        public VariantDefinitionSO CurrentVariant { get; private set; }
        public int CurrentStressLevel { get; private set; } = 1000;
        public int ActiveItemCount { get; private set; }
        public ShowcaseMetricsSnapshot CurrentMetrics { get; private set; } = ShowcaseMetricsSnapshot.Empty;
        public int ModuleCount { get; private set; }

        public bool CanSwitchModules => ModuleCount > 1;

        public bool CanSwitchVariants
        {
            get
            {
                return CurrentModule != null &&
                       CurrentModule.Variants != null &&
                       CurrentModule.Variants.Length > 1;
            }
        }

        public void SetModuleCount(int moduleCount)
        {
            ModuleCount = Mathf.Max(0, moduleCount);
        }

        public void PublishSelection(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (CurrentModule == module && CurrentVariant == variant)
                return;

            CurrentModule = module;
            CurrentVariant = variant;
            SelectionChanged?.Invoke(module, variant);
        }

        public void PublishStress(int stressLevel, int activeItemCount, ShowcaseMetricsSnapshot metrics)
        {
            stressLevel = Mathf.Max(1, stressLevel);
            activeItemCount = Mathf.Max(0, activeItemCount);

            if (CurrentStressLevel == stressLevel &&
                ActiveItemCount == activeItemCount &&
                CurrentMetrics == metrics)
                return;

            CurrentStressLevel = stressLevel;
            ActiveItemCount = activeItemCount;
            CurrentMetrics = metrics;
            StressStateChanged?.Invoke(stressLevel, activeItemCount);
            MetricsChanged?.Invoke(metrics);
        }

        public void ResetState()
        {
            CurrentModule = null;
            CurrentVariant = null;
            CurrentStressLevel = 1000;
            ActiveItemCount = 0;
            CurrentMetrics = ShowcaseMetricsSnapshot.Empty;
            ModuleCount = 0;
        }
    }
}
