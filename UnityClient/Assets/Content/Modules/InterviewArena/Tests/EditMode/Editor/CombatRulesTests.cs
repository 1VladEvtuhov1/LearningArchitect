using LearningArchitect.Modules.InterviewArena;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Tests.InterviewArena
{
    public sealed class CombatRulesTests
    {
        [Test]
        public void CanDamage_AllowsPlayerAgainstEnemy()
        {
            Assert.IsTrue(CombatRules.CanDamage(CombatTeam.Player, CombatTeam.Enemy));
        }

        [Test]
        public void CanDamage_BlocksFriendlyFire()
        {
            Assert.IsFalse(CombatRules.CanDamage(CombatTeam.Player, CombatTeam.Player));
            Assert.IsFalse(CombatRules.CanDamage(CombatTeam.Enemy, CombatTeam.Enemy));
        }

        [Test]
        public void CanDamage_BlocksNeutralTargets()
        {
            Assert.IsFalse(CombatRules.CanDamage(CombatTeam.Player, CombatTeam.Neutral));
        }

        [Test]
        public void HitReact_MapsIncomingOppositeOfTravel()
        {
            Vector3 forward = Vector3.forward;
            Assert.AreEqual(HitReactDirection.Front, HitReactDirectionRules.Resolve(forward, Vector3.back));
            Assert.AreEqual(HitReactDirection.Back, HitReactDirectionRules.Resolve(forward, Vector3.forward));
            Assert.AreEqual(HitReactDirection.Right, HitReactDirectionRules.Resolve(forward, Vector3.left));
            Assert.AreEqual(HitReactDirection.Left, HitReactDirectionRules.Resolve(forward, Vector3.right));
        }

        [Test]
        public void Block_IsFrontalWhenStrikeTravelsIntoFacingCone()
        {
            Vector3 forward = Vector3.forward;
            Assert.IsTrue(MeleeBlockRules.IsFrontalBlock(forward, Vector3.back, 70f));
            Assert.IsFalse(MeleeBlockRules.IsFrontalBlock(forward, Vector3.forward, 70f));

            Vector3 sixty = Quaternion.Euler(0f, 60f, 0f) * Vector3.back;
            Vector3 eighty = Quaternion.Euler(0f, 80f, 0f) * Vector3.back;
            Assert.IsTrue(MeleeBlockRules.IsFrontalBlock(forward, sixty, 70f));
            Assert.IsFalse(MeleeBlockRules.IsFrontalBlock(forward, eighty, 70f));
        }
    }
}
