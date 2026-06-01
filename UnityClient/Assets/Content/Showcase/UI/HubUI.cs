using LearningArchitect.Core;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LearningArchitect.UI
{
    public sealed class HubUI : MonoBehaviour
    {
        [Serializable]
        private struct ModuleStatBinding
        {
            public RectTransform root;
            public TextMeshProUGUI label;
            public TextMeshProUGUI value;
        }

        [SerializeField] private TextMeshProUGUI moduleName;
        [SerializeField] private TextMeshProUGUI variantName;
        [SerializeField] private TextMeshProUGUI moduleSelectorName;
        [SerializeField] private TextMeshProUGUI variantSelectorName;
        [Header("Module Stats")]
        [SerializeField] private RectTransform moduleStatsContainer;
        [SerializeField] private ModuleStatBinding[] moduleStats = Array.Empty<ModuleStatBinding>();

        [Header("Feedback")]
        [SerializeField] private Color moduleColor = default;
        [SerializeField] private Color variantColor = default;
        [SerializeField] private Color pulseColor = default;
        [SerializeField] private float pulseDuration = 0.18f;
        [SerializeField] private float pulseScale = 1.08f;

        private float modulePulse;
        private string moduleSelectorOverrideText = string.Empty;
        private Vector3 moduleBaseScale = Vector3.one;
        private float variantPulse;
        private string variantSelectorOverrideText = string.Empty;
        private Vector3 variantBaseScale = Vector3.one;

        public TextMeshProUGUI ModuleName
        {
            get => moduleName;
            set => moduleName = value;
        }

        public TextMeshProUGUI VariantName
        {
            get => variantName;
            set => variantName = value;
        }

        public TextMeshProUGUI ModuleSelectorName
        {
            get => moduleSelectorName;
            set => moduleSelectorName = value;
        }

        public TextMeshProUGUI VariantSelectorName
        {
            get => variantSelectorName;
            set => variantSelectorName = value;
        }

        public Color ModuleColor
        {
            get => moduleColor;
            set => moduleColor = value;
        }

        public Color VariantColor
        {
            get => variantColor;
            set => variantColor = value;
        }

        public Color PulseColor
        {
            get => pulseColor;
            set => pulseColor = value;
        }

        private void Awake()
        {
            InitializeLayout();
        }

        /// <summary>EditMode tests: runs the same setup as <see cref="Awake"/> without relying on reflection.</summary>
        internal void RunInitializeForEditModeTests()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            ApplyPaletteDefaults();
            Validate();

            moduleBaseScale = moduleName.rectTransform.localScale;
            variantBaseScale = variantName.rectTransform.localScale;
        }

        private void OnValidate()
        {
            ApplyPaletteDefaults();
        }

        private void Update()
        {
            AnimateText(moduleName, moduleBaseScale, ref modulePulse, moduleColor);
            AnimateText(variantName, variantBaseScale, ref variantPulse, variantColor);
        }

        public void ShowSelection(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            string moduleTitle = ShowcaseLocalization.GetModuleName(module);
            string moduleCategory = ShowcaseLocalization.GetModuleCategory(module);
            moduleName.text = BuildModuleTitleMarkup(moduleCategory, moduleTitle);
            moduleName.color = moduleColor;

            string variantTitle = ShowcaseLocalization.GetVariantName(variant);
            variantName.text = "<b>" + variantTitle + "</b>";
            variantName.color = variantColor;

            moduleSelectorName.text = string.IsNullOrWhiteSpace(moduleSelectorOverrideText)
                ? ShowcaseLocalization.GetModuleName(module)
                : moduleSelectorOverrideText;
            moduleSelectorName.color = moduleColor;

            variantSelectorName.text = string.IsNullOrWhiteSpace(variantSelectorOverrideText)
                ? ShowcaseLocalization.GetVariantName(variant)
                : variantSelectorOverrideText;
            variantSelectorName.color = moduleColor;

            ApplyModuleStats(module, variant);
        }

        public void SetSelectorContext(string moduleSelectorText, string variantSelectorText)
        {
            moduleSelectorOverrideText = moduleSelectorText ?? string.Empty;
            variantSelectorOverrideText = variantSelectorText ?? string.Empty;

            if (moduleSelectorName != null && !string.IsNullOrWhiteSpace(moduleSelectorOverrideText))
                moduleSelectorName.text = moduleSelectorOverrideText;

            if (variantSelectorName != null && !string.IsNullOrWhiteSpace(variantSelectorOverrideText))
                variantSelectorName.text = variantSelectorOverrideText;
        }

        public void PlayModuleSwitchFeedback(int direction)
        {
            modulePulse = pulseDuration;
        }

        public void PlayVariantSwitchFeedback(int direction)
        {
            variantPulse = pulseDuration;
        }

        public void PlayCategorySwitchFeedback()
        {
            modulePulse = pulseDuration;
        }

        private void AnimateText(TextMeshProUGUI target, Vector3 baseScale, ref float timer, Color baseColor)
        {
            if (target == null)
                return;

            if (timer > 0f)
                timer -= Time.unscaledDeltaTime;

            float normalized = pulseDuration <= 0f ? 0f : Mathf.Clamp01(timer / pulseDuration);
            float eased = normalized * normalized * (3f - 2f * normalized);
            float scale = Mathf.Lerp(1f, pulseScale, eased);

            target.rectTransform.localScale = baseScale * scale;
            target.color = Color.Lerp(baseColor, pulseColor, eased);
        }

        private void ApplyPaletteDefaults()
        {
            if (moduleColor == default)
                moduleColor = ShowcasePalette.TextPrimary;

            if (variantColor == default)
                variantColor = ShowcasePalette.AccentMain;

            if (pulseColor == default)
                pulseColor = ShowcasePalette.AccentStrong;
        }

        private void Validate()
        {
            if (moduleName == null || variantName == null || moduleSelectorName == null || variantSelectorName == null)
                throw new InvalidOperationException($"{nameof(HubUI)} requires all primary text references to be assigned.");

            for (int i = 0; i < moduleStats.Length; i++)
            {
                if (moduleStats[i].root == null || moduleStats[i].label == null || moduleStats[i].value == null)
                    throw new InvalidOperationException($"{nameof(HubUI)} requires all module stat bindings to be assigned.");
            }
        }

        private void ApplyModuleStats(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            if (moduleStats == null || moduleStats.Length == 0)
                return;

            string[] labels =
            {
                ShowcaseLocalization.GetText("module"),
                ShowcaseLocalization.GetText("module_type"),
                ShowcaseLocalization.GetText("variant"),
                ShowcaseLocalization.GetText("variants"),
                ShowcaseLocalization.GetText("active_items")
            };

            string[] values =
            {
                ShowcaseLocalization.GetModuleName(module),
                GetStatValueOrDefault(ShowcaseLocalization.GetModuleCategory(module)),
                ShowcaseLocalization.GetVariantName(variant),
                BuildVariantProgressValue(module, variant),
                BuildActiveItemsValue(module)
            };

            for (int i = 0; i < moduleStats.Length; i++)
            {
                ModuleStatBinding binding = moduleStats[i];
                bool hasData = i < labels.Length;

                if (binding.root != null)
                    binding.root.gameObject.SetActive(hasData);

                if (!hasData)
                    continue;

                if (binding.label != null)
                    binding.label.text = labels[i];

                if (binding.value != null)
                    binding.value.text = values[i];
            }
        }

        private string BuildVariantProgressValue(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            VariantDefinitionSO[] variants = module?.Variants;
            if (variants == null || variants.Length == 0)
                return ShowcaseLocalization.GetText("na");

            int currentIndex = FindVariantIndex(variants, variant);
            return (currentIndex + 1) + "/" + variants.Length;
        }

        private static int FindVariantIndex(VariantDefinitionSO[] variants, VariantDefinitionSO variant)
        {
            if (variants == null || variants.Length == 0)
                return 0;

            if (variant == null)
                return 0;

            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i] == variant)
                    return i;
            }

            return 0;
        }

        private static string BuildActiveItemsValue(ModuleDefinitionSO module)
        {
            if (module == null)
                return ShowcaseLocalization.GetText("na");

            return GetStatValueOrDefault(ShowcaseLocalization.GetModuleActiveItemLabel(module));
        }

        private static string GetStatValueOrDefault(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ShowcaseLocalization.GetText("na")
                : value;
        }

        private static string BuildModuleTitleMarkup(string category, string title)
        {
            if (string.IsNullOrWhiteSpace(category))
                return "<b>" + title + "</b>";

            return "<size=52%><color=#" + ShowcasePalette.TextMutedHex + "><b>" +
                   category.ToUpperInvariant() +
                   "</b></color></size>\n<b>" +
                   title +
                   "</b>";
        }
    }
}
