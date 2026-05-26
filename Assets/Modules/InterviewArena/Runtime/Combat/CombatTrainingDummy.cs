using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Dark-fantasy practice target: restores health after death for repeatable melee/crossbow drills.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class CombatTrainingDummy : MonoBehaviour
    {
        [SerializeField] private float respawnDelay = 1.4f;
        [SerializeField] private Renderer[] tintRenderers;

        private Health health;
        private float respawnTimer = -1f;
        private Color[] defaultColors;

        private void Awake()
        {
            health = GetComponent<Health>();
            CacheDefaultColors();
            health.Died += HandleDied;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= HandleDied;
        }

        private void Update()
        {
            if (respawnTimer < 0f)
                return;

            respawnTimer -= Time.deltaTime;
            if (respawnTimer > 0f)
                return;

            respawnTimer = -1f;
            health.RestoreFullHealth();
            SetTint(0.72f, 0.2f, 0.24f, 0.95f);
        }

        private void HandleDied(Health _)
        {
            SetTint(0.18f, 0.18f, 0.2f, 0.55f);
            respawnTimer = respawnDelay;
        }

        private void CacheDefaultColors()
        {
            if (tintRenderers == null || tintRenderers.Length == 0)
                tintRenderers = GetComponentsInChildren<Renderer>();

            defaultColors = new Color[tintRenderers.Length];
            for (int i = 0; i < tintRenderers.Length; i++)
            {
                Renderer renderer = tintRenderers[i];
                defaultColors[i] = renderer != null && renderer.sharedMaterial != null
                    ? renderer.sharedMaterial.color
                    : new Color(0.72f, 0.2f, 0.24f, 0.95f);
            }
        }

        private void SetTint(float r, float g, float b, float a)
        {
            if (tintRenderers == null)
                return;

            Color color = new Color(r, g, b, a);
            for (int i = 0; i < tintRenderers.Length; i++)
            {
                Renderer renderer = tintRenderers[i];
                if (renderer == null || renderer.sharedMaterial == null)
                    continue;

                renderer.sharedMaterial.color = color;
            }
        }
    }
}
