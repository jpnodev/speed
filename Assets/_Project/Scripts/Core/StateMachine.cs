using UnityEngine;

namespace Speed.Core
{
    public class StateMachine
    {
        public State CurrentState { get; private set; }

        public void Initialize(State startingState)
        {
            CurrentState = startingState;
            CurrentState?.Enter();
        }

        public void ChangeState(State newState)
        {
            if (CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Tick()
        {
            CurrentState?.Tick();
        }

        public void FixedTick()
        {
            CurrentState?.FixedTick();
        }
    }
}
