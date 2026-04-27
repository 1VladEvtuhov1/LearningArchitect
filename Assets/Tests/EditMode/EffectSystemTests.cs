using LearningArchitect.Effects;
using NUnit.Framework;

namespace LearningArchitect.Tests
{
    public sealed class EffectSystemTests
    {
        [Test]
        public void PeriodicDamageAggregatesAndApplies()
        {
            EffectManager manager = new EffectManager(10, 16, 16);
            manager.InitializeHealth(100);

            Assert.IsTrue(manager.TryQueueAddPeriodicDamage(0, 10, 1, 10));
            manager.FlushPendingCommands();
            manager.Tick(1f / 60f);

            Assert.AreEqual(90, manager.GetHealth(0));
            Assert.AreEqual(10, manager.Metrics.appliedDamage);
            Assert.AreEqual(1, manager.Metrics.dirtyEntities);
            Assert.AreEqual(1, manager.Metrics.processedEffects);
        }

        [Test]
        public void HotspotEffectsProduceSingleDirtyEntity()
        {
            EffectManager manager = new EffectManager(10, 128, 128);
            manager.InitializeHealth(1000);

            for (int i = 0; i < 100; i++)
                Assert.IsTrue(manager.TryQueueAddPeriodicDamage(0, 1, 1, 10));

            manager.FlushPendingCommands();
            manager.Tick(1f / 60f);

            Assert.AreEqual(900, manager.GetHealth(0));
            Assert.AreEqual(100, manager.Metrics.appliedDamage);
            Assert.AreEqual(1, manager.Metrics.dirtyEntities);
            Assert.AreEqual(100, manager.Metrics.processedEffects);
        }

        [Test]
        public void ExpiredEffectsAreRemovedInCleanup()
        {
            EffectManager manager = new EffectManager(10, 16, 16);
            manager.InitializeHealth(100);

            Assert.IsTrue(manager.TryQueueAddPeriodicDamage(0, 10, 1, 2));
            manager.FlushPendingCommands();

            manager.Tick(1f / 60f);
            Assert.AreEqual(1, manager.Metrics.totalEffects);

            manager.Tick(1f / 60f);
            Assert.AreEqual(0, manager.Metrics.totalEffects);
        }

        [Test]
        public void ScheduledEffectsDoNotProcessBeforeNextTick()
        {
            const int effectCount = 100000;
            EffectManager manager = new EffectManager(256, effectCount, effectCount);
            manager.InitializeHealth(200000);

            for (int i = 0; i < effectCount; i++)
                Assert.IsTrue(manager.TryQueueAddPeriodicDamage(i & 255, 1, 10, 100));

            manager.FlushPendingCommands();
            manager.Tick(1f / 60f);

            Assert.AreEqual(effectCount, manager.Metrics.totalEffects);
            Assert.AreEqual(0, manager.Metrics.scheduledEffects);
            Assert.AreEqual(0, manager.Metrics.processedEffects);
            Assert.AreEqual(200000, manager.GetHealth(0));
        }

        [Test]
        public void Effects100000ApplyThroughChunkLocalAggregation()
        {
            const int effectCount = 100000;
            EffectManager manager = new EffectManager(256, effectCount, effectCount);
            manager.InitializeHealth(200000);

            for (int i = 0; i < effectCount; i++)
                Assert.IsTrue(manager.TryQueueAddPeriodicDamage(i & 255, 1, 1, 100));

            manager.FlushPendingCommands();
            manager.Tick(1f / 60f);

            Assert.AreEqual(effectCount, manager.Metrics.totalEffects);
            Assert.AreEqual(effectCount, manager.Metrics.scheduledEffects);
            Assert.AreEqual(effectCount, manager.Metrics.processedEffects);
            Assert.AreEqual(256, manager.Metrics.appliedEntities);
            Assert.AreEqual(effectCount, manager.Metrics.appliedDamage);
        }

        [Test]
        public void Hotspot100000EffectsApplyToOneEntity()
        {
            const int effectCount = 100000;
            EffectManager manager = new EffectManager(256, effectCount, effectCount);
            manager.InitializeHealth(200000);

            for (int i = 0; i < effectCount; i++)
                Assert.IsTrue(manager.TryQueueAddPeriodicDamage(0, 1, 1, 100));

            manager.FlushPendingCommands();
            manager.Tick(1f / 60f);

            Assert.AreEqual(effectCount, manager.Metrics.scheduledEffects);
            Assert.AreEqual(effectCount, manager.Metrics.processedEffects);
            Assert.AreEqual(1, manager.Metrics.dirtyEntities);
            Assert.AreEqual(1, manager.Metrics.appliedEntities);
            Assert.AreEqual(100000, manager.GetHealth(0));
        }
    }
}
