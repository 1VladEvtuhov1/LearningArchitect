using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    [DefaultExecutionOrder(50)]
    [DisallowMultipleComponent]
    public sealed class ArenaCursorAim : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform aimPivot;

        public Vector3 AimDirection { get; private set; } = Vector3.forward;
        public Transform AimPivot => aimPivot != null ? aimPivot : transform;

        public void ApplyConfig(PlayerConfig playerConfig, Camera camera, Transform pivot)
        {
            config = playerConfig;
            targetCamera = camera;
            if (pivot != null)
                aimPivot = pivot;
        }

        private void LateUpdate()
        {
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            Transform pivot = AimPivot;

            if (cam == null || pivot == null)
                return;

            Ray ray = cam.ScreenPointToRay(ReadMousePosition());
            Plane aimPlane = new Plane(Vector3.up, pivot.position);

            if (!aimPlane.Raycast(ray, out float enter))
                return;

            Vector3 point = ray.GetPoint(enter);
            Vector3 direction = Vector3.ProjectOnPlane(point - pivot.position, Vector3.up);
            if (direction.sqrMagnitude < 0.001f)
                return;

            AimDirection = direction.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(AimDirection, Vector3.up);
            float turnSpeed = config != null ? config.AimTurnSpeed : 900f;
            pivot.rotation = Quaternion.RotateTowards(
                pivot.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime);
        }

        private static Vector3 ReadMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
                return UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#endif
            return Input.mousePosition;
        }
    }
}
