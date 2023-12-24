namespace Axis.Luna.Unions.Types
{
    public interface IUnion<T1, T2>
    {
        /// <summary>
        /// The internal value
        /// </summary>
        protected object Value { get; }

        /// <summary>
        /// Explicitly expresses the "this" of the implementer as an interface
        /// so the explicit members can be accessed
        /// </summary>
        IUnion<T1, T2> AsUnion();

        public bool Is(out T1 value) => Is(Value, out value);

        public bool Is(out T2 value) => Is(Value, out value);

        public bool IsNull() => Value is null;

        private static bool Is<T>(object unionValue, out T value)
        {
            if (unionValue is T t1)
            {
                value = t1;
                return true;
            }

            value = default!;
            return false;
        }

        public TOut MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<TOut> nullMap = null!)
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);

            if (Value is T1 t1)
                return t1Mapper.Invoke(t1);

            if (Value is T2 t2)
                return t2Mapper.Invoke(t2);

            // unknown type, assume null
            return nullMap switch
            {
                null => default!,
                _ => nullMap.Invoke()
            };
        }

        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action nullConsumer = null)
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);

            if (Value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (Value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (Value is null && nullConsumer is not null)
                nullConsumer.Invoke();
        }

        public IUnion<T1, T2> WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action nullConsumer = null)
        {
            ConsumeMatch(t1Consumer, t2Consumer, nullConsumer);
            return this;
        }
    }


    public interface IUnionXOf<T1, T2> : IUnion<T1, T2>
    {
        abstract static IUnionXOf<T1, T2> Of(T1 value);

        abstract static IUnionXOf<T1, T2> Of(T2 value);
    }
}
