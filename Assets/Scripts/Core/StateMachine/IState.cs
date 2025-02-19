using System;

namespace Architecture
{
    public interface IState : IEquatable<IState>
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
        void OnFixedUpdate();
    }
}

