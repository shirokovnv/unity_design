namespace Architecture
{
    public class Transition : ITransition
    {
        public IState From { get; }
        public IState To { get; }
        public IPredicate Condition { get; }

        public Transition(IState from, IState to, IPredicate condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }
    }
}
