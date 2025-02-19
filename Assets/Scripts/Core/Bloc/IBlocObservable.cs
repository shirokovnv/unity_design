namespace Architecture
{
    public interface IBlocObservable<BlocEvent, BlocState>
    {
        public void Subscribe(IBlocConsumer<BlocEvent, BlocState> observer);
        public void Unsubscribe(IBlocConsumer<BlocEvent, BlocState> observer);
    }
}
