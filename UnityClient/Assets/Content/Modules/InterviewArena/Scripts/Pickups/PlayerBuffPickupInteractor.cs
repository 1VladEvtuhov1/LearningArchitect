using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerBuffController))]
    public sealed class PlayerBuffPickupInteractor : MonoBehaviour
    {
        private PlayerInputReader input;
        private PlayerBuffController buffController;
        private BuffPickup activePickup;

        public BuffPickup ActivePickup => activePickup;
        public float HoldNormalized =>
            activePickup != null && activePickup.IsAvailable
                ? activePickup.HoldNormalized
                : 0f;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            buffController = GetComponent<PlayerBuffController>();
        }

        private void Update()
        {
            if (activePickup == null || !activePickup.IsAvailable)
                return;

            if (!input.InteractHeld)
            {
                activePickup.ResetHoldProgress();
                return;
            }

            activePickup.AddHoldProgress(Time.deltaTime, buffController);
        }

        public void SetActivePickup(BuffPickup pickup)
        {
            if (pickup == null || !pickup.IsAvailable)
                return;

            if (activePickup != null && activePickup != pickup)
                activePickup.ResetHoldProgress();

            activePickup = pickup;
        }

        public void ClearActivePickup(BuffPickup pickup)
        {
            if (activePickup != pickup)
                return;

            pickup.ResetHoldProgress();
            activePickup = null;
        }
    }
}
