using System;
using System.Collections.Generic;

namespace Architecture
{
    public class StateMachine
    {
        IState current;
        readonly Dictionary<IState, HashSet<ITransition>> states = new();
        readonly HashSet<ITransition> anyTransitions = new();

        public void Update()
        {
            var transition = GetTransition();
            if (transition != null)
                ChangeState(transition.To);

            current?.OnUpdate();
        }

        public void FixedUpdate()
        {
            current?.OnFixedUpdate();
        }

        public void SetState(IState state)
        {
            current = GetOrAddState(state);
            current?.OnEnter();
        }

        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            if (from == null || to == null)
            {
                throw new ArgumentException("State cannot be null.");
            }

            GetOrAddState(from);
            GetOrAddState(to);

            states[from].Add(new Transition(from, to, condition));
        }

        public void AddAnyTransition(IState to, IPredicate condition)
        {
            anyTransitions.Add(new Transition(null, GetOrAddState(to), condition));
        }

        public void RemoveAnyTransition(IState to, IPredicate condition)
        {
            anyTransitions.RemoveWhere(transition => transition.To == to && transition.Condition == condition);
        }

        public void RemoveTransition(IState from, IState to, IPredicate condition)
        {
            states[from]?.RemoveWhere(transition => transition.To == to && transition.Condition == condition);
        }

        public void ClearAnyTransitions()
        {
            anyTransitions.Clear();
        }

        void ChangeState(IState state)
        {
            if (state == current || state.GetType() == current.GetType()) return;

            var previousState = current;
            var nextState = state;

            previousState?.OnExit();
            nextState?.OnEnter();

            current = nextState;
        }

        ITransition GetTransition()
        {
            foreach (var transition in anyTransitions)
                if (transition.Condition.Evaluate())
                    return transition;

            foreach (var transition in states[current])
                if (transition.Condition.Evaluate())
                    return transition;

            return null;
        }

        IState GetOrAddState(IState state)
        {
            if (!states.ContainsKey(state))
            {
                states.Add(state, new HashSet<ITransition>());
            }

            return state;
        }
    }
}

