using UnityEngine;

namespace LearningArchitect.Core
{
    [CreateAssetMenu(menuName = "Learning Architect/Variant Definition", fileName = "VariantDefinition")]
    public sealed class VariantDefinitionSO : ScriptableObject
    {
        public string variantName;
        public string variantNameRu;
        public GameObject prefab;
        [TextArea] public string architectureDescription;
        [TextArea] public string architectureDescriptionRu;
        [TextArea] public string tradeOffs;
        [TextArea] public string tradeOffsRu;
        [TextArea] public string pros;
        [TextArea] public string prosRu;
        [TextArea] public string cons;
        [TextArea] public string consRu;

        public string GetVariantName(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(variantNameRu))
                return variantNameRu;

            return variantName;
        }

        public string GetArchitectureDescription(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(architectureDescriptionRu))
                return architectureDescriptionRu;

            return architectureDescription;
        }

        public string GetTradeOffs(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(tradeOffsRu))
                return tradeOffsRu;

            return tradeOffs;
        }

        public string GetPros(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(prosRu))
                return prosRu;

            return pros;
        }

        public string GetCons(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(consRu))
                return consRu;

            return cons;
        }
    }
}
