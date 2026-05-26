using System;
using UnityEngine;

namespace LearningArchitect.Core
{
    /// <summary>
    /// Authoring asset for a module variant: prefab, stress tuning, and stable localization id.
    /// All user-visible copy lives in Unity Localization (<c>ShowcaseContent</c>).
    /// Stress preset values are spawn counts (objects placed on the scene), not hidden simulation budgets.
    /// </summary>
    [CreateAssetMenu(menuName = "Learning Architect/Variant Definition", fileName = "VariantDefinition")]
    public sealed class VariantDefinitionSO : ScriptableObject
    {
        private static readonly int[] DefaultStressPresets = ShowcaseStressSpawn.DefaultPresets;

        [SerializeField]
        [Tooltip("Stable id for ShowcaseContent rows (not the asset file name).")]
        private string localizationKey;

        [SerializeField] private GameObject prefab;
        [SerializeField] private int[] stressPresets;

        public string LocalizationKey => localizationKey;
        public GameObject Prefab => prefab;
        public int[] StressPresets => stressPresets;

        public string GetLocalizationKey()
        {
            if (!string.IsNullOrWhiteSpace(localizationKey))
                return localizationKey;

            throw new InvalidOperationException($"{nameof(VariantDefinitionSO)} '{name}' requires an explicit {nameof(localizationKey)}.");
        }

        public int[] GetStressPresets()
        {
            if (stressPresets == null || stressPresets.Length == 0)
                return DefaultStressPresets;

            return stressPresets;
        }
    }
}
