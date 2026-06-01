using System.Collections;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Lightweight procedural hit/swing flashes (no authored VFX assets required).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatHitFeedback : MonoBehaviour
    {
        private static CombatHitFeedback instance;

        [SerializeField] private Color hitColor = new Color(1f, 0.55f, 0.2f, 1f);
        [SerializeField] private Color swingColor = new Color(0.82f, 0.9f, 1f, 0.75f);
        [SerializeField] private Color crossbowColor = new Color(0.75f, 0.95f, 1f, 0.9f);
        [SerializeField] private float hitFlashScale = 0.35f;
        [SerializeField] private float swingFlashScale = 0.55f;
        [SerializeField] private float crossbowFlashScale = 0.22f;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void PlayDamageHit(in DamageInfo info)
        {
            if (instance == null)
                return;

            instance.StartCoroutine(instance.FlashRoutine(
                info.HitPoint,
                info.HitDirection,
                instance.hitColor,
                instance.hitFlashScale,
                0.12f));
        }

        public static void PlayMeleeSwing(Transform origin, float radius)
        {
            if (instance == null || origin == null)
                return;

            Vector3 center = origin.position + origin.forward * Mathf.Max(0.2f, radius * 0.45f);
            instance.StartCoroutine(instance.FlashRoutine(
                center,
                origin.forward,
                instance.swingColor,
                instance.swingFlashScale,
                0.08f));
        }

        public static void PlayMeleeWindup(Transform origin, float radius)
        {
            if (instance == null || origin == null)
                return;

            Vector3 center = origin.position + origin.forward * Mathf.Max(0.15f, radius * 0.35f);
            Color windup = instance.swingColor;
            windup.a *= 0.55f;
            instance.StartCoroutine(instance.FlashRoutine(
                center,
                origin.forward,
                windup,
                instance.swingFlashScale * 0.65f,
                0.1f));
        }

        public static void PlayMeleeHitConfirm(Vector3 position)
        {
            if (instance == null)
                return;

            instance.StartCoroutine(instance.FlashRoutine(
                position,
                Vector3.up,
                instance.hitColor,
                instance.hitFlashScale * 1.15f,
                0.1f));
        }

        public static void PlayBuffApplied(Vector3 position, Color color)
        {
            if (instance == null)
                return;

            instance.StartCoroutine(instance.FlashRoutine(
                position + Vector3.up * 0.35f,
                Vector3.up,
                color,
                instance.hitFlashScale * 1.35f,
                0.16f));
        }

        public static void PlayCrossbowFire(Transform muzzle)
        {
            if (instance == null || muzzle == null)
                return;

            instance.StartCoroutine(instance.FlashRoutine(
                muzzle.position,
                muzzle.forward,
                instance.crossbowColor,
                instance.crossbowFlashScale,
                0.06f));
        }

        private IEnumerator FlashRoutine(
            Vector3 position,
            Vector3 direction,
            Color color,
            float scale,
            float duration)
        {
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "CombatHitFlash";
            flash.transform.position = position;
            flash.transform.localScale = Vector3.one * scale;
            if (direction.sqrMagnitude > 0.0001f)
                flash.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            Collider collider = flash.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            Renderer renderer = flash.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Sprites/Default");

                Material material = new Material(shader);
                if (shader.name.Contains("Universal Render Pipeline"))
                {
                    material.SetColor("_BaseColor", color);
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", color * 2.2f);
                }
                else
                {
                    material.color = color;
                }

                renderer.sharedMaterial = material;
            }

            float elapsed = 0f;
            Vector3 startScale = flash.transform.localScale;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pulse = 1f + Mathf.Sin(t * Mathf.PI) * 0.35f;
                flash.transform.localScale = startScale * pulse;
                if (renderer != null)
                {
                    Color faded = color;
                    faded.a = Mathf.Lerp(color.a, 0f, t);
                    renderer.sharedMaterial.SetColor("_BaseColor", faded);
                    renderer.sharedMaterial.SetColor("_EmissionColor", faded * 2.2f);
                }

                yield return null;
            }

            Destroy(flash);
            if (renderer != null && renderer.sharedMaterial != null)
                Destroy(renderer.sharedMaterial);
        }
    }
}
