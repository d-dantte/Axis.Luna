using System;

namespace Axis.Luna.Common.Unions
{
    public interface IUnion<T1, T2, T3, TSelf>
    where TSelf : IUnion<T1, T2, T3, TSelf>
    {
        /// <summary>
        /// The payload instance of the Union
        /// </summary>
        protected object Value { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Is(out T1 value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Is(out T2 value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Is(out T3 value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="t1Mapper"></param>
        /// <param name="t2Mapper"></param>
        /// <param name="t3Mapper"></param>
        /// <param name="nullMap"></param>
        /// <returns></returns>
        public TOut MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<TOut> nullMap = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t1Consumer"></param>
        /// <param name="t2Consumer"></param>
        /// <param name="t3Consumer"></param>
        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t1Consumer"></param>
        /// <param name="t2Consumer"></param>
        /// <param name="t3Consumer"></param>
        /// <returns></returns>
        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer);
    }

    public interface IUnionOf<T1, T2, T3, TSelf> :
        IUnion<T1, T2, T3, TSelf>
        where TSelf : IUnionOf<T1, T2, T3, TSelf>
    {
        abstract static TSelf Of(T1 value);

        abstract static TSelf Of(T2 value);

        abstract static TSelf Of(T3 value);
    }

    public abstract class RefUnion<T1, T2, T3, TSelf> :
        IUnion<T1, T2, T3, TSelf>
        where TSelf : RefUnion<T1, T2, T3, TSelf>
    {
        private readonly object _value;

        object IUnion<T1, T2, T3, TSelf>.Value => _value;

        #region Construction
        protected RefUnion(T1 value) => _value = value;

        protected RefUnion(T2 value) => _value = value;

        protected RefUnion(T3 value) => _value = value;
        #endregion

        #region Is

        public bool Is(out T1 value)
        {
            if (_value is T1 t1)
            {
                value = t1;
                return true;
            }

            value = default;
            return false;
        }

        public bool Is(out T2 value)
        {
            if (_value is T2 t2)
            {
                value = t2;
                return true;
            }

            value = default;
            return false;
        }

        public bool Is(out T3 value)
        {
            if (_value is T3 t2)
            {
                value = t2;
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
            Func<TOut> nullMap = null)
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);
            ArgumentNullException.ThrowIfNull(t3Mapper);

            if (_value is T1 t1)
                return t1Mapper.Invoke(t1);

            if (_value is T2 t2)
                return t2Mapper.Invoke(t2);

            if (_value is T3 t3)
                return t3Mapper.Invoke(t3);

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
            Action<T3> t3Consumer)
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);
            ArgumentNullException.ThrowIfNull(t3Consumer);

            if (_value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (_value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (_value is T3 t3)
                t3Consumer.Invoke(t3);
        }

        #endregion

        #region With

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer)
        {
            ConsumeMatch(t1Consumer, t2Consumer, t3Consumer);
            return (TSelf)this;
        }

        #endregion
    }
}
