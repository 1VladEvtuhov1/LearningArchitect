using UnityEngine;

namespace LearningArchitect.Core
{
    [CreateAssetMenu(menuName = "Learning Architect/Module Definition", fileName = "ModuleDefinition")]
    public sealed class ModuleDefinitionSO : ScriptableObject
    {
        public string moduleName;
        public string moduleNameRu;
        [TextArea] public string description;
        [TextArea] public string descriptionRu;
        public VariantDefinitionSO[] variants;

        public string GetModuleName(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(moduleNameRu))
                return moduleNameRu;

            return moduleName;
        }

        public string GetDescription(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(descriptionRu))
                return descriptionRu;

            return description;
        }
    }
}
