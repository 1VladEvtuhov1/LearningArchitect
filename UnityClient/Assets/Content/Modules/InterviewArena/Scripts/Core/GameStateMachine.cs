using System;

namespace LearningArchitect.Modules.InterviewArena
{
    public sealed class GameStateMachine
    {
        public GameState Current { get; private set; } = GameState.Boot;

        public event Action<GameState> StateChanged;

        public void Enter(GameState nextState)
        {
            if (Current == nextState)
                return;

            Current = nextState;
            StateChanged?.Invoke(Current);
        }
    }
}
