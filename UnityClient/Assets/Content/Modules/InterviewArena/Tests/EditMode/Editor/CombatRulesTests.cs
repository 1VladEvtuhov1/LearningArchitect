using LearningArchitect.Modules.InterviewArena;
using NUnit.Framework;

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
    }
}
