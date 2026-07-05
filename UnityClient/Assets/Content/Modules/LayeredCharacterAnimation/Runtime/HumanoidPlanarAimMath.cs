using UnityEngine;

namespace LearningArchitect.Modules.Animation3D
{
    /// <summary>
    /// Shared horizontal aim math for locomotion body forward vs cursor aim.
    /// </summary>
    public static class HumanoidPlanarAimMath
    {
        public static float ExpLerpFactor(float speed, float deltaTime) =>
            1f - Mathf.Exp(-Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime));

        public static float SignedPlanarYaw(Vector3 from, Vector3 to, float fallbackSign)
        {
            from.y = 0f;
            to.y = 0f;

            if (from.sqrMagnitude < 0.0001f || to.sqrMagnitude < 0.0001f)
                return 0f;

            from.Normalize();
            to.Normalize();

            float crossY = Vector3.Dot(Vector3.up, Vector3.Cross(from, to));
            float dot = Vector3.Dot(from, to);
            float yaw = Mathf.Atan2(crossY, dot) * Mathf.Rad2Deg;

            if (dot < -0.98f && Mathf.Abs(crossY) < 0.02f)
                yaw = 180f * Mathf.Sign(fallbackSign == 0f ? 1f : fallbackSign);

            return yaw;
        }

        public static float StabilizeSignedYaw(Vector3 from, Vector3 to, ref float lastSign)
        {
            float yaw = SignedPlanarYaw(from, to, lastSign);

            from.y = 0f;
            to.y = 0f;
            if (from.sqrMagnitude < 0.0001f || to.sqrMagnitude < 0.0001f)
                return yaw;

            from.Normalize();
            to.Normalize();
            float dot = Vector3.Dot(from, to);

            if (dot < -0.98f && Mathf.Abs(yaw) > 170f)
                yaw = 180f * Mathf.Sign(lastSign == 0f ? 1f : lastSign);
            else if (Mathf.Abs(yaw) > 1f)
                lastSign = Mathf.Sign(yaw);

            return yaw;
        }

        public static float ClampSignedYaw(float rawYaw, float maxDegrees) =>
            Mathf.Clamp(rawYaw, -maxDegrees, maxDegrees);

        public static Vector3 ClampAimDirection(
            Vector3 bodyForward,
            Vector3 desiredAimDirection,
            float maxDegrees,
            float fallbackSign,
            out float rawYaw,
            out float clampedYaw)
        {
            rawYaw = SignedPlanarYaw(bodyForward, desiredAimDirection, fallbackSign);
            clampedYaw = ClampSignedYaw(rawYaw, maxDegrees);

            bodyForward.y = 0f;
            if (bodyForward.sqrMagnitude < 0.0001f)
                return desiredAimDirection.sqrMagnitude > 0.0001f
                    ? desiredAimDirection.normalized
                    : Vector3.forward;

            bodyForward.Normalize();
            return (Quaternion.AngleAxis(clampedYaw, Vector3.up) * bodyForward).normalized;
        }

        public static Vector3 ClampAimDirection(
            Vector3 bodyForward,
            Vector3 desiredAimDirection,
            float maxDegrees,
            float fallbackSign) =>
            ClampAimDirection(bodyForward, desiredAimDirection, maxDegrees, fallbackSign, out _, out _);
    }
}
