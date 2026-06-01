using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Shared capsule wall-slide for player and enemies (Ground-layer obstacles).
    /// </summary>
    public static class PlanarLocomotionCollision
    {
        private const float Skin = 0.06f;

        public static Vector3 SlideAlongWalls(
            Rigidbody body,
            CapsuleCollider capsule,
            Vector3 planarVelocity,
            LayerMask obstructionMask,
            float airborneSlideScale = 0f)
        {
            if (body == null || capsule == null || planarVelocity.sqrMagnitude < 0.0001f)
                return planarVelocity;

            float scale = airborneSlideScale <= 0f ? 1f : Mathf.Clamp01(airborneSlideScale);
            if (scale <= 0f)
                return planarVelocity;

            ResolveCapsuleCastPoints(capsule, out Vector3 pointA, out Vector3 pointB, out float radius);
            Vector3 direction = planarVelocity.normalized;
            float castDistance = planarVelocity.magnitude * Time.fixedDeltaTime + Skin;

            if (!Physics.CapsuleCast(
                pointA,
                pointB,
                radius,
                direction,
                out RaycastHit hit,
                castDistance,
                obstructionMask,
                QueryTriggerInteraction.Ignore))
            {
                return planarVelocity;
            }

            Vector3 adjusted = PlayerLocomotionMath.SlideAlongWall(planarVelocity, hit.normal);
            return adjusted * scale;
        }

        public static void ApplyPlanarVelocity(Rigidbody body, Vector3 planarVelocity)
        {
            if (body == null)
                return;

            body.linearVelocity = new Vector3(planarVelocity.x, body.linearVelocity.y, planarVelocity.z);
        }

        public static void ResolveCapsuleCastPoints(
            CapsuleCollider capsule,
            out Vector3 pointA,
            out Vector3 pointB,
            out float radius)
        {
            Transform capsuleTransform = capsule.transform;
            float scaleY = Mathf.Abs(capsuleTransform.lossyScale.y);
            float scaleXZ = Mathf.Max(
                Mathf.Abs(capsuleTransform.lossyScale.x),
                Mathf.Abs(capsuleTransform.lossyScale.z));
            radius = Mathf.Max(0.01f, capsule.radius * scaleXZ);
            float height = Mathf.Max(capsule.height * scaleY, radius * 2f + 0.01f);
            float cylinder = Mathf.Max(0f, height - radius * 2f);
            Vector3 center = capsuleTransform.TransformPoint(capsule.center);
            Vector3 up = capsuleTransform.up;
            pointA = center - up * (cylinder * 0.5f);
            pointB = center + up * (cylinder * 0.5f);
        }
    }
}
