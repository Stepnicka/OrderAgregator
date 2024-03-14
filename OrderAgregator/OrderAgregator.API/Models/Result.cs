namespace OrderAgregator.API.Models
{
    public readonly struct Result<TValue, TError>
    {
        private readonly TValue? _value;
        private readonly TError? _error;

        public bool IsSuccess { get; }

        private Result(TValue value)
        {
            _value = value;
            IsSuccess = true;
        }

        private Result(TError error)
        {
            _error = error;
            _value = default;
            IsSuccess = false;
        }

        public static implicit operator Result<TValue, TError>(TValue value) => new(value);
        public static implicit operator Result<TValue, TError>(TError error) => new(error);

        public TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure) => IsSuccess ? success(_value!) : failure(_error!);
    }

    public class Of<T0, T1> 
    {
        private readonly T0? _value0;
        private readonly T1? _value1;

        private readonly int _index;

        protected Of(T0 value)
        {
            _value0 = value;
            _index = 0;

            _value1 = default;
        }

        protected Of(T1 value)
        {
            _value1 = value;
            _index = 1;

            _value0 = default;
        }

        public static implicit operator Of<T0, T1>(T0 value) => new(value);
        public static implicit operator Of<T0, T1>(T1 value) => new(value);

        public TResult Match<TResult>(Func<T0, TResult> f0, Func<T1, TResult> f1)
        {
            if (_index == 0 && f0 != null)
            {
                return f0(_value0!);
            }

            if (_index == 1 && f1 != null)
            {
                return f1(_value1!);
            }

            throw new InvalidOperationException();
        }
    }
}
