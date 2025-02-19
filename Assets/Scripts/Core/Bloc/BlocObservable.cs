using System;
using System.Collections.Generic;

namespace Architecture
{
    public class BlocObservable<BlocEvent, BlocState> :
        Bloc<BlocEvent, BlocState>,
        IBlocObservable<BlocEvent, BlocState>,
        IDisposable
    where BlocState : IEquatable<BlocState>
    {
        private readonly List<IBlocConsumer<BlocEvent, BlocState>> consumers = new();

        public BlocObservable(BlocState initialState) : base(initialState)
        {
        }

        ~BlocObservable()
        {
            Dispose();
        }

        public void Dispose()
        {
            consumers.ForEach(consumer =>
            {
                consumer.Dispose();
                SubscriptionUtils.Dispose(ref stateChanged);
            });
        }

        public void Subscribe(IBlocConsumer<BlocEvent, BlocState> observer)
        {
            if (!consumers.Contains(observer))
            {
                consumers.Add(observer);
                observer.OnEvent += OnEvent;
                stateChanged += observer.OnStateChanged;
            }
        }

        public void Unsubscribe(IBlocConsumer<BlocEvent, BlocState> observer)
        {
            if (consumers.Contains(observer))
            {
                observer.OnEvent -= OnEvent;
                stateChanged -= observer.OnStateChanged;
                consumers.Remove(observer);
            }
        }

        protected void OnEvent(object sender, BlocEvent blocEvent)
        {
            AddEvent(blocEvent);
        }
    }
}
