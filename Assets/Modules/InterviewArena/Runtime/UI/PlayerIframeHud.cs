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

        public void BindPlayerHealth(Health health)
        {
            playerHealth = health;
        }

        private void Start()
        {
            if (playerHealth != null)
                return;

            InterviewArenaRuntimeContext context = FindFirstObjectByType<InterviewArenaRuntimeContext>();
            if (context != null && context.Player != null)
                BindPlayerHealth(context.Player.GetComponent<Health>());
        }

        private void Update()
        {
            if (playerHealth == null || fillImage == null)
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
