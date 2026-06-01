using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerBuffController))]
    public sealed class PlayerBuffVfx : MonoBehaviour
    {
        [SerializeField] private Transform speedAura;
        [SerializeField] private Transform meleeAura;

        [SerializeField] private Color speedColor = new Color(0.35f, 0.92f, 1f, 0.8f);
        [SerializeField] private Color meleeColor = new Color(1f, 0.62f, 0.28f, 0.8f);

        private PlayerBuffController controller;
        private Material speedMaterial;
        private Material meleeMaterial;

        private void Awake()
        {
            controller = GetComponent<PlayerBuffController>();
            EnsureAuraObjects();
            Refresh();
        }

        private void OnEnable()
        {
            if (controller != null)
                controller.Changed += Refresh;
        }

        private void OnDisable()
        {
            if (controller != null)
                controller.Changed -= Refresh;
        }

        private void OnDestroy()
        {
            if (speedMaterial != null)
                Destroy(speedMaterial);
            if (meleeMaterial != null)
                Destroy(meleeMaterial);
        }

        private void Update()
        {
            AnimateAuras();
        }

        private void AnimateAuras()
        {
            if (speedAura != null && speedAura.gameObject.activeSelf)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 5.2f) * 0.06f;
                speedAura.localScale = new Vector3(1.35f, 0.06f, 1.35f) * pulse;
                speedAura.Rotate(0f, 55f * Time.deltaTime, 0f, Space.Self);
            }

            if (meleeAura != null && meleeAura.gameObject.activeSelf)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 6.1f) * 0.06f;
                meleeAura.localScale = new Vector3(1.15f, 0.06f, 1.15f) * pulse;
                meleeAura.Rotate(0f, -70f * Time.deltaTime, 0f, Space.Self);
            }
        }

        private void Refresh()
        {
            if (controller == null)
                return;

            bool speed = controller.GetRemaining(BuffKind.MoveSpeed) > 0.001f;
            bool melee = controller.GetRemaining(BuffKind.MeleeDamage) > 0.001f;

            if (speedAura != null)
                speedAura.gameObject.SetActive(speed);
            if (meleeAura != null)
                meleeAura.gameObject.SetActive(melee);
        }

        private void EnsureAuraObjects()
        {
            if (speedAura == null)
                speedAura = CreateAura("SpeedAura", speedColor, ref speedMaterial, new Vector3(1.35f, 0.06f, 1.35f));
            if (meleeAura == null)
                meleeAura = CreateAura("MeleeAura", meleeColor, ref meleeMaterial, new Vector3(1.15f, 0.06f, 1.15f));
        }

        private Transform CreateAura(string name, Color color, ref Material material, Vector3 localScale)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = name;
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            ring.transform.localRotation = Quaternion.identity;
            ring.transform.localScale = localScale;

            Collider collider = ring.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            Renderer renderer = ring.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Sprites/Default");

                material = new Material(shader);
                if (shader.name.Contains("Universal Render Pipeline"))
                {
                    material.SetColor("_BaseColor", color);
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", color * 1.6f);
                }
                else
                {
                    material.color = color;
                }

                renderer.sharedMaterial = material;
            }

            ring.SetActive(false);
            return ring.transform;
        }
    }
}

