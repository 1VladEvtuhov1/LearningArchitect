using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [CreateAssetMenu(
        menuName = "Learning Architect/Interview Arena/Buff Pickup Config",
        fileName = "InterviewArena_BuffPickup")]
    public sealed class BuffPickupConfig : ScriptableObject
    {
        [Header("Effect")]
        [SerializeField] private BuffKind kind = BuffKind.MoveSpeed;
        [SerializeField] private float multiplier = 1.35f;
        [SerializeField] private float duration = 10f;

        [Header("Pickup")]
        [SerializeField] private float holdDuration = 0.55f;
        [SerializeField] private float pickupRadius = 1.1f;
        [SerializeField] private float respawnDelay = 18f;

        [Header("Presentation")]
        [SerializeField] private Color worldColor = new Color(0.35f, 0.92f, 1f, 0.95f);

        public BuffKind Kind => kind;
        public float Multiplier => multiplier;
        public float Duration => duration;
        public float HoldDuration => Mathf.Max(0.1f, holdDuration);
        public float PickupRadius => Mathf.Max(0.35f, pickupRadius);
        public float RespawnDelay => Mathf.Max(0f, respawnDelay);
        public Color WorldColor => worldColor;
    }
}
