using LearningArchitect.Shared.Events;
using NUnit.Framework;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class GameEventTests
    {
        [Test]
        public void RegisterListener_tracks_runtime_subscribers()
        {
            GameEvent gameEvent = ScriptableObject.CreateInstance<GameEvent>();
            GameObject host = new GameObject("GameEventListenerHost");
            GameEventListener listener = host.AddComponent<GameEventListener>();

            try
            {
                gameEvent.RegisterListener(listener);
                Assert.AreEqual(1, gameEvent.GetRuntimeListeners().Count);

                gameEvent.UnregisterListener(listener);
                Assert.AreEqual(0, gameEvent.GetRuntimeListeners().Count);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(gameEvent);
            }
        }

        [Test]
        public void Raise_does_not_throw_with_registered_listener()
        {
            GameEvent gameEvent = ScriptableObject.CreateInstance<GameEvent>();
            GameObject host = new GameObject("GameEventListenerHost");
            GameEventListener listener = host.AddComponent<GameEventListener>();

            try
            {
                gameEvent.RegisterListener(listener);
                Assert.DoesNotThrow(() => gameEvent.Raise());
            }
            finally
            {
                gameEvent.UnregisterListener(listener);
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(gameEvent);
            }
        }
    }
}
