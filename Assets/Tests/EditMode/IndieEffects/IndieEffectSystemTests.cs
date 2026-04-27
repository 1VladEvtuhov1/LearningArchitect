using LearningArchitect.IndieEffects;
using NUnit.Framework;

namespace LearningArchitect.Tests
{
    public sealed class IndieEffectSystemTests
    {
        [Test]
        public void DamageUsesShieldBeforeHealth()
        {
            SimulationSystem sim = new SimulationSystem(1, 4);
            Entity target = sim.CreateEntity(100f, 50f, 4f);

            Assert.IsTrue(sim.AddShield(target, 100f, 1f));
            Assert.IsTrue(sim.AddDot(target, 20f, 1f));

            sim.Tick(0.5f);

            Health health = sim.GetHealth(target);
            Shield shield = sim.GetShield(target);

            Assert.AreEqual(100f, health.Value, 0.001f);
            Assert.AreEqual(40f, shield.Value, 0.001f);
            Assert.AreEqual(1, sim.Metrics.appliedEntities);
        }

        [Test]
        public void HealClampsToMaxHealth()
        {
            SimulationSystem sim = new SimulationSystem(1, 4);
            Entity target = sim.CreateEntity(100f, 0f, 4f);

            Assert.IsTrue(sim.AddDot(target, 80f, 1f));
            sim.Tick(0.5f);
            Assert.AreEqual(60f, sim.GetHealth(target).Value, 0.001f);

            Assert.IsTrue(sim.AddHeal(target, 200f, 1f));
            sim.Tick(1f);

            Assert.AreEqual(100f, sim.GetHealth(target).Value, 0.001f);
        }

        [Test]
        public void SlowAppliesStrongestMultiplierAndThenExpires()
        {
            SimulationSystem sim = new SimulationSystem(1, 4);
            Entity target = sim.CreateEntity(100f, 0f, 10f);

            Assert.IsTrue(sim.AddSlow(target, 0.8f, 0.5f));
            Assert.IsTrue(sim.AddSlow(target, 0.4f, 0.5f));

            sim.Tick(0.25f);
            Assert.AreEqual(4f, sim.GetMovement(target).CurrentSpeed, 0.001f);

            sim.Tick(0.25f);
            Assert.AreEqual(0, sim.Data.Slows.Count);

            sim.Tick(0.1f);
            Assert.AreEqual(10f, sim.GetMovement(target).CurrentSpeed, 0.001f);
        }

        [Test]
        public void ExpiredEffectsAreCleanedFromSeparateStorages()
        {
            SimulationSystem sim = new SimulationSystem(1, 8);
            Entity target = sim.CreateEntity(100f, 20f, 5f);

            Assert.IsTrue(sim.AddDot(target, 1f, 0.1f));
            Assert.IsTrue(sim.AddHeal(target, 1f, 0.1f));
            Assert.IsTrue(sim.AddShield(target, 1f, 0.1f));
            Assert.IsTrue(sim.AddSlow(target, 0.5f, 0.1f));

            sim.Tick(0.1f);

            Assert.AreEqual(0, sim.Data.Dots.Count);
            Assert.AreEqual(0, sim.Data.Heals.Count);
            Assert.AreEqual(0, sim.Data.ShieldEffects.Count);
            Assert.AreEqual(0, sim.Data.Slows.Count);
        }

        [Test]
        public void ManyEffectsOnOneTargetAggregateBeforeApply()
        {
            SimulationSystem sim = new SimulationSystem(1, 128);
            Entity target = sim.CreateEntity(1000f, 0f, 5f);

            for (int i = 0; i < 100; i++)
                Assert.IsTrue(sim.AddDot(target, 1f, 10f));

            sim.Tick(1f);

            Assert.AreEqual(900f, sim.GetHealth(target).Value, 0.001f);
            Assert.AreEqual(100, sim.Metrics.processedEffects);
            Assert.AreEqual(1, sim.Metrics.appliedEntities);
            Assert.AreEqual(100f, sim.Metrics.damageApplied, 0.001f);
        }

        [Test]
        public void SimultaneousEffectsUseCentralApplyFormula()
        {
            SimulationSystem sim = new SimulationSystem(1, 8);
            Entity target = sim.CreateEntity(100f, 50f, 10f);

            Assert.IsTrue(sim.AddDot(target, 80f, 1f));
            Assert.IsTrue(sim.AddHeal(target, 30f, 1f));
            Assert.IsTrue(sim.AddShield(target, 20f, 1f));
            Assert.IsTrue(sim.AddSlow(target, 0.5f, 1f));

            sim.Tick(1f);

            Assert.AreEqual(70f, sim.GetHealth(target).Value, 0.001f);
            Assert.AreEqual(0f, sim.GetShield(target).Value, 0.001f);
            Assert.AreEqual(5f, sim.GetMovement(target).CurrentSpeed, 0.001f);
            Assert.AreEqual(4, sim.Metrics.processedEffects);
            Assert.AreEqual(1, sim.Metrics.appliedEntities);
        }

        [Test]
        public void AddOrderDoesNotChangeSimultaneousResult()
        {
            SimulationSystem first = new SimulationSystem(1, 8);
            Entity firstTarget = first.CreateEntity(100f, 50f, 10f);
            first.AddDot(firstTarget, 80f, 1f);
            first.AddHeal(firstTarget, 30f, 1f);
            first.AddShield(firstTarget, 20f, 1f);
            first.AddSlow(firstTarget, 0.5f, 1f);

            SimulationSystem second = new SimulationSystem(1, 8);
            Entity secondTarget = second.CreateEntity(100f, 50f, 10f);
            second.AddSlow(secondTarget, 0.5f, 1f);
            second.AddShield(secondTarget, 20f, 1f);
            second.AddHeal(secondTarget, 30f, 1f);
            second.AddDot(secondTarget, 80f, 1f);

            first.Tick(1f);
            second.Tick(1f);

            Assert.AreEqual(first.GetHealth(firstTarget).Value, second.GetHealth(secondTarget).Value, 0.001f);
            Assert.AreEqual(first.GetShield(firstTarget).Value, second.GetShield(secondTarget).Value, 0.001f);
            Assert.AreEqual(first.GetMovement(firstTarget).CurrentSpeed, second.GetMovement(secondTarget).CurrentSpeed, 0.001f);
        }
    }
}
