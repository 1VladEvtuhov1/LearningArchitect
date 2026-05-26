using LearningArchitect.Modules.InterviewArena;
using NUnit.Framework;

namespace LearningArchitect.Tests.InterviewArena
{
    public sealed class EnemyFsmLogicTests
    {
        [Test]
        public void CanSeePlayer_RespectsRangeAndForwardCone()
        {
            Assert.IsTrue(EnemyFsmLogic.CanSeePlayer(5f, 0.5f, 9f, -0.15f));
            Assert.IsFalse(EnemyFsmLogic.CanSeePlayer(12f, 0.5f, 9f, -0.15f));
            Assert.IsFalse(EnemyFsmLogic.CanSeePlayer(5f, -0.9f, 9f, -0.15f));
        }

        [Test]
        public void IsInCrossbowRange_RequiresBandBetweenMinAndMax()
        {
            Assert.IsTrue(EnemyFsmLogic.IsInCrossbowRange(5f, 2f, 11f));
            Assert.IsFalse(EnemyFsmLogic.IsInCrossbowRange(1f, 2f, 11f));
            Assert.IsFalse(EnemyFsmLogic.IsInCrossbowRange(12f, 2f, 11f));
        }

        [Test]
        public void ResolveNextState_ChaseWhenPlayerVisibleButOutOfWeaponRanges()
        {
            EnemyState next = EnemyFsmLogic.ResolveNextState(
                EnemyState.Patrol,
                canSeePlayer: true,
                shouldLosePlayer: false,
                inMeleeRange: false,
                inCrossbowRange: false,
                attackCycleActive: false,
                rangedCycleActive: false);

            Assert.AreEqual(EnemyState.Chase, next);
        }

        [Test]
        public void ResolveNextState_AttackWhenInMeleeRange()
        {
            EnemyState next = EnemyFsmLogic.ResolveNextState(
                EnemyState.Chase,
                canSeePlayer: true,
                shouldLosePlayer: false,
                inMeleeRange: true,
                inCrossbowRange: true,
                attackCycleActive: false,
                rangedCycleActive: false);

            Assert.AreEqual(EnemyState.Attack, next);
        }

        [Test]
        public void ResolveNextState_RangedWhenInCrossbowBandOnly()
        {
            EnemyState next = EnemyFsmLogic.ResolveNextState(
                EnemyState.Chase,
                canSeePlayer: true,
                shouldLosePlayer: false,
                inMeleeRange: false,
                inCrossbowRange: true,
                attackCycleActive: false,
                rangedCycleActive: false);

            Assert.AreEqual(EnemyState.Ranged, next);
        }

        [Test]
        public void ResolveNextState_ReturnsPatrolWhenPlayerLost()
        {
            EnemyState next = EnemyFsmLogic.ResolveNextState(
                EnemyState.Chase,
                canSeePlayer: true,
                shouldLosePlayer: true,
                inMeleeRange: false,
                inCrossbowRange: false,
                attackCycleActive: false,
                rangedCycleActive: false);

            Assert.AreEqual(EnemyState.Patrol, next);
        }
    }
}
