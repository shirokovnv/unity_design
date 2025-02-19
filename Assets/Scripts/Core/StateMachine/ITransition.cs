namespace Architecture
{
    public interface ITransition
    {
        IState From { get; }
        IState To { get; }
        IPredicate Condition { get; }
    }
}
