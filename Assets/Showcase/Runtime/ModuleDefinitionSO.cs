using UnityEngine;

namespace LearningArchitect.Core
{
    public enum ShowcaseModuleCategory
    {
        SimulationModule = 0,
        ArchitecturePatternModule = 1
    }

    [CreateAssetMenu(menuName = "Learning Architect/Module Definition", fileName = "ModuleDefinition")]
    public sealed class ModuleDefinitionSO : ScriptableObject
    {
        [SerializeField] private string localizationKey;
        [SerializeField] private ShowcaseModuleCategory category = ShowcaseModuleCategory.SimulationModule;
        [SerializeField] private string categoryLabel;
        [SerializeField] private string categoryLabelRu;
        [SerializeField] private string moduleName;
        [SerializeField] private string moduleNameRu;
        [SerializeField] [TextArea] private string thesis;
        [SerializeField] [TextArea] private string thesisRu;
        [SerializeField] [TextArea] private string description;
        [SerializeField] [TextArea] private string descriptionRu;
        [SerializeField] [TextArea] private string problemStatement;
        [SerializeField] [TextArea] private string problemStatementRu;
        [SerializeField] [TextArea] private string webGlPresetNote;
        [SerializeField] [TextArea] private string webGlPresetNoteRu;
        [SerializeField] private string activeItemLabel;
        [SerializeField] private string activeItemLabelRu;
        [SerializeField] private VariantDefinitionSO[] variants;

        public string LocalizationKey => localizationKey;
        public ShowcaseModuleCategory Category => category;
        public string CategoryLabel => categoryLabel;
        public string CategoryLabelRu => categoryLabelRu;
        public string ModuleName => moduleName;
        public string ModuleNameRu => moduleNameRu;
        public string Thesis => thesis;
        public string ThesisRu => thesisRu;
        public string Description => description;
        public string DescriptionRu => descriptionRu;
        public string ProblemStatement => problemStatement;
        public string ProblemStatementRu => problemStatementRu;
        public string WebGlPresetNote => webGlPresetNote;
        public string WebGlPresetNoteRu => webGlPresetNoteRu;
        public string ActiveItemLabel => activeItemLabel;
        public string ActiveItemLabelRu => activeItemLabelRu;
        public VariantDefinitionSO[] Variants => variants;

        public string GetLocalizationKey()
        {
            if (!string.IsNullOrWhiteSpace(localizationKey))
                return localizationKey;

            return name;
        }

        public string GetModuleName(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(moduleNameRu))
                return moduleNameRu;

            return moduleName;
        }

        public string GetCategoryLabel(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(categoryLabelRu))
                return categoryLabelRu;

            if (!string.IsNullOrWhiteSpace(categoryLabel))
                return categoryLabel;

            return GetDefaultCategoryLabel(language);
        }

        public string GetDescription(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(descriptionRu))
                return descriptionRu;

            return description;
        }

        public string GetThesis(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(thesisRu))
                return thesisRu;

            return thesis;
        }

        public string GetProblemStatement(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(problemStatementRu))
                return problemStatementRu;

            return problemStatement;
        }

        public string GetWebGlPresetNote(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(webGlPresetNoteRu))
                return webGlPresetNoteRu;

            return webGlPresetNote;
        }

        public string GetActiveItemLabel(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(activeItemLabelRu))
                return activeItemLabelRu;

            return activeItemLabel;
        }

        private string GetDefaultCategoryLabel(ShowcaseLanguage language)
        {
            return category switch
            {
                ShowcaseModuleCategory.ArchitecturePatternModule => language == ShowcaseLanguage.Russian
                    ? "Архитектурный паттерн"
                    : "Architecture Pattern Module",
                _ => language == ShowcaseLanguage.Russian
                    ? "Симуляционный модуль"
                    : "Simulation Module"
            };
        }
    }
}
