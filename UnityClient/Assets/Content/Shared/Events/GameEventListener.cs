using UnityEngine;
using UnityEngine.Events;

namespace LearningArchitect.Shared.Events
{
    [DisallowMultipleComponent]
    public sealed class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private UnityEvent response;

        public GameEvent Event => gameEvent;

        private void OnEnable()
        {
            if (gameEvent != null)
                gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (gameEvent != null)
                gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(GameEvent raisedEvent)
        {
            if (raisedEvent != gameEvent)
                return;

            response?.Invoke();
        }
    }
}
