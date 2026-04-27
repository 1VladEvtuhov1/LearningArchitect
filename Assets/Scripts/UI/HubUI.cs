using LearningArchitect.Core;
using TMPro;
using UnityEngine;

namespace LearningArchitect.UI
{
    public sealed class HubUI : MonoBehaviour
    {
        public TextMeshProUGUI moduleName;
        public TextMeshProUGUI variantName;
        public TextMeshProUGUI inputHints;
        public TextMeshProUGUI moduleSelectorName;
        public TextMeshProUGUI variantSelectorName;

        [Header("Feedback")]
        public Color moduleColor = default;
        public Color variantColor = default;
        public Color hintColor = default;
        public Color pulseColor = default;
        public float pulseDuration = 0.18f;
        public float pulseScale = 1.08f;

        private Vector3 moduleBaseScale = Vector3.one;
        private Vector3 variantBaseScale = Vector3.one;
        private Vector3 hintsBaseScale = Vector3.one;
        private float modulePulse;
        private float variantPulse;
        private float hintsPulse;
        private ModuleDefinitionSO currentModule;
        private VariantDefinitionSO currentVariant;

        private void Awake()
        {
            ApplyPaletteDefaults();

            if (moduleName != null)
                moduleBaseScale = moduleName.rectTransform.localScale;

            if (variantName != null)
                variantBaseScale = variantName.rectTransform.localScale;

            if (inputHints != null)
                hintsBaseScale = inputHints.rectTransform.localScale;
        }

        private void OnEnable()
        {
            ShowcaseLocalization.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            ShowcaseLocalization.LanguageChanged -= HandleLanguageChanged;
        }

        private void Update()
        {
            AnimateText(moduleName, moduleBaseScale, ref modulePulse, moduleColor);
            AnimateText(variantName, variantBaseScale, ref variantPulse, variantColor);
            AnimateText(inputHints, hintsBaseScale, ref hintsPulse, hintColor);
        }

        public void SetModule(ModuleDefinitionSO module, VariantDefinitionSO variant)
        {
            currentModule = module;
            currentVariant = variant;
            RefreshTexts();
        }

        private void RefreshTexts()
        {
            if (moduleName != null)
            {
                string moduleTitle = ShowcaseLocalization.GetModuleName(currentModule);
                moduleName.text = "<b>" + moduleTitle + "</b>";
                moduleName.color = moduleColor;
            }

            if (variantName != null)
            {
                string variantTitle = ShowcaseLocalization.GetVariantName(currentVariant);
                variantName.text = "<b>" + variantTitle + "</b>";
                variantName.color = variantColor;
            }

            if (inputHints != null)
            {
                inputHints.text = ShowcaseLocalization.GetText("realtime_preview");
                inputHints.color = hintColor;
            }

            if (moduleSelectorName != null)
            {
                moduleSelectorName.text = ShowcaseLocalization.GetModuleName(currentModule);
                moduleSelectorName.color = moduleColor;
            }

            if (variantSelectorName != null)
            {
                variantSelectorName.text = ShowcaseLocalization.GetVariantName(currentVariant);
                variantSelectorName.color = moduleColor;
            }
        }

        public void PlayModuleSwitchFeedback(int direction)
        {
            modulePulse = pulseDuration;
            hintsPulse = pulseDuration;

            if (inputHints != null)
                inputHints.text = direction < 0 ? ShowcaseLocalization.GetText("switching_module_prev") : ShowcaseLocalization.GetText("switching_module_next");
        }

        public void PlayVariantSwitchFeedback(int direction)
        {
            variantPulse = pulseDuration;
            hintsPulse = pulseDuration;

            if (inputHints != null)
                inputHints.text = direction < 0 ? ShowcaseLocalization.GetText("switching_variant_prev") : ShowcaseLocalization.GetText("switching_variant_next");
        }

        private void HandleLanguageChanged(ShowcaseLanguage language)
        {
            RefreshTexts();
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

            if (hintColor == default)
                hintColor = ShowcasePalette.TextMuted;

            if (pulseColor == default)
                pulseColor = ShowcasePalette.AccentStrong;
        }
    }
}
