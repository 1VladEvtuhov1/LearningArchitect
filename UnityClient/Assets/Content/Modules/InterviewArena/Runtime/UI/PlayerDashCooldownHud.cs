using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class PlayerDashCooldownHud : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private PlayerMotor playerMotor;

        private bool loggedMissingMotor;

        public void BindPlayerMotor(PlayerMotor motor)
        {
            playerMotor = motor;
            loggedMissingMotor = false;
        }

        private void Update()
        {
            if (playerMotor == null)
            {
                if (!loggedMissingMotor)
                {
                    loggedMissingMotor = true;
                    InterviewArenaAuthoringLog.MissingReference(this, nameof(playerMotor));
                }

                return;
            }

            bool onCooldown = playerMotor.IsDashOnCooldown;
            float normalized = playerMotor.DashCooldownNormalized;

            if (fillImage != null)
            {
                fillImage.fillAmount = onCooldown ? normalized : 0f;
                fillImage.enabled = onCooldown;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = onCooldown ? 1f : 0.4f;

            if (label != null)
            {
                label.enabled = true;
                label.text = onCooldown
                    ? Mathf.CeilToInt(playerMotor.DashCooldownRemaining).ToString()
                    : "Shift";
            }
        }
    }
}
