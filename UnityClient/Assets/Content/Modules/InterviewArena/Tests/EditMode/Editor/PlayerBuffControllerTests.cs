using LearningArchitect.Modules.InterviewArena;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LearningArchitect.Tests.InterviewArena
{
    public sealed class PlayerBuffControllerTests
    {
        [Test]
        public void GetMultiplier_ReturnsOne_WhenNoBuffActive()
        {
            var controller = new GameObject("BuffController").AddComponent<PlayerBuffController>();

            Assert.AreEqual(1f, controller.GetMultiplier(BuffKind.MoveSpeed));
            Assert.AreEqual(1f, controller.GetMultiplier(BuffKind.MeleeDamage));

            Object.DestroyImmediate(controller.gameObject);
        }

        [Test]
        public void ApplyBuff_UsesConfiguredMultiplier()
        {
            var controller = new GameObject("BuffController").AddComponent<PlayerBuffController>();
            BuffPickupConfig config = ScriptableObject.CreateInstance<BuffPickupConfig>();

            SetConfig(config, BuffKind.MoveSpeed, 1.5f, 4f);
            controller.ApplyBuff(config);

            Assert.AreEqual(1.5f, controller.GetMultiplier(BuffKind.MoveSpeed), 0.001f);

            Object.DestroyImmediate(config);
            Object.DestroyImmediate(controller.gameObject);
        }

        [Test]
        public void ApplyBuff_RefreshesDuration_AndKeepsStrongerMultiplier()
        {
            var controller = new GameObject("BuffController").AddComponent<PlayerBuffController>();
            BuffPickupConfig weak = ScriptableObject.CreateInstance<BuffPickupConfig>();
            BuffPickupConfig strong = ScriptableObject.CreateInstance<BuffPickupConfig>();

            SetConfig(weak, BuffKind.MeleeDamage, 1.2f, 2f);
            SetConfig(strong, BuffKind.MeleeDamage, 1.6f, 5f);

            controller.ApplyBuff(weak);
            controller.ApplyBuff(strong);

            Assert.AreEqual(1.6f, controller.GetMultiplier(BuffKind.MeleeDamage), 0.001f);
            Assert.AreEqual(5f, controller.GetRemaining(BuffKind.MeleeDamage), 0.001f);

            Object.DestroyImmediate(weak);
            Object.DestroyImmediate(strong);
            Object.DestroyImmediate(controller.gameObject);
        }

        private static void SetConfig(BuffPickupConfig config, BuffKind kind, float multiplier, float duration)
        {
            SerializedObject serialized = new SerializedObject(config);
            serialized.FindProperty("kind").enumValueIndex = (int)kind;
            serialized.FindProperty("multiplier").floatValue = multiplier;
            serialized.FindProperty("duration").floatValue = duration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
