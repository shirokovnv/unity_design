using System;
using System.Collections.Generic;

namespace Architecture
{
    public class Bloc<BlocEvent, BlocState> : IBloc<BlocEvent, BlocState>
    where BlocState : IEquatable<BlocState>
    {
        protected EventHandler<BlocState> stateChanged;
        protected BlocState state;
        protected Dictionary<Type, Func<BlocEvent, BlocState>> handlers;

        event EventHandler<BlocState> IBloc<BlocEvent, BlocState>.StateChanged
        {
            add
            {
                stateChanged += value;
            }

            remove
            {
                stateChanged -= value;
            }
        }

        public Bloc(BlocState initialState)
        {
            state = initialState;
            handlers = new Dictionary<Type, Func<BlocEvent, BlocState>>();
        }

        public void AddEvent(BlocEvent blocEvent)
        {
            var newState = handlers.ContainsKey(blocEvent.GetType())
                ? handlers[blocEvent.GetType()](blocEvent)
                : default;

            if (!newState.Equals(state))
            {
                state = newState;
                OnStateChanged(newState);
            }
        }

        protected virtual void OnStateChanged(BlocState state)
        {
            stateChanged?.Invoke(this, state);
        }

        protected virtual void On<T>(Func<BlocEvent, BlocState> handler) where T : BlocEvent
        {
            if (!handlers.ContainsKey(typeof(T)))
            {
                handlers.Add(typeof(T), handler);
            }
        }
    }
}
