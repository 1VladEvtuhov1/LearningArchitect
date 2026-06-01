using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class PlayerBuffHud : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private Image speedIcon;
        [SerializeField] private TextMeshProUGUI speedTimerLabel;

        [Header("Melee")]
        [SerializeField] private Image meleeIcon;
        [SerializeField] private TextMeshProUGUI meleeTimerLabel;

        [Header("Pickup hold")]
        [SerializeField] private Image pickupHoldFill;
        [SerializeField] private TextMeshProUGUI pickupHoldLabel;

        private PlayerBuffController controller;
        private PlayerBuffPickupInteractor interactor;

        public void Bind(PlayerBuffController buffController, PlayerBuffPickupInteractor pickupInteractor)
        {
            controller = buffController;
            interactor = pickupInteractor;
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (controller == null)
                return;

            UpdateBuff(
                BuffKind.MoveSpeed,
                controller.GetRemaining(BuffKind.MoveSpeed),
                speedIcon,
                speedTimerLabel);

            UpdateBuff(
                BuffKind.MeleeDamage,
                controller.GetRemaining(BuffKind.MeleeDamage),
                meleeIcon,
                meleeTimerLabel);

            if (pickupHoldFill != null)
            {
                float hold = interactor != null ? interactor.HoldNormalized : 0f;
                pickupHoldFill.fillAmount = hold;
                pickupHoldFill.enabled = hold > 0.001f;
            }

            if (pickupHoldLabel != null)
            {
                bool active = interactor != null && interactor.ActivePickup != null && interactor.ActivePickup.IsAvailable;
                pickupHoldLabel.enabled = active;
            }
        }

        private static void UpdateBuff(
            BuffKind kind,
            float remaining,
            Image icon,
            TextMeshProUGUI timerLabel)
        {
            bool active = remaining > 0.001f;
            if (icon != null)
                icon.enabled = active;
            if (timerLabel != null)
            {
                timerLabel.enabled = active;
                if (active)
                    timerLabel.text = Mathf.CeilToInt(remaining).ToString();
            }
        }
    }
}

