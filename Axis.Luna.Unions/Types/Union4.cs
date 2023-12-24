using System;

namespace Axis.Luna.Unions.Types
{
    public interface IUnion<T1, T2, T3, T4, TSelf>
    where TSelf : IUnion<T1, T2, T3, T4, TSelf>
    {
        protected object Value { get; }

        bool Is(out T1 value);

        bool Is(out T2 value);

        bool Is(out T3 value);

        bool Is(out T4 value);

        bool IsNull();

        public TOut MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<TOut>? nullMap = null);

        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action? nullConsumer = null);

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action? nullConsumer = null);

        abstract static TSelf Of(T1 value);

        abstract static TSelf Of(T2 value);

        abstract static TSelf Of(T3 value);

        abstract static TSelf Of(T4 value);
    }
}
