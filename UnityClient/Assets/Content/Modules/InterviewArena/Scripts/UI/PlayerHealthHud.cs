using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHud : MonoBehaviour
    {
        private static readonly Color FillHealthy = new Color(0.25f, 0.78f, 0.42f, 0.95f);
        private static readonly Color FillCritical = new Color(0.86f, 0.22f, 0.24f, 0.95f);

        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Health playerHealth;

        private bool loggedMissingHealth;

        public void BindPlayerHealth(Health health)
        {
            playerHealth = health;
            loggedMissingHealth = false;
            Refresh();
        }

        private void Awake()
        {
            if (healthSlider == null)
                healthSlider = GetComponent<Slider>();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (playerHealth == null)
            {
                if (!loggedMissingHealth)
                {
                    loggedMissingHealth = true;
                    InterviewArenaAuthoringLog.MissingReference(this, nameof(playerHealth));
                }

                return;
            }

            float normalized = PlayerVitalHudRules.Normalized(
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth);

            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                healthSlider.wholeNumbers = false;
                healthSlider.interactable = false;
                healthSlider.SetValueWithoutNotify(normalized);
            }

            if (fillImage != null)
            {
                fillImage.enabled = true;
                fillImage.color = Color.Lerp(FillCritical, FillHealthy, normalized);
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            if (label != null)
            {
                label.enabled = true;
                label.text = PlayerVitalHudRules.FormatLabel(
                    playerHealth.CurrentHealth,
                    playerHealth.MaxHealth);
            }
        }
    }
}
