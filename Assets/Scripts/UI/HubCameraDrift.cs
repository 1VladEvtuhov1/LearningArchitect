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
        public float minDistance = 8f;
        public float maxDistance = 17f;
        public float yaw = 0f;
        public float pitch = 23f;
        public float orbitSpeed = 80f;
        public float zoomSpeed = 2.5f;
        public float keyboardOrbitSpeed = 28f;

        [Header("Idle Drift")]
        public Vector3 positionAmplitude = new Vector3(0.08f, 0.025f, 0f);
        public Vector3 rotationAmplitude = new Vector3(0.12f, 0.2f, 0f);
        public float speed = 0.14f;

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
        }

        private void Update()
        {
            if (enableOrbit)
                UpdateOrbitInput();

            float time = Time.unscaledTime * speed;
            Vector3 offset = new Vector3(
                Mathf.Sin(time * 0.91f) * positionAmplitude.x,
                Mathf.Sin(time * 1.17f) * positionAmplitude.y,
                Mathf.Sin(time * 0.73f) * positionAmplitude.z);

            Vector3 eulerOffset = new Vector3(
                Mathf.Sin(time * 0.83f) * rotationAmplitude.x,
                Mathf.Sin(time) * rotationAmplitude.y,
                Mathf.Sin(time * 0.67f) * rotationAmplitude.z);

            pitch = Mathf.Clamp(pitch, 12f, 55f);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 orbitPosition = targetPosition + orbitRotation * new Vector3(0f, 0f, -distance);

            transform.position = orbitPosition + offset;
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up) * Quaternion.Euler(eulerOffset);
        }

        private void UpdateOrbitInput()
        {
            float horizontal = 0f;
            float scroll = 0f;
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
                    scroll -= 1f;
                if (keyboard.eKey.isPressed)
                    scroll += 1f;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                orbitHeld = mouse.rightButton.isPressed;
                pointerPosition = mouse.position.ReadValue();
                scroll += mouse.scroll.ReadValue().y * 0.01f;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                horizontal += 1f;
            if (Input.GetKey(KeyCode.Q))
                scroll -= 1f;
            if (Input.GetKey(KeyCode.E))
                scroll += 1f;

            orbitHeld = orbitHeld || Input.GetMouseButton(1);
            pointerPosition = Input.mousePosition;
            scroll += Input.mouseScrollDelta.y;
#endif

            yaw += horizontal * keyboardOrbitSpeed * Time.unscaledDeltaTime;

            if (orbitHeld)
            {
                if (hasPointerPosition)
                {
                    Vector2 delta = pointerPosition - previousPointerPosition;
                    yaw += delta.x * orbitSpeed * 0.01f;
                    pitch -= delta.y * orbitSpeed * 0.01f;
                }

                previousPointerPosition = pointerPosition;
                hasPointerPosition = true;
            }
            else
            {
                hasPointerPosition = false;
            }

            if (Mathf.Abs(scroll) > 0.001f)
                distance -= scroll * zoomSpeed;
        }
    }
}
