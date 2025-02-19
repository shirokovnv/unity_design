using System;

namespace Architecture
{
    public interface IBlocConsumer<BlocEvent, BlocState> : IDisposable
    {
        public event EventHandler<BlocEvent> OnEvent;
        public void OnStateChanged(object sender, BlocState state);
    }
}
