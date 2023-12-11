using System;

namespace Axis.Luna.Common.Unions
{
    public interface IUnion<T1, T2, T3, T4, TSelf>
    where TSelf : IUnion<T1, T2, T3, T4, TSelf>
    {
        protected object Value { get; }

        bool Is(out T1 value);

        bool Is(out T2 value);

        bool Is(out T3 value);

        bool Is(out T4 value);

        public TOut MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<TOut> nullMap = null);

        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer);

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer);
    }

    public interface IUnionOf<T1, T2, T3, T4, TSelf> :
        IUnion<T1, T2, T3, T4, TSelf>
        where TSelf : IUnionOf<T1, T2, T3, T4, TSelf>
    {
        abstract static TSelf Of(T1 value);

        abstract static TSelf Of(T2 value);

        abstract static TSelf Of(T3 value);

        abstract static TSelf Of(T4 value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TSelf"></typeparam>
    public abstract class RefUnion<T1, T2, T3, T4, TSelf> :
        IUnion<T1, T2, T3, T4, TSelf>
        where TSelf : RefUnion<T1, T2, T3, T4, TSelf>
    {
        private readonly object _value;

        object IUnion<T1, T2, T3, T4, TSelf>.Value => _value;

        #region Construction
        protected RefUnion(T1 value) => _value = value;

        protected RefUnion(T2 value) => _value = value;

        protected RefUnion(T3 value) => _value = value;

        protected RefUnion(T4 value) => _value = value;
        #endregion

        #region Is

        public bool Is(out T1 value)
        {
            if (_value is T1 _v)
            {
                value = _v;
                return true;
            }

            value = default;
            return false;
        }

        public bool Is(out T2 value)
        {
            if (_value is T2 _v)
            {
                value = _v;
                return true;
            }

            value = default;
            return false;
        }

        public bool Is(out T3 value)
        {
            if (_value is T3 _v)
            {
                value = _v;
                return true;
            }

            value = default;
            return false;
        }

        public bool Is(out T4 value)
        {
            if (_value is T4 _v)
            {
                value = _v;
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
            Func<TOut> nullMap = null)
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);
            ArgumentNullException.ThrowIfNull(t3Mapper);
            ArgumentNullException.ThrowIfNull(t4Mapper);

            if (_value is T1 t1)
                return t1Mapper.Invoke(t1);

            if (_value is T2 t2)
                return t2Mapper.Invoke(t2);

            if (_value is T3 t3)
                return t3Mapper.Invoke(t3);

            if (_value is T4 t4)
                return t4Mapper.Invoke(t4);

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
            Action<T4> t4Consumer)
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);
            ArgumentNullException.ThrowIfNull(t3Consumer);
            ArgumentNullException.ThrowIfNull(t4Consumer);

            if (_value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (_value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (_value is T3 t3)
                t3Consumer.Invoke(t3);

            else if (_value is T4 t4)
                t4Consumer.Invoke(t4);
        }

        #endregion

        #region With

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer)
        {
            ConsumeMatch(t1Consumer, t2Consumer, t3Consumer, t4Consumer);
            return (TSelf)this;
        }

        #endregion
    }
}
