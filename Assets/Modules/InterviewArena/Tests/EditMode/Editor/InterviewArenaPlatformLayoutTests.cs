using LearningArchitect.Modules.InterviewArena;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.InterviewArena
{
    public sealed class InterviewArenaPlatformLayoutTests
    {
        [Test]
        public void ClampToSurface_KeepsPointInsidePlatformDisc()
        {
            Vector3 outside = new Vector3(50f, -2f, 50f);
            const float halfHeight = 1f;
            Vector3 clamped = InterviewArenaPlatformLayout.ClampToSurface(outside, halfHeight);

            Assert.GreaterOrEqual(clamped.y, InterviewArenaPlatformLayout.ResolveBodyCenterY(halfHeight));
            Assert.LessOrEqual(
                new Vector2(clamped.x, clamped.z).magnitude,
                InterviewArenaPlatformLayout.ResolvePlanarRadius(null));
        }

        [Test]
        public void ResolvePlanarRadius_UsesColliderBounds()
        {
            var platform = new GameObject("TestPlatform");
            try
            {
                var box = platform.AddComponent<BoxCollider>();
                box.size = new Vector3(40f, 1f, 30f);
                platform.transform.position = Vector3.zero;

                float radius = InterviewArenaPlatformLayout.ResolvePlanarRadius(box, 0.35f);
                Assert.GreaterOrEqual(radius, 19f);
            }
            finally
            {
                Object.DestroyImmediate(platform);
            }
        }
    }
}
