using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DisallowMultipleComponent]
    public sealed class BuffPickup : MonoBehaviour
    {
        [SerializeField] private BuffPickupConfig config;
        [SerializeField] private SphereCollider pickupTrigger;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer visualRenderer;

        private float holdProgress;
        private float respawnTimer;
        private bool consumed;
        private Vector3 baseVisualScale = Vector3.one;
        private Color baseColor = Color.white;

        public BuffPickupConfig Config => config;
        public bool IsAvailable => !consumed && config != null;
        public float HoldNormalized =>
            config != null && config.HoldDuration > 0f
                ? Mathf.Clamp01(holdProgress / config.HoldDuration)
                : 0f;

        public void ApplyConfig(BuffPickupConfig pickupConfig)
        {
            config = pickupConfig;
            ConfigureTrigger();
            ApplyVisualDefaults();
        }

        public void ResetHoldProgress()
        {
            holdProgress = 0f;
            UpdateHoldVisual();
        }

        public void AddHoldProgress(float deltaTime, PlayerBuffController target)
        {
            if (!IsAvailable || target == null || config == null)
                return;

            holdProgress += deltaTime;
            UpdateHoldVisual();

            if (holdProgress < config.HoldDuration)
                return;

            target.ApplyBuff(config);
            Consume();
        }

        private void Update()
        {
            if (!consumed || config == null || config.RespawnDelay <= 0f)
                return;

            respawnTimer -= Time.deltaTime;
            if (respawnTimer > 0f)
                return;

            consumed = false;
            holdProgress = 0f;
            SetVisualActive(true);
            UpdateHoldVisual();
        }

        private void Consume()
        {
            consumed = true;
            holdProgress = 0f;
            respawnTimer = config != null ? config.RespawnDelay : 0f;
            SetVisualActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerBuffPickupInteractor interactor = other.GetComponentInParent<PlayerBuffPickupInteractor>();
            if (interactor == null || !IsAvailable)
                return;

            interactor.SetActivePickup(this);
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerBuffPickupInteractor interactor = other.GetComponentInParent<PlayerBuffPickupInteractor>();
            if (interactor == null)
                return;

            interactor.ClearActivePickup(this);
        }

        private void ConfigureTrigger()
        {
            if (pickupTrigger == null)
                pickupTrigger = GetComponent<SphereCollider>();

            if (pickupTrigger == null)
                pickupTrigger = gameObject.AddComponent<SphereCollider>();

            pickupTrigger.isTrigger = true;
            pickupTrigger.radius = config != null ? config.PickupRadius : 1f;
        }

        private void ApplyVisualDefaults()
        {
            if (visualRoot == null)
                visualRoot = transform;

            baseVisualScale = visualRoot.localScale;
            if (visualRenderer == null)
                visualRenderer = visualRoot.GetComponentInChildren<Renderer>(true);

            if (visualRenderer != null && config != null)
            {
                baseColor = config.WorldColor;
                visualRenderer.sharedMaterial.color = baseColor;
            }

            UpdateHoldVisual();
        }

        private void UpdateHoldVisual()
        {
            if (visualRoot == null)
                return;

            float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.06f;
            visualRoot.localScale = baseVisualScale * pulse;

            if (visualRenderer == null)
                return;

            Color color = baseColor;
            color.a = Mathf.Lerp(0.55f, 1f, HoldNormalized);
            visualRenderer.sharedMaterial.color = color;
        }

        private void SetVisualActive(bool active)
        {
            if (visualRoot != null)
                visualRoot.gameObject.SetActive(active);

            if (pickupTrigger != null)
                pickupTrigger.enabled = active;
        }
    }
}
