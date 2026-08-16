using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Idle body faces aim. While moving, body follows planar velocity.
    /// Arena animator is melee-only: turn clips play whenever turning and not action-locked.
    /// </summary>
    public static class BodyFacingRules
    {
        public static Vector3 ResolveDesiredForward(
            bool isIdle,
            Vector3 flatVelocity,
            Vector3 aimDirection,
            Vector3 currentBodyForward)
        {
            if (!isIdle)
            {
                if (flatVelocity.sqrMagnitude < 0.0001f)
                    return currentBodyForward;

                return flatVelocity.normalized;
            }

            if (aimDirection.sqrMagnitude < 0.001f)
                return currentBodyForward;

            return aimDirection.normalized;
        }

        public static bool ShouldPlayTurnClip(bool isTurningInPlace, bool actionActive)
        {
            if (actionActive)
                return false;

            return isTurningInPlace;
        }
    }
}
