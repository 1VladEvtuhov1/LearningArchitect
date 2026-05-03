using UnityEngine;

namespace LearningArchitect.Core
{
    [CreateAssetMenu(menuName = "Learning Architect/Variant Definition", fileName = "VariantDefinition")]
    public sealed class VariantDefinitionSO : ScriptableObject
    {
        private static readonly int[] DefaultStressPresets = { 1000, 5000, 10000 };

        [SerializeField] private string localizationKey;
        [SerializeField] private string variantName;
        [SerializeField] private string variantNameRu;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int[] stressPresets;
        [SerializeField] private string[] stressPresetLabels;
        [SerializeField] private string[] stressPresetLabelsRu;
        [SerializeField] [TextArea] private string architectureDescription;
        [SerializeField] [TextArea] private string architectureDescriptionRu;
        [SerializeField] [TextArea] private string dataFlow;
        [SerializeField] [TextArea] private string dataFlowRu;
        [SerializeField] [TextArea] private string runtimeLifecycle;
        [SerializeField] [TextArea] private string runtimeLifecycleRu;
        [SerializeField] [TextArea] private string whyThisApproach;
        [SerializeField] [TextArea] private string whyThisApproachRu;
        [SerializeField] [TextArea] private string compareSummary;
        [SerializeField] [TextArea] private string compareSummaryRu;
        [SerializeField] [TextArea] private string takeaway;
        [SerializeField] [TextArea] private string takeawayRu;
        [SerializeField] [TextArea] private string tradeOffs;
        [SerializeField] [TextArea] private string tradeOffsRu;
        [SerializeField] [TextArea] private string pros;
        [SerializeField] [TextArea] private string prosRu;
        [SerializeField] [TextArea] private string cons;
        [SerializeField] [TextArea] private string consRu;

        public string LocalizationKey => localizationKey;
        public string VariantName => variantName;
        public string VariantNameRu => variantNameRu;
        public GameObject Prefab => prefab;
        public string ArchitectureDescription => architectureDescription;
        public string ArchitectureDescriptionRu => architectureDescriptionRu;
        public string DataFlow => dataFlow;
        public string DataFlowRu => dataFlowRu;
        public string RuntimeLifecycle => runtimeLifecycle;
        public string RuntimeLifecycleRu => runtimeLifecycleRu;
        public string WhyThisApproach => whyThisApproach;
        public string WhyThisApproachRu => whyThisApproachRu;
        public string CompareSummary => compareSummary;
        public string CompareSummaryRu => compareSummaryRu;
        public string Takeaway => takeaway;
        public string TakeawayRu => takeawayRu;
        public string TradeOffs => tradeOffs;
        public string TradeOffsRu => tradeOffsRu;
        public string Pros => pros;
        public string ProsRu => prosRu;
        public string Cons => cons;
        public string ConsRu => consRu;
        public int[] StressPresets => stressPresets;

        public string GetLocalizationKey()
        {
            if (!string.IsNullOrWhiteSpace(localizationKey))
                return localizationKey;

            return name;
        }

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

        public string GetCompareSummary(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(compareSummaryRu))
                return compareSummaryRu;

            return compareSummary;
        }

        public string GetDataFlow(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(dataFlowRu))
                return dataFlowRu;

            return dataFlow;
        }

        public string GetRuntimeLifecycle(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(runtimeLifecycleRu))
                return runtimeLifecycleRu;

            return runtimeLifecycle;
        }

        public string GetWhyThisApproach(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(whyThisApproachRu))
                return whyThisApproachRu;

            return whyThisApproach;
        }

        public string GetTakeaway(ShowcaseLanguage language)
        {
            if (language == ShowcaseLanguage.Russian && !string.IsNullOrWhiteSpace(takeawayRu))
                return takeawayRu;

            return takeaway;
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

        public int[] GetStressPresets()
        {
            if (stressPresets == null || stressPresets.Length == 0)
                return DefaultStressPresets;

            return stressPresets;
        }

        public string[] GetStressPresetLabels(ShowcaseLanguage language)
        {
            string[] presets = language == ShowcaseLanguage.Russian
                ? stressPresetLabelsRu
                : stressPresetLabels;

            if (presets == null || presets.Length == 0)
                return null;

            if (presets.Length != GetStressPresets().Length)
                return null;

            return presets;
        }
    }
}
