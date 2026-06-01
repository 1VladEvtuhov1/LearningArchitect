using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class PlayerIframeHud : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Health playerHealth;

        private bool loggedMissingHealth;

        public void BindPlayerHealth(Health health)
        {
            playerHealth = health;
            loggedMissingHealth = false;
        }

        private void Update()
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

            if (fillImage == null)
                return;

            bool active = playerHealth.IsInvulnerable;
            float normalized = playerHealth.InvulnerabilityNormalized;

            fillImage.fillAmount = active ? normalized : 0f;
            fillImage.enabled = active;

            if (canvasGroup != null)
                canvasGroup.alpha = active ? 1f : 0.3f;
        }
    }
}
