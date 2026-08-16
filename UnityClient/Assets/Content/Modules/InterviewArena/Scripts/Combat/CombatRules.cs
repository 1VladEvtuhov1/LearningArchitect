using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    public static class CombatRules
    {
        public static bool CanDamage(CombatTeam sourceTeam, CombatTeam targetTeam)
        {
            if (targetTeam == CombatTeam.Neutral)
                return false;

            if (sourceTeam == targetTeam)
                return false;

            return sourceTeam == CombatTeam.Player && targetTeam == CombatTeam.Enemy
                || sourceTeam == CombatTeam.Enemy && targetTeam == CombatTeam.Player;
        }
    }

    public enum HitReactDirection
    {
        Front = 0,
        Back = 1,
        Left = 2,
        Right = 3
    }

    /// <summary>
    /// Maps strike travel direction onto the victim's logical body for hit reacts.
    /// Incoming = opposite of <see cref="DamageInfo.HitDirection"/>.
    /// </summary>
    public static class HitReactDirectionRules
    {
        public static HitReactDirection Resolve(Vector3 bodyForward, Vector3 hitTravelDirection)
        {
            bodyForward.y = 0f;
            hitTravelDirection.y = 0f;
            if (bodyForward.sqrMagnitude < 0.0001f)
                bodyForward = Vector3.forward;
            if (hitTravelDirection.sqrMagnitude < 0.0001f)
                return HitReactDirection.Front;

            Vector3 incoming = -hitTravelDirection.normalized;
            Vector3 local = Quaternion.Inverse(Quaternion.LookRotation(bodyForward.normalized, Vector3.up))
                * incoming;

            if (Mathf.Abs(local.z) >= Mathf.Abs(local.x))
                return local.z >= 0f ? HitReactDirection.Front : HitReactDirection.Back;

            return local.x >= 0f ? HitReactDirection.Right : HitReactDirection.Left;
        }
    }

    public static class MeleeBlockRules
    {
        public const float DefaultConeHalfAngleDegrees = 70f;

        /// <summary>
        /// True when the strike travels into the defender's front cone.
        /// </summary>
        public static bool IsFrontalBlock(
            Vector3 bodyForward,
            Vector3 hitTravelDirection,
            float coneHalfAngleDegrees)
        {
            bodyForward.y = 0f;
            hitTravelDirection.y = 0f;
            if (bodyForward.sqrMagnitude < 0.0001f || hitTravelDirection.sqrMagnitude < 0.0001f)
                return false;

            Vector3 incoming = -hitTravelDirection.normalized;
            float halfAngle = Mathf.Clamp(coneHalfAngleDegrees, 1f, 179f);
            return Vector3.Angle(bodyForward.normalized, incoming) <= halfAngle;
        }
    }
}
