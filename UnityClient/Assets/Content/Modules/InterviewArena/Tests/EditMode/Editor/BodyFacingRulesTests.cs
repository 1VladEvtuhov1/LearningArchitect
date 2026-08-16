using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class BodyFacingRulesTests
    {
        [Test]
        public void Idle_FacesAimDirection()
        {
            Vector3 desired = BodyFacingRules.ResolveDesiredForward(
                isIdle: true,
                flatVelocity: Vector3.zero,
                aimDirection: new Vector3(1f, 0f, 0f),
                currentBodyForward: Vector3.forward);

            Assert.AreEqual(Vector3.right, desired);
        }

        [Test]
        public void Idle_WithoutAim_KeepsCurrentFacing()
        {
            Vector3 current = Vector3.back;
            Vector3 desired = BodyFacingRules.ResolveDesiredForward(
                isIdle: true,
                flatVelocity: Vector3.zero,
                aimDirection: Vector3.zero,
                currentBodyForward: current);

            Assert.AreEqual(current, desired);
        }

        [Test]
        public void Moving_FollowsVelocity_NotAim()
        {
            Vector3 desired = BodyFacingRules.ResolveDesiredForward(
                isIdle: false,
                flatVelocity: new Vector3(0f, 0f, 2f),
                aimDirection: Vector3.right,
                currentBodyForward: Vector3.right);

            Assert.AreEqual(Vector3.forward, desired);
        }

        [Test]
        public void Melee_PlaysTurnClipWhileTurning()
        {
            Assert.IsTrue(BodyFacingRules.ShouldPlayTurnClip(
                isTurningInPlace: true,
                actionActive: false));
        }

        [Test]
        public void ActionLock_DoesNotPlayTurnClip()
        {
            Assert.IsFalse(BodyFacingRules.ShouldPlayTurnClip(
                isTurningInPlace: true,
                actionActive: true));
        }

        [Test]
        public void IdleNotTurning_DoesNotPlayTurnClip()
        {
            Assert.IsFalse(BodyFacingRules.ShouldPlayTurnClip(
                isTurningInPlace: false,
                actionActive: false));
        }
    }
}
