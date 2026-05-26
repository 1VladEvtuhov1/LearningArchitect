using LearningArchitect.Modules.InterviewArena;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.InterviewArena
{
    public sealed class PlayerLocomotionMathTests
    {
        [Test]
        public void ProjectOnGround_FollowsSlopeNormal()
        {
            Vector3 wish = Vector3.forward;
            Vector3 normal = new Vector3(0.2f, 0.98f, 0f).normalized;
            Vector3 projected = PlayerLocomotionMath.ProjectOnGround(wish, normal);

            Assert.Greater(projected.y, 0f);
            Assert.Less(Vector3.Angle(projected, normal), 2f);
        }

        [Test]
        public void IsWalkableNormal_RejectsSteepSurfaces()
        {
            Vector3 steep = new Vector3(0f, 0.2f, 0.98f).normalized;
            Assert.IsFalse(PlayerLocomotionMath.IsWalkableNormal(steep, 45f));
            Assert.IsTrue(PlayerLocomotionMath.IsWalkableNormal(Vector3.up, 45f));
        }
    }
}
