using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    public static class PlayerLocomotionMath
    {
        public static Vector3 ProjectOnGround(Vector3 wishDirection, Vector3 groundNormal)
        {
            if (wishDirection.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            Vector3 projected = Vector3.ProjectOnPlane(wishDirection, groundNormal);
            return projected.sqrMagnitude < 0.0001f ? Vector3.zero : projected.normalized;
        }

        public static bool IsWalkableNormal(Vector3 groundNormal, float maxGroundAngleDegrees)
        {
            float limit = Mathf.Clamp(maxGroundAngleDegrees, 0f, 89f);
            float dot = Vector3.Dot(groundNormal, Vector3.up);
            float minDot = Mathf.Cos(limit * Mathf.Deg2Rad);
            return dot >= minDot;
        }

        public static Vector3 BuildCameraRelativePlanarDirection(
            Vector2 moveInput,
            Vector3 cameraForward,
            Vector3 cameraRight)
        {
            if (moveInput.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            Vector3 forward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
            Vector3 right = Vector3.ProjectOnPlane(cameraRight, Vector3.up);

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;

            forward.Normalize();
            right.Normalize();
            return (forward * moveInput.y + right * moveInput.x).normalized;
        }
    }
}
