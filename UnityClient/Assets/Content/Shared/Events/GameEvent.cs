using System.Collections.Generic;
using UnityEngine;

namespace LearningArchitect.Shared.Events
{
    [CreateAssetMenu(
        fileName = "GameEvent",
        menuName = "Learning Architect/Events/Game Event")]
    public sealed class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> listeners = new List<GameEventListener>(8);

        public void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                GameEventListener listener = listeners[i];
                if (listener != null)
                    listener.OnEventRaised(this);
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (listener == null || listeners.Contains(listener))
                return;

            listeners.Add(listener);
        }

        public void UnregisterListener(GameEventListener listener)
        {
            if (listener == null)
                return;

            listeners.Remove(listener);
        }

#if UNITY_EDITOR
        public IReadOnlyList<GameEventListener> GetRuntimeListeners() => listeners;
#endif
    }
}
