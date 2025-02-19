using System;

namespace Architecture
{
    public class ReactiveProperty<T> : IEquatable<ReactiveProperty<T>>, IDisposable
        where T : IEquatable<T>
    {
        public event Action<T> OnChanged;

        private T value;

        public T Value
        {
            get => value;
            set
            {
                if (!Equals(this.value, value))
                {
                    this.value = value;
                    OnChanged?.Invoke(value);
                }
            }
        }

        public ReactiveProperty(T initialValue)
        {
            value = initialValue;
        }

        public bool Equals(ReactiveProperty<T> other)
        {
            if (other is null)
            {
                return false;
            }

            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is not ReactiveProperty<T>)
            {
                return false;
            }

            return Equals((ReactiveProperty<T>)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public void Dispose()
        {
            SubscriptionUtils.Dispose(ref OnChanged);
        }

        ~ReactiveProperty()
        {
            Dispose();
        }
    }
}
