using LearningArchitect.Modules.Animation3D;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Runs after <see cref="ArenaCursorAim"/> and before <see cref="PlayerCombat"/> so fire
    /// uses the same clamped aim as presentation in the same frame.
    /// </summary>
    [DefaultExecutionOrder(75)]
    [DisallowMultipleComponent]
    public sealed class BowAimDirectionProvider : MonoBehaviour, IClampedAimProvider
    {
        [SerializeField] private ArenaCursorAim cursorAim;
        [SerializeField] private float maxAimYawDegrees = 75f;

        private IBodyFacingProvider bodyFacing;
        private float lastAimYawSign = 1f;
        private Vector3 rawAimDirection = Vector3.forward;
        private Vector3 bodyForward = Vector3.forward;
        private Vector3 targetClampedAimDirection = Vector3.forward;

        public Vector3 RawAimDirection => rawAimDirection;
        public Vector3 BodyForward => bodyForward;
        public Vector3 TargetClampedAimDirection => targetClampedAimDirection;
        public float MaxAimYawDegrees => Mathf.Max(0f, maxAimYawDegrees);

        public void ApplyMaxAimYawDegrees(float degrees)
        {
            maxAimYawDegrees = Mathf.Max(0f, degrees);
        }

        private void Awake()
        {
            if (cursorAim == null)
                cursorAim = GetComponent<ArenaCursorAim>();

            bodyFacing = GetComponent<IBodyFacingProvider>();
        }

        private void LateUpdate()
        {
            Recalculate();
        }

        /// <summary>
        /// Recalculates clamped aim. Safe to call from Edit Mode tests without LateUpdate.
        /// </summary>
        public void Recalculate()
        {
            if (cursorAim == null)
                cursorAim = GetComponent<ArenaCursorAim>();

            bodyForward = ResolveRootBodyForward();
            rawAimDirection = ResolveRawAim(bodyForward);

            float rawYaw = HumanoidPlanarAimMath.StabilizeSignedYaw(
                bodyForward,
                rawAimDirection,
                ref lastAimYawSign);
            float clampedYaw = HumanoidPlanarAimMath.ClampSignedYaw(rawYaw, MaxAimYawDegrees);
            targetClampedAimDirection =
                (Quaternion.AngleAxis(clampedYaw, Vector3.up) * bodyForward).normalized;
        }

        /// <summary>Test/tooling: seed hysteresis sign without going through LateUpdate.</summary>
        public void SetLastAimYawSignForTests(float sign)
        {
            lastAimYawSign = sign == 0f ? 1f : Mathf.Sign(sign);
        }

        public float LastAimYawSignForTests => lastAimYawSign;

        private Vector3 ResolveRootBodyForward()
        {
            // Must match locomotion torso facing — not root transform.
            // Root stays camera-aligned; while walking S the body faces velocity, so clamping
            // against transform.forward lets fire go "through the back".
            if (bodyFacing == null)
                bodyFacing = GetComponent<IBodyFacingProvider>();

            if (bodyFacing != null)
            {
                Vector3 logical = bodyFacing.LogicalBodyForward;
                logical.y = 0f;
                if (logical.sqrMagnitude > 0.0001f)
                    return logical.normalized;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                return Vector3.forward;

            return forward.normalized;
        }

        private Vector3 ResolveRawAim(Vector3 fallbackForward)
        {
            if (cursorAim != null && cursorAim.AimDirection.sqrMagnitude > 0.0001f)
            {
                Vector3 aim = cursorAim.AimDirection;
                aim.y = 0f;
                if (aim.sqrMagnitude > 0.0001f)
                    return aim.normalized;
            }

            return fallbackForward;
        }
    }
}
