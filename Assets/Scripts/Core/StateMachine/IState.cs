namespace Architecture
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
        void OnFixedUpdate();
    }
}

