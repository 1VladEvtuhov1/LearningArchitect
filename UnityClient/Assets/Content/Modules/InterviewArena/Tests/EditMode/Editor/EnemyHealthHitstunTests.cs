using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class EnemyHealthHitstunTests
    {
        [Test]
        public void ApplyDamage_InterruptsWindupAndStuns()
        {
            var go = new GameObject(nameof(ApplyDamage_InterruptsWindupAndStuns));
            EnemyConfig config = CreateConfigWithHitstun(0.85f);
            try
            {
                Health health = go.AddComponent<Health>();
                health.ApplyConfig(CombatTeam.Enemy, 90f, 0f);
                EnemyMeleeAttack melee = go.AddComponent<EnemyMeleeAttack>();
                go.AddComponent<EnemyCrossbowAttack>();
                EnemyHealthHitstun hitstun = go.AddComponent<EnemyHealthHitstun>();
                hitstun.ApplyConfig(config);

                Assert.IsTrue(melee.TryBeginAttack(config));
                Assert.IsTrue(melee.IsBusy);

                health.ApplyDamage(new DamageInfo(
                    10f,
                    CombatTeam.Player,
                    Vector3.zero,
                    Vector3.back));

                Assert.IsTrue(hitstun.IsStunned);
                Assert.IsFalse(melee.IsBusy);
                Assert.AreEqual(1, melee.MeleeAnimStartVersion);

                melee.Tick(config, 0, 999f);
                Assert.IsFalse(melee.IsBusy);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Tick_ExpiresStun()
        {
            var go = new GameObject(nameof(Tick_ExpiresStun));
            EnemyConfig config = CreateConfigWithHitstun(0.85f);
            try
            {
                Health health = go.AddComponent<Health>();
                health.ApplyConfig(CombatTeam.Enemy, 90f, 0f);
                go.AddComponent<EnemyMeleeAttack>();
                go.AddComponent<EnemyCrossbowAttack>();
                EnemyHealthHitstun hitstun = go.AddComponent<EnemyHealthHitstun>();
                hitstun.ApplyConfig(config);

                health.ApplyDamage(new DamageInfo(
                    10f,
                    CombatTeam.Player,
                    Vector3.zero,
                    Vector3.back));

                Assert.IsTrue(hitstun.IsStunned);
                hitstun.Tick(config.HitstunDuration);
                Assert.IsFalse(hitstun.IsStunned);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(config);
            }
        }

        private static EnemyConfig CreateConfigWithHitstun(float duration)
        {
            EnemyConfig config = ScriptableObject.CreateInstance<EnemyConfig>();
            SerializedObject serialized = new SerializedObject(config);
            serialized.FindProperty("hitstunDuration").floatValue = duration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }
    }
}
