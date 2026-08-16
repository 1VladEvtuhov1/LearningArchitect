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

        [Test]
        public void ApplyDamage_WhenGuardAbsorbs_DoesNotReduceHealthOrRaiseDamaged()
        {
            var go = new GameObject(nameof(ApplyDamage_WhenGuardAbsorbs_DoesNotReduceHealthOrRaiseDamaged));
            go.AddComponent<AbsorbingGuard>();
            Health health = go.AddComponent<Health>();
            health.ApplyConfig(CombatTeam.Player, 50f, 0f);

            bool damaged = false;
            bool blocked = false;
            health.Damaged += (_, __) => damaged = true;
            health.Blocked += (_, __) => blocked = true;

            health.ApplyDamage(new DamageInfo(
                10f,
                CombatTeam.Enemy,
                Vector3.zero,
                Vector3.back));

            Assert.AreEqual(50f, health.CurrentHealth);
            Assert.IsTrue(blocked);
            Assert.IsFalse(damaged);
            Assert.IsFalse(health.IsInvulnerable);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ApplyDamage_WhenFirstGuardRejects_SecondGuardCanAbsorb()
        {
            var go = new GameObject(nameof(ApplyDamage_WhenFirstGuardRejects_SecondGuardCanAbsorb));
            go.AddComponent<RejectingGuard>();
            go.AddComponent<SilentAbsorbingGuard>();
            Health health = go.AddComponent<Health>();
            health.ApplyConfig(CombatTeam.Player, 50f, 0f);

            bool damaged = false;
            bool blocked = false;
            health.Damaged += (_, __) => damaged = true;
            health.Blocked += (_, __) => blocked = true;

            health.ApplyDamage(new DamageInfo(
                10f,
                CombatTeam.Enemy,
                Vector3.zero,
                Vector3.back));

            Assert.AreEqual(50f, health.CurrentHealth);
            Assert.IsFalse(blocked);
            Assert.IsFalse(damaged);

            Object.DestroyImmediate(go);
        }

        private sealed class AbsorbingGuard : MonoBehaviour, IIncomingHitGuard
        {
            public bool TryAbsorb(in DamageInfo incoming, out DamageInfo absorbed)
            {
                absorbed = new DamageInfo(
                    0f,
                    incoming.SourceTeam,
                    incoming.HitPoint,
                    incoming.HitDirection,
                    0f,
                    wasBlocked: true);
                return true;
            }
        }

        private sealed class RejectingGuard : MonoBehaviour, IIncomingHitGuard
        {
            public bool TryAbsorb(in DamageInfo incoming, out DamageInfo absorbed)
            {
                absorbed = incoming;
                return false;
            }
        }

        private sealed class SilentAbsorbingGuard : MonoBehaviour, IIncomingHitGuard
        {
            public bool TryAbsorb(in DamageInfo incoming, out DamageInfo absorbed)
            {
                absorbed = new DamageInfo(
                    0f,
                    incoming.SourceTeam,
                    incoming.HitPoint,
                    incoming.HitDirection,
                    0f,
                    wasBlocked: false);
                return true;
            }
        }
    }

    public sealed class MeleeBlockControllerTests
    {
        [Test]
        public void TickBlock_WhenWanted_SetsBlockingAndLock()
        {
            var go = new GameObject(nameof(TickBlock_WhenWanted_SetsBlockingAndLock));
            var gate = go.AddComponent<PlayerActionCoordinator>();
            var block = go.AddComponent<MeleeBlockController>();

            block.TickBlock(true);

            Assert.IsTrue(block.IsBlocking);
            Assert.AreEqual(1, block.BlockStartVersion);
            Assert.IsFalse(gate.CanMove);
            Assert.IsTrue(gate.CanTurn);
            Assert.IsFalse(gate.CanAttack);
            Assert.IsTrue(gate.CanDash);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void TickBlock_WhenReleased_ClearsGuard()
        {
            var go = new GameObject(nameof(TickBlock_WhenReleased_ClearsGuard));
            var gate = go.AddComponent<PlayerActionCoordinator>();
            var block = go.AddComponent<MeleeBlockController>();

            block.TickBlock(true);
            block.TickBlock(false);

            Assert.IsFalse(block.IsBlocking);
            Assert.IsTrue(gate.CanAttack);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void TryAbsorb_FrontalHit_BlocksDamage()
        {
            var go = new GameObject(nameof(TryAbsorb_FrontalHit_BlocksDamage));
            go.transform.rotation = Quaternion.identity;
            go.AddComponent<PlayerActionCoordinator>();
            Health health = go.AddComponent<Health>();
            health.ApplyConfig(CombatTeam.Player, 80f, 0f);
            var block = go.AddComponent<MeleeBlockController>();
            block.TickBlock(true);

            health.ApplyDamage(new DamageInfo(
                25f,
                CombatTeam.Enemy,
                Vector3.zero,
                Vector3.back));

            Assert.AreEqual(80f, health.CurrentHealth);
            Assert.IsTrue(block.IsBlocking);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void TryAbsorb_RearHit_DoesNotBlock()
        {
            var go = new GameObject(nameof(TryAbsorb_RearHit_DoesNotBlock));
            go.transform.rotation = Quaternion.identity;
            go.AddComponent<PlayerActionCoordinator>();
            Health health = go.AddComponent<Health>();
            health.ApplyConfig(CombatTeam.Player, 80f, 0f);
            var block = go.AddComponent<MeleeBlockController>();
            block.TickBlock(true);

            health.ApplyDamage(new DamageInfo(
                25f,
                CombatTeam.Enemy,
                Vector3.zero,
                Vector3.forward));

            Assert.AreEqual(55f, health.CurrentHealth);
            Assert.IsFalse(block.IsBlocking);

            Object.DestroyImmediate(go);
        }
    }
}
