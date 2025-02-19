using System;
using System.Collections.Generic;
using System.Linq;

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
            current = GetStateByType(state.GetType());
            current?.OnEnter();
        }

        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            if (from == null || to == null)
            {
                throw new ArgumentException("State cannot be null.");
            }

            var stateFrom = GetOrAddState(from);
            var stateTo = GetOrAddState(to);

            states[stateFrom].Add(new Transition(stateFrom, stateTo, condition));
        }

        public void AddAnyTransition(IState to, IPredicate condition)
        {
            anyTransitions.Add(new Transition(null, GetOrAddState(to), condition));
        }

        public void RemoveAnyTransition(IState to, IPredicate condition)
        {
            var stateTo = GetStateByType(to.GetType());

            anyTransitions.RemoveWhere(transition => transition.To == stateTo && transition.Condition == condition);
        }

        public void RemoveTransition(IState from, IState to, IPredicate condition)
        {
            var stateFrom = GetStateByType(from.GetType());
            var stateTo = GetStateByType(from.GetType());

            states[stateFrom].RemoveWhere(transition => transition.To == stateTo && transition.Condition == condition);
        }

        public void ClearAnyTransitions()
        {
            anyTransitions.Clear();
        }

        void ChangeState(IState state)
        {
            if (state == current || state.GetType() == current.GetType()) return;

            var previousState = current;
            var nextState = GetStateByType(state.GetType());

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
            var searchState = GetStateByType(state.GetType());
            if (searchState == null)
            {
                states.Add(searchState, new HashSet<ITransition>());
            }

            return searchState;
        }

        IState GetStateByType(Type stateType)
        {
            return states.Keys.Where(state => state.GetType() == stateType).FirstOrDefault();
        }
    }
}

