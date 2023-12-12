using System;
using System.Collections.Generic;

namespace Axis.Luna.Common.Unions
{
    public interface IUnion<T1, T2, T3, T4, T5, TSelf>
    where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
    {
        protected object Value { get; }

        bool Is(out T1 value);

        bool Is(out T2 value);

        bool Is(out T3 value);

        bool Is(out T4 value);

        bool Is(out T5 value);

        bool IsNull();

        public TOut MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<T5, TOut> t5Mapper,
            Func<TOut> nullMap = null);

        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action nullConsumer = null);

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action nullConsumer = null);
    }

    public interface IUnionOf<T1, T2, T3, T4, T5, TSelf> :
        IUnion<T1, T2, T3, T4, T5, TSelf>
        where TSelf : IUnionOf<T1, T2, T3, T4, T5, TSelf>
    {
        abstract static TSelf Of(T1 value);

        abstract static TSelf Of(T2 value);

        abstract static TSelf Of(T3 value);

        abstract static TSelf Of(T4 value);

        abstract static TSelf Of(T5 value);
    }

    public interface IUnionImplicits<T1, T2, T3, T4, T5, TSelf> :
        IUnion<T1, T2, T3, T4, T5, TSelf>
        where TSelf : IUnionImplicits<T1, T2, T3, T4, T5, TSelf>
    {
        static abstract implicit operator TSelf(T1 value);
        static abstract implicit operator TSelf(T2 value);
        static abstract implicit operator TSelf(T3 value);
        static abstract implicit operator TSelf(T4 value);
        static abstract implicit operator TSelf(T5 value);
    }

    public abstract class RefUnion<T1, T2, T3, T4, T5, TSelf> :
        IUnion<T1, T2, T3, T4, T5, TSelf>
        where TSelf : RefUnion<T1, T2, T3, T4, T5, TSelf>
    {
        private readonly object _value;

        object IUnion<T1, T2, T3, T4, T5, TSelf>.Value => _value;

        #region Construction

        // Remove this if a compile-time check for distinct generic types is available
        static RefUnion()
        {
            var types = new HashSet<Type>
            {
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)
            };

            if (types.Count != 5)
                throw new InvalidOperationException("Invalid generic types: duplicate types found");
        }

        protected RefUnion(object value)
        {
            _value = value switch
            {
                null => null,
                T1 or T2 or T3 or T4 or T5 => value,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Invalid {nameof(value)} type: '{value.GetType()}'")
            };
        }
        #endregion

        #region Is

        public bool Is(out T1 value) => Is(_value, out value);

        public bool Is(out T2 value) => Is(_value, out value);

        public bool Is(out T3 value) => Is(_value, out value);

        public bool Is(out T4 value) => Is(_value, out value);

        public bool Is(out T5 value) => Is(_value, out value);

        public bool IsNull() => _value is null;

        private static bool Is<T>(object unionValue, out T value)
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

        public TOut MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<T5, TOut> t5Mapper,
            Func<TOut> nullMap = null)
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);
            ArgumentNullException.ThrowIfNull(t3Mapper);
            ArgumentNullException.ThrowIfNull(t4Mapper);
            ArgumentNullException.ThrowIfNull(t5Mapper);

            if (_value is T1 t1)
                return t1Mapper.Invoke(t1);

            if (_value is T2 t2)
                return t2Mapper.Invoke(t2);

            if (_value is T3 t3)
                return t3Mapper.Invoke(t3);

            if (_value is T4 t4)
                return t4Mapper.Invoke(t4);

            if (_value is T5 t5)
                return t5Mapper.Invoke(t5);

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
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action nullConsumer = null)
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);
            ArgumentNullException.ThrowIfNull(t3Consumer);
            ArgumentNullException.ThrowIfNull(t4Consumer);
            ArgumentNullException.ThrowIfNull(t5Consumer);

            if (_value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (_value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (_value is T3 t3)
                t3Consumer.Invoke(t3);

            else if (_value is T4 t4)
                t4Consumer.Invoke(t4);

            else if (_value is T5 t5)
                t5Consumer.Invoke(t5);

            else if (_value is null && nullConsumer is not null)
                nullConsumer.Invoke();
        }

        #endregion

        #region With

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action nullConsumer = null)
        {
            ConsumeMatch(t1Consumer, t2Consumer, t3Consumer, t4Consumer, t5Consumer, nullConsumer);
            return (TSelf)this;
        }

        #endregion
    }
}
