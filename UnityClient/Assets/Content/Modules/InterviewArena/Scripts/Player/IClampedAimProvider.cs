using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Authoritative planar aim for projectile and upper-body presentation.
    /// Computes every frame from cursor raw aim + root body forward (stance does not gate calculation).
    /// </summary>
    public interface IClampedAimProvider
    {
        Vector3 RawAimDirection { get; }
        Vector3 BodyForward { get; }
        Vector3 TargetClampedAimDirection { get; }
        float MaxAimYawDegrees { get; }
    }
}
