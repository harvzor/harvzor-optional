using System;

namespace Harvzor.Optional
{
    public struct Optional<T> : IOptional
    {
        private object _value;

        public bool IsDefined { get; private set; }

        public T Value
        {
            get
            {
                return this.IsDefined
                    ? (T)_value
                    : default(T);
            }
            set
            {
                _value = value;
                IsDefined = true;
            }
        }

        public Type GenericType => typeof(T);

        object IOptional.Value
        {
            get => Value;
            set
            {
                IsDefined = true;
                _value = value;
            }
        }

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T> { Value = value };
        }

        public static explicit operator T(Optional<T> optional)
        {
            return optional.Value;
        }
    }
}
