using NUnit.Framework;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class TimedInputBufferTests
    {
        [Test]
        public void Consume_WhenEmpty_ReturnsFalse()
        {
            var buffer = new TimedInputBuffer();
            Assert.IsFalse(buffer.Consume());
        }

        [Test]
        public void Press_ThenConsumeWithinWindow_ReturnsTrueOnce()
        {
            var buffer = new TimedInputBuffer();
            buffer.Press(0.2f);

            Assert.IsTrue(buffer.Consume());
            Assert.IsFalse(buffer.Consume());
        }

        [Test]
        public void Tick_AfterWindowExpires_ConsumeReturnsFalse()
        {
            var buffer = new TimedInputBuffer();
            buffer.Press(0.1f);
            buffer.Tick(0.15f);

            Assert.IsFalse(buffer.Consume());
        }

        [Test]
        public void Press_RefreshesRemainingWindow()
        {
            var buffer = new TimedInputBuffer();
            buffer.Press(0.2f);
            buffer.Tick(0.15f);
            buffer.Press(0.2f);
            buffer.Tick(0.1f);

            Assert.IsTrue(buffer.IsBuffered);
            Assert.IsTrue(buffer.Consume());
        }
    }
}
