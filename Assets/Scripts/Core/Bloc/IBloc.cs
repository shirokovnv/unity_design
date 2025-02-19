using System;

namespace Architecture
{
    public interface IBloc<BlocEvent, BlocState> where BlocState : IEquatable<BlocState>
    {
        public event EventHandler<BlocState> StateChanged;
        public void AddEvent(BlocEvent blocEvent);
    }
}
