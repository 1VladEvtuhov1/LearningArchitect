using System;
using UnityEngine;

namespace LearningArchitect.Core
{
    public enum ShowcaseModuleCategory
    {
        SimulationModule = 0,
        ArchitecturePatternModule = 1
    }

    /// <summary>
    /// Authoring asset for a showcase module: taxonomy, stable localization id, and variant list.
    /// All user-visible copy lives in Unity Localization (<c>ShowcaseContent</c> / <c>ShowcaseUI</c>).
    /// </summary>
    [CreateAssetMenu(menuName = "Learning Architect/Module Definition", fileName = "ModuleDefinition")]
    public sealed class ModuleDefinitionSO : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Stable id for ShowcaseContent / ShowcaseUI rows (not the asset file name).")]
        private string localizationKey;

        [SerializeField]
        [Tooltip("Used for navigation and for default module-type label keys in ShowcaseUI.")]
        private ShowcaseModuleCategory category = ShowcaseModuleCategory.SimulationModule;

        [SerializeField] private VariantDefinitionSO[] variants;

        public string LocalizationKey => localizationKey;
        public ShowcaseModuleCategory Category => category;
        public VariantDefinitionSO[] Variants => variants;

        public string GetLocalizationKey()
        {
            if (!string.IsNullOrWhiteSpace(localizationKey))
                return localizationKey;

            throw new InvalidOperationException($"{nameof(ModuleDefinitionSO)} '{name}' requires an explicit {nameof(localizationKey)}.");
        }
    }
}
