using LearningArchitect.Modules.InterviewArena;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.InterviewArena
{
    public sealed class CombatHitResolverTests
    {
        [Test]
        public void DamageInfo_KnockbackImpulse_RoundTripsThroughConstructor()
        {
            DamageInfo info = new DamageInfo(
                10f,
                CombatTeam.Player,
                Vector3.zero,
                Vector3.forward,
                4.5f);

            Assert.AreEqual(4.5f, info.KnockbackImpulse);
        }
    }
}
