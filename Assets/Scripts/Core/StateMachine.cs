using System;

namespace A2.Core
{
    // Tiny generic state machine for GameManager and AI controller
    public class StateMachine<T> where T : struct, Enum
    {
        public T State { get; private set; }
        public event Action<T,T>? OnStateChanged;

        public StateMachine(T initial) { State = initial; }

        public void SetState(T next)
        {
            if (!next.Equals(State))
            {
                var prev = State;
                State = next;
                OnStateChanged?.Invoke(prev, next);
            }
        }
    }
}