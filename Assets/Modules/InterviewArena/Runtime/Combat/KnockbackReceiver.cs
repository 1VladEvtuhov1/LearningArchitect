using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    public sealed class KnockbackReceiver : MonoBehaviour
    {
        [SerializeField] private float knockbackMultiplier = 1f;

        private Rigidbody body;
        private Health health;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (health != null)
                health.Damaged += HandleDamaged;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Damaged -= HandleDamaged;
        }

        private void HandleDamaged(Health _, DamageInfo info)
        {
            if (info.KnockbackImpulse <= 0f || body == null)
                return;

            Vector3 direction = info.HitDirection.sqrMagnitude > 0.0001f
                ? info.HitDirection.normalized
                : Vector3.forward;

            body.AddForce(direction * (info.KnockbackImpulse * knockbackMultiplier), ForceMode.Impulse);
        }
    }
}
