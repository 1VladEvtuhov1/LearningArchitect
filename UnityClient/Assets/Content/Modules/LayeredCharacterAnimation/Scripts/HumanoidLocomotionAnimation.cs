using UnityEngine;

namespace LearningArchitect.Modules.Animation3D
{
    /// <summary>
    /// Shared helpers for 2D directional humanoid locomotion (MoveX / MoveY vs body forward).
    /// </summary>
    public static class HumanoidLocomotionAnimation
    {
        public static void ResolveLocalMove(
            Vector3 flatVelocity,
            Vector3 bodyForward,
            float speedNormalization,
            out float moveX,
            out float moveY,
            out float speed01)
        {
            flatVelocity.y = 0f;
            speed01 = Mathf.Clamp01(flatVelocity.magnitude / Mathf.Max(0.01f, speedNormalization));

            if (speed01 <= 0.001f)
            {
                moveX = 0f;
                moveY = 0f;
                return;
            }

            bodyForward.y = 0f;
            if (bodyForward.sqrMagnitude < 0.0001f)
            {
                moveX = 0f;
                moveY = speed01;
                return;
            }

            Vector3 forward = bodyForward.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 direction = flatVelocity.normalized;
            moveX = Vector3.Dot(direction, right) * speed01;
            moveY = Vector3.Dot(direction, forward) * speed01;
        }
    }
}
