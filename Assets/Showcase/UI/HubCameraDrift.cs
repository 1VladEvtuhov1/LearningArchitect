using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LearningArchitect.UI
{
    public sealed class HubCameraDrift : MonoBehaviour
    {
        [Header("Orbit")]
        public bool enableOrbit = true;
        public Vector3 targetPosition = Vector3.zero;
        public float distance = 12.5f;
        public float minDistance = 4f;
        public float maxDistance = 28f;
        public float yaw = 0f;
        public float pitch = 23f;
        public float orbitSpeed = 80f;
        public float zoomSpeed = 5.5f;
        public float keyboardOrbitSpeed = 28f;
        public float rotationSmoothTime = 0.1f;
        public float zoomSmoothTime = 0.08f;

        [Header("Idle Drift")]
        public Vector3 positionAmplitude = new Vector3(0.08f, 0.025f, 0f);
        public Vector3 rotationAmplitude = new Vector3(0.12f, 0.2f, 0f);
        public float speed = 0.14f;
        public float demoCueBlendSpeed = 3.2f;

        private float cueDistanceOffset;
        private float cuePitchOffset;
        private float cueYawOffset;
        private float targetCueDistanceOffset;
        private float targetCuePitchOffset;
        private float targetCueYawOffset;

        private float targetYaw;
        private float targetPitch;
        private float targetDistance;
        private float displayYaw;
        private float displayPitch;
        private float displayDistance;
        private float yawVelocity;
        private float pitchVelocity;
        private float distanceVelocity;

        private Vector2 previousPointerPosition;
        private bool hasPointerPosition;

        private void Awake()
        {
            Vector3 offset = transform.position - targetPosition;
            distance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);

            if (offset.sqrMagnitude > 0.0001f)
            {
                Vector3 flat = new Vector3(offset.x, 0f, offset.z);
                yaw = Mathf.Atan2(flat.x, -flat.z) * Mathf.Rad2Deg;
                pitch = Mathf.Asin(offset.y / distance) * Mathf.Rad2Deg;
            }

            targetYaw = displayYaw = yaw;
            targetPitch = displayPitch = pitch;
            targetDistance = displayDistance = distance;
        }

        private void Update()
        {
            if (enableOrbit)
                UpdateOrbitInput();

            float deltaTime = Time.unscaledDeltaTime;
            float rotationLerp = 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.001f, rotationSmoothTime));
            float zoomLerp = 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.001f, zoomSmoothTime));

            displayYaw = Mathf.LerpAngle(displayYaw, targetYaw, rotationLerp);
            displayPitch = Mathf.Lerp(displayPitch, targetPitch, rotationLerp);
            displayDistance = Mathf.Lerp(displayDistance, targetDistance, zoomLerp);

            float cueLerp = 1f - Mathf.Exp(-demoCueBlendSpeed * deltaTime);
            cueYawOffset = Mathf.Lerp(cueYawOffset, targetCueYawOffset, cueLerp);
            cuePitchOffset = Mathf.Lerp(cuePitchOffset, targetCuePitchOffset, cueLerp);
            cueDistanceOffset = Mathf.Lerp(cueDistanceOffset, targetCueDistanceOffset, cueLerp);

            float time = Time.unscaledTime * speed;
            Vector3 offset = new Vector3(
                Mathf.Sin(time * 0.91f) * positionAmplitude.x,
                Mathf.Sin(time * 1.17f) * positionAmplitude.y,
                Mathf.Sin(time * 0.73f) * positionAmplitude.z);

            Vector3 eulerOffset = new Vector3(
                Mathf.Sin(time * 0.83f) * rotationAmplitude.x,
                Mathf.Sin(time) * rotationAmplitude.y,
                Mathf.Sin(time * 0.67f) * rotationAmplitude.z);

            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            yaw = displayYaw;
            pitch = displayPitch;
            distance = displayDistance;

            float finalPitch = Mathf.Clamp(pitch + cuePitchOffset, minPitch, maxPitch);
            float finalDistance = Mathf.Clamp(distance + cueDistanceOffset, minDistance, maxDistance);
            float finalYaw = yaw + cueYawOffset;

            Quaternion orbitRotation = Quaternion.Euler(finalPitch, finalYaw, 0f);
            Vector3 orbitPosition = targetPosition + orbitRotation * new Vector3(0f, 0f, -finalDistance);

            transform.position = orbitPosition + offset;
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up) * Quaternion.Euler(eulerOffset);
        }

        public void ApplyDemoCue(float yawOffset, float pitchOffset, float distanceOffset)
        {
            targetCueYawOffset = yawOffset;
            targetCuePitchOffset = pitchOffset;
            targetCueDistanceOffset = distanceOffset;
        }

        public void ResetDemoCue()
        {
            targetCueYawOffset = 0f;
            targetCuePitchOffset = 0f;
            targetCueDistanceOffset = 0f;
        }

        private void UpdateOrbitInput()
        {
            float horizontal = 0f;
            float scrollUnits = 0f;
            bool orbitHeld = false;
            Vector2 pointerPosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    horizontal -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    horizontal += 1f;
                if (keyboard.qKey.isPressed)
                    scrollUnits += 1f;
                if (keyboard.eKey.isPressed)
                    scrollUnits -= 1f;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                orbitHeld = mouse.rightButton.isPressed;
                pointerPosition = mouse.position.ReadValue();
                scrollUnits += NormalizeScrollDelta(mouse.scroll.ReadValue().y);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                horizontal += 1f;
            if (Input.GetKey(KeyCode.Q))
                scrollUnits += 1f;
            if (Input.GetKey(KeyCode.E))
                scrollUnits -= 1f;

            orbitHeld = orbitHeld || Input.GetMouseButton(1);
            pointerPosition = Input.mousePosition;
            scrollUnits += NormalizeScrollDelta(Input.mouseScrollDelta.y);
#endif

            targetYaw += horizontal * keyboardOrbitSpeed * Time.unscaledDeltaTime;

            if (orbitHeld)
            {
                if (hasPointerPosition)
                {
                    Vector2 delta = pointerPosition - previousPointerPosition;
                    targetYaw += delta.x * orbitSpeed * 0.01f;
                    targetPitch -= delta.y * orbitSpeed * 0.01f;
                }

                previousPointerPosition = pointerPosition;
                hasPointerPosition = true;
            }
            else
            {
                hasPointerPosition = false;
            }

            if (Mathf.Abs(scrollUnits) > 0.001f)
            {
                float zoomDelta = scrollUnits * zoomSpeed;
                if (Mathf.Abs(scrollUnits) <= 1.01f && scrollUnits != 0f)
                    zoomDelta = scrollUnits * zoomSpeed * Time.unscaledDeltaTime * 60f;

                targetDistance -= zoomDelta;
            }
        }

        private static float NormalizeScrollDelta(float rawDelta)
        {
            if (Mathf.Abs(rawDelta) < 0.001f)
                return 0f;

#if ENABLE_INPUT_SYSTEM
            if (Mathf.Abs(rawDelta) >= 10f)
                return Mathf.Sign(rawDelta) * Mathf.Clamp(Mathf.Abs(rawDelta) / 120f, 0.05f, 2.5f);
#endif

            return Mathf.Clamp(rawDelta, -2.5f, 2.5f);
        }

        private const float minPitch = 8f;
        private const float maxPitch = 72f;
    }
}
