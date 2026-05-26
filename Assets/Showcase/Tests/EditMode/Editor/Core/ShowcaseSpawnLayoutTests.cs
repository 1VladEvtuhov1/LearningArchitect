using LearningArchitect.Core;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.Core
{
    [Category("LearningArchitect.Core.Edit")]
    public sealed class ShowcaseSpawnLayoutTests
    {
        [Test]
        public void RandomPointOnPlatform_SpawnsAbovePlatformTop()
        {
            for (int i = 0; i < 32; i++)
            {
                Vector3 point = ShowcaseSpawnLayout.RandomPointOnPlatform(7f, 0.2f);
                Assert.GreaterOrEqual(point.y, ShowcaseSpawnLayout.PlatformTopY);
                Assert.LessOrEqual(new Vector2(point.x, point.z).magnitude, ShowcaseSpawnLayout.PlatformRadiusXZ + 0.01f);
            }
        }

        [Test]
        public void ClampToSurface_PreventsPositionsBelowPlatform()
        {
            Vector3 clamped = ShowcaseSpawnLayout.ClampToSurface(new Vector3(1f, -2f, 1f));
            Assert.AreEqual(ShowcaseSpawnLayout.SurfaceY, clamped.y);
        }
    }
}
