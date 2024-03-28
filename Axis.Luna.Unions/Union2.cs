namespace Axis.Luna.Unions
{
    public interface IUnion<T1, T2, TSelf>
    where TSelf : IUnion<T1, T2, TSelf>
    {
        protected object? Value { get; }

        bool Is(out T1? value);

        bool Is(out T2? value);

        bool IsNull();

        TOut? MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<TOut>? nullMap = null);

        void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null);

        TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null);
    }

    public interface IUnionOf<T1, T2, TSelf> :
        IUnion<T1, T2, TSelf>
        where TSelf : IUnionOf<T1, T2, TSelf>
    {
        abstract static TSelf Of(T1 value);

        abstract static TSelf Of(T2 value);
    }

    public interface IUnionImplicits<T1, T2, TSelf> :
        IUnion<T1, T2, TSelf>
        where TSelf : IUnionImplicits<T1, T2, TSelf>
    {
        static abstract implicit operator TSelf(T1 value);
        static abstract implicit operator TSelf(T2 value);
    }

    public abstract class RefUnion<T1, T2, TSelf> :
        IUnion<T1, T2, TSelf>
        where TSelf : RefUnion<T1, T2, TSelf>
    {
        private readonly object? _value;

        object? IUnion<T1, T2, TSelf>.Value => _value;

        #region Construction

        // Remove this if a compile-time check for distinct generic types is available
        static RefUnion()
        {
            var types = new HashSet<Type>
            {
                typeof(T1), typeof(T2)
            };

            if (types.Count != 2)
                throw new InvalidOperationException("Invalid generic types: duplicate types found");
        }

        protected RefUnion(object? value)
        {
            _value = value switch
            {
                null => null,
                T1 or T2 => value,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Invalid {nameof(value)} type: '{value.GetType()}'")
            };
        }
        #endregion

        #region Is

        public bool Is(out T1? value) => Is(_value, out value);

        public bool Is(out T2? value) => Is(_value, out value);

        public bool IsNull() => _value is null;

        private static bool Is<T>(object? unionValue, out T? value)
        {
            if (unionValue is T t1)
            {
                value = t1;
                return true;
            }

            value = default;
            return false;
        }

        #endregion

        #region Map

        public TOut? MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<TOut>? nullMap = null)
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);

            if (_value is T1 t1)
                return t1Mapper.Invoke(t1);

            if (_value is T2 t2)
                return t2Mapper.Invoke(t2);

            // unknown type, assume null
            return nullMap switch
            {
                null => default,
                _ => nullMap.Invoke()
            };
        }

        #endregion

        #region Consume

        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null)
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);

            if (_value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (_value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (_value is null && nullConsumer is not null)
                nullConsumer.Invoke();
        }

        #endregion

        #region With

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null)
        {
            ConsumeMatch(t1Consumer, t2Consumer, nullConsumer);
            return (TSelf)this;
        }

        #endregion
    }

    public readonly struct ValueUnion<T1, T2> :
        IUnion<T1, T2, ValueUnion<T1, T2>>
    {
        private readonly object? _value;

        object? IUnion<T1, T2, ValueUnion<T1, T2>>.Value => _value;

        #region Construction

        // Remove this if a compile-time check for distinct generic types is available
        static ValueUnion()
        {
            var types = new HashSet<Type>
            {
                typeof(T1), typeof(T2)
            };

            if (types.Count != 2)
                throw new InvalidOperationException("Invalid generic types: duplicate types found");
        }

        public ValueUnion(object value)
        {
            _value = value switch
            {
                null => null,
                T1 or T2 => value,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Invalid {nameof(value)} type: '{value.GetType()}'")
            };
        }
        #endregion

        #region Is

        public bool Is(out T1? value) => Is(_value, out value);

        public bool Is(out T2? value) => Is(_value, out value);

        public bool IsNull() => _value is null;

        private static bool Is<T>(object? unionValue, out T ?value)
        {
            if (unionValue is T t1)
            {
                value = t1;
                return true;
            }

            value = default;
            return false;
        }

        #endregion

        #region Map

        public TOut? MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<TOut>? nullMap = null)
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);

            if (_value is T1 t1)
                return t1Mapper.Invoke(t1);

            if (_value is T2 t2)
                return t2Mapper.Invoke(t2);

            // unknown type, assume null
            return nullMap switch
            {
                null => default,
                _ => nullMap.Invoke()
            };
        }

        #endregion

        #region Consume

        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null)
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);

            if (_value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (_value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (_value is null && nullConsumer is not null)
                nullConsumer.Invoke();
        }

        #endregion

        #region With

        public ValueUnion<T1, T2> WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null)
        {
            ConsumeMatch(t1Consumer, t2Consumer, nullConsumer);
            return this;
        }

        #endregion
    }
}
