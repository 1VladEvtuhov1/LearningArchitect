using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena
{
    /// <summary>
    /// Read-only logical body facing for combat systems that must not depend on visual mesh orientation.
    /// </summary>
    public interface IBodyFacingProvider
    {
        Vector3 LogicalBodyForward { get; }
        float PlanarAimLimitDegrees { get; }
    }
}
