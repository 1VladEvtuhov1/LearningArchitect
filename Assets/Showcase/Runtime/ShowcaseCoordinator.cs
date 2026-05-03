namespace LearningArchitect.Core
{
    public sealed class ShowcaseCoordinator
    {
        private readonly ModuleDefinitionSO[] modules;
        private readonly ModuleRuntimeHost runtimeHost;
        private readonly ShowcaseSelectionState selectionState;
        private readonly VariantSpawner spawner;
        private readonly ShowcaseStressState stressState;

        public ShowcaseCoordinator(
            ModuleDefinitionSO[] modules,
            ShowcaseSelectionState selectionState,
            ShowcaseStressState stressState,
            VariantSpawner spawner,
            ModuleRuntimeHost runtimeHost)
        {
            this.modules = modules;
            this.selectionState = selectionState;
            this.stressState = stressState;
            this.spawner = spawner;
            this.runtimeHost = runtimeHost;

            selectionState.Clamp(modules);
            stressState.SetActiveItemCount(runtimeHost.ActiveItemCount);
        }

        public int CurrentModuleIndex => selectionState.CurrentModuleIndex;

        public int ModuleCount => modules == null ? 0 : modules.Length;

        public int CurrentVariantIndex => selectionState.CurrentVariantIndex;

        public ModuleDefinitionSO CurrentModule
        {
            get
            {
                if (modules == null || modules.Length == 0)
                    return null;

                return modules[selectionState.CurrentModuleIndex];
            }
        }

        public VariantDefinitionSO CurrentVariant
        {
            get
            {
                ModuleDefinitionSO module = CurrentModule;
                if (module == null || module.Variants == null || module.Variants.Length == 0)
                    return null;

                return module.Variants[selectionState.CurrentVariantIndex];
            }
        }

        public int CurrentStressLevel => stressState.CurrentStressLevel;

        public int ActiveItemCount => stressState.ActiveItemCount;

        public ShowcaseMetricsSnapshot CurrentMetrics => stressState.MetricsSnapshot;

        public bool HasModules() => ModuleCount > 0;

        public bool CanSwitchModules() => ModuleCount > 1;

        public bool CanSwitchCategories()
        {
            if (modules == null || modules.Length == 0)
                return false;

            ShowcaseModuleCategory? firstCategory = null;
            for (int i = 0; i < modules.Length; i++)
            {
                ModuleDefinitionSO module = modules[i];
                if (module == null)
                    continue;

                if (firstCategory == null)
                {
                    firstCategory = module.Category;
                    continue;
                }

                if (module.Category != firstCategory.Value)
                    return true;
            }

            return false;
        }

        public bool CanSwitchVariants()
        {
            ModuleDefinitionSO module = CurrentModule;
            return module != null && module.Variants != null && module.Variants.Length > 1;
        }

        public void MoveToNextModule()
        {
            if (!HasModules())
                return;

            selectionState.MoveToNextModule(modules.Length);
        }

        public void MoveToPreviousModule()
        {
            if (!HasModules())
                return;

            selectionState.MoveToPreviousModule(modules.Length);
        }

        public void MoveToNextVariant()
        {
            int variantCount = GetCurrentVariantCount();
            if (variantCount == 0)
                return;

            selectionState.MoveToNextVariant(variantCount);
        }

        public void MoveToPreviousVariant()
        {
            int variantCount = GetCurrentVariantCount();
            if (variantCount == 0)
                return;

            selectionState.MoveToPreviousVariant(variantCount);
        }

        public void MoveToNextCategory()
        {
            if (!HasModules())
                return;

            int nextIndex = FindNextCategoryIndex(CurrentModule?.Category ?? ShowcaseModuleCategory.SimulationModule);
            if (nextIndex < 0)
                return;

            selectionState.SetSelection(nextIndex, 0, modules);
        }

        public void Select(int moduleIndex, int variantIndex)
        {
            if (!HasModules())
                return;

            selectionState.SetSelection(moduleIndex, variantIndex, modules);
        }

        public void SetStressLevel(int count)
        {
            stressState.SetStressLevel(NormalizeRequestedStressLevel(count));
            runtimeHost.SetStressLevel(stressState.CurrentStressLevel);
            SyncRuntimeState();
        }

        public void ActivateCurrentVariant()
        {
            stressState.SetStressLevel(RemapStressLevelForCurrentVariant());
            runtimeHost.Deactivate();
            spawner.Despawn();

            var handle = spawner.Spawn(CurrentVariant);
            runtimeHost.Activate(handle);
            runtimeHost.SetStressLevel(stressState.CurrentStressLevel);
            SyncRuntimeState();
        }

        public void DeactivateCurrentVariant()
        {
            runtimeHost.Deactivate();
            spawner.Despawn();
            stressState.SetMetricsSnapshot(ShowcaseMetricsSnapshot.Empty);
        }

        public void SyncRuntimeState()
        {
            stressState.SetMetricsSnapshot(runtimeHost.GetMetricsSnapshot(stressState.CurrentStressLevel));
        }

        private int GetCurrentVariantCount()
        {
            ModuleDefinitionSO module = CurrentModule;
            if (module == null || module.Variants == null)
            {
                return 0;
            }

            return module.Variants.Length;
        }

        private int NormalizeRequestedStressLevel(int requestedCount)
        {
            int[] presets = CurrentVariant?.GetStressPresets();
            if (presets == null || presets.Length == 0)
            {
                stressState.SetSelectedPresetIndex(-1);
                return requestedCount;
            }

            int presetIndex = FindPresetIndex(presets, requestedCount);
            if (presetIndex >= 0)
            {
                stressState.SetSelectedPresetIndex(presetIndex);
                return requestedCount;
            }

            stressState.SetSelectedPresetIndex(0);
            return presets[0];
        }

        private int RemapStressLevelForCurrentVariant()
        {
            int currentStressLevel = stressState.CurrentStressLevel;
            int[] presets = CurrentVariant?.GetStressPresets();
            if (presets == null || presets.Length == 0)
            {
                stressState.SetSelectedPresetIndex(-1);
                return currentStressLevel;
            }

            int exactPresetIndex = FindPresetIndex(presets, currentStressLevel);
            if (exactPresetIndex >= 0)
            {
                stressState.SetSelectedPresetIndex(exactPresetIndex);
                return currentStressLevel;
            }

            int selectedPresetIndex = stressState.SelectedPresetIndex;
            if (selectedPresetIndex >= 0)
            {
                int remappedIndex = selectedPresetIndex < presets.Length
                    ? selectedPresetIndex
                    : presets.Length - 1;
                stressState.SetSelectedPresetIndex(remappedIndex);
                return presets[remappedIndex];
            }

            stressState.SetSelectedPresetIndex(0);
            return presets[0];
        }

        private static int FindPresetIndex(int[] presets, int count)
        {
            if (presets == null)
                return -1;

            for (int i = 0; i < presets.Length; i++)
            {
                if (presets[i] == count)
                    return i;
            }

            return -1;
        }

        private int FindNextCategoryIndex(ShowcaseModuleCategory currentCategory)
        {
            if (modules == null || modules.Length == 0)
                return -1;

            for (int i = 1; i <= modules.Length; i++)
            {
                int index = (selectionState.CurrentModuleIndex + i) % modules.Length;
                ModuleDefinitionSO module = modules[index];
                if (module != null && module.Category != currentCategory)
                    return index;
            }

            return -1;
        }
    }
}
