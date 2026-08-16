using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class BowAimDirectionProviderTests
    {
        [Test]
        public void Clamp_AtPositiveLimit_MatchesPlus75()
        {
            var go = CreateProviderRoot(out ArenaCursorAim cursor, out BowAimDirectionProvider provider);
            SetCursorAim(cursor, Quaternion.AngleAxis(120f, Vector3.up) * Vector3.forward);
            provider.Recalculate();

            float yaw = Vector3.SignedAngle(provider.BodyForward, provider.TargetClampedAimDirection, Vector3.up);
            Assert.AreEqual(75f, yaw, 0.05f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Clamp_AtNegativeLimit_MatchesMinus75()
        {
            var go = CreateProviderRoot(out ArenaCursorAim cursor, out BowAimDirectionProvider provider);
            SetCursorAim(cursor, Quaternion.AngleAxis(-120f, Vector3.up) * Vector3.forward);
            provider.Recalculate();

            float yaw = Vector3.SignedAngle(provider.BodyForward, provider.TargetClampedAimDirection, Vector3.up);
            Assert.AreEqual(-75f, yaw, 0.05f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Near180_KeepsLastSideAcrossFrames()
        {
            var go = CreateProviderRoot(out ArenaCursorAim cursor, out BowAimDirectionProvider provider);
            provider.SetLastAimYawSignForTests(1f);

            SetCursorAim(cursor, Quaternion.AngleAxis(179f, Vector3.up) * Vector3.forward);
            provider.Recalculate();
            float yawA = Vector3.SignedAngle(provider.BodyForward, provider.TargetClampedAimDirection, Vector3.up);

            SetCursorAim(cursor, Quaternion.AngleAxis(-179f, Vector3.up) * Vector3.forward);
            provider.Recalculate();
            float yawB = Vector3.SignedAngle(provider.BodyForward, provider.TargetClampedAimDirection, Vector3.up);

            Assert.Greater(yawA, 0f);
            Assert.Greater(yawB, 0f, "Hysteresis should keep the previous side near ±180.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Provider_RecalculatesRegardlessOfStance()
        {
            var go = CreateProviderRoot(out ArenaCursorAim cursor, out BowAimDirectionProvider provider);
            go.AddComponent<PlayerInputReader>();
            var stance = go.AddComponent<PlayerCombatStance>();

            stance.SetStance(ArenaCombatStance.MeleeReady);
            SetCursorAim(cursor, Quaternion.AngleAxis(40f, Vector3.up) * Vector3.forward);
            provider.Recalculate();

            float yaw = Vector3.SignedAngle(provider.BodyForward, provider.TargetClampedAimDirection, Vector3.up);
            Assert.AreEqual(40f, yaw, 0.1f);
            Assert.IsFalse(stance.IsBowAimStance);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ClampedAim_DiffersFromRearFacingMuzzleForward()
        {
            var go = CreateProviderRoot(out ArenaCursorAim cursor, out BowAimDirectionProvider provider);

            var muzzle = new GameObject("CrossbowMuzzle").transform;
            muzzle.SetParent(go.transform, false);
            muzzle.localPosition = new Vector3(0.2f, 1.1f, 0.4f);
            muzzle.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);

            SetCursorAim(cursor, Quaternion.AngleAxis(90f, Vector3.up) * Vector3.forward);
            provider.Recalculate();

            Vector3 expected = provider.TargetClampedAimDirection;
            Assert.AreEqual(75f, Vector3.SignedAngle(Vector3.forward, expected, Vector3.up), 0.1f);
            Assert.Greater(Vector3.Dot(expected, Vector3.right), 0.5f);
            Assert.Less(Vector3.Dot(muzzle.forward, expected), 0f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RebuildSpawn_WithRearMuzzle_KeepsRadiusAlongClampedAim()
        {
            Vector3 root = Vector3.zero;
            Vector3 rearMuzzle = new Vector3(0f, 1.1f, -0.55f);
            Vector3 clampedAim = Vector3.right;

            Vector3 spawn = CrossbowWeaponController.RebuildSpawnAlongFireDirection(
                root,
                rearMuzzle,
                clampedAim);

            Assert.AreEqual(1.1f, spawn.y, 0.001f);
            Assert.AreEqual(0.55f, new Vector3(spawn.x, 0f, spawn.z).magnitude, 0.001f);
            Assert.AreEqual(0.55f, spawn.x, 0.001f);
            Assert.AreEqual(0f, spawn.z, 0.001f);
            Assert.Greater(Vector3.Dot(spawn - root, clampedAim), 0.5f);
        }

        [Test]
        public void DefaultStance_IsMeleeReady_UntilBowAimSet()
        {
            var go = new GameObject(nameof(DefaultStance_IsMeleeReady_UntilBowAimSet));
            go.AddComponent<PlayerInputReader>();
            var stance = go.AddComponent<PlayerCombatStance>();

            Assert.IsFalse(stance.IsBowAimStance);
            stance.SetStance(ArenaCombatStance.BowAim);
            Assert.IsTrue(stance.IsBowAimStance);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Clamp_UsesLogicalBodyForward_WhenWalkingBackward()
        {
            // Root still faces +Z (camera-forward); torso faces -Z like walk-S locomotion.
            // Cursor at +Z must clamp relative to torso, not root — else fire goes through the back.
            var go = CreateProviderRoot(out ArenaCursorAim cursor, out BowAimDirectionProvider provider);
            var bodyFacing = go.AddComponent<FakeBodyFacingProvider>();
            bodyFacing.LogicalBodyForward = Vector3.back;

            SetCursorAim(cursor, Vector3.forward);
            provider.Recalculate();

            Assert.Greater(
                Vector3.Dot(provider.BodyForward, Vector3.back),
                0.99f,
                "Body forward must follow logical torso.");
            float yaw = Vector3.SignedAngle(provider.BodyForward, provider.TargetClampedAimDirection, Vector3.up);
            Assert.AreEqual(75f, Mathf.Abs(yaw), 0.05f);
            Assert.Greater(
                Vector3.Dot(provider.TargetClampedAimDirection, Vector3.back),
                0.2f,
                "Clamped aim must stay near back-facing torso, not world forward.");
            Assert.Less(Vector3.Dot(provider.TargetClampedAimDirection, Vector3.forward), 0f);

            Object.DestroyImmediate(go);
        }

        private static GameObject CreateProviderRoot(
            out ArenaCursorAim cursor,
            out BowAimDirectionProvider provider)
        {
            var go = new GameObject("BowAimProviderTest");
            go.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            cursor = go.AddComponent<ArenaCursorAim>();
            provider = go.AddComponent<BowAimDirectionProvider>();
            return go;
        }

        private static void SetCursorAim(ArenaCursorAim cursor, Vector3 planarDirection)
        {
            cursor.SetAimDirectionForTests(planarDirection);
        }

        private sealed class FakeBodyFacingProvider : MonoBehaviour, IBodyFacingProvider
        {
            public Vector3 LogicalBodyForward { get; set; } = Vector3.forward;
            public float PlanarAimLimitDegrees => 75f;
        }
    }
}
