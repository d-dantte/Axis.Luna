using System;

namespace Axis.Luna.Unions.Types
{
    public interface IUnion<T1, T2, T3, T4, T5, T6, TSelf>
    where TSelf : IUnion<T1, T2, T3, T4, T5,T6, TSelf>
    {
        protected object Value { get; }

        bool Is(out T1 value);

        bool Is(out T2 value);

        bool Is(out T3 value);

        bool Is(out T4 value);

        bool Is(out T5 value);

        bool Is(out T6 value);

        bool IsNull();

        public TOut MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<T5, TOut> t5Mapper,
            Func<T6, TOut> t6Mapper,
            Func<TOut>? nullMap = null);

        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action<T6> t6Consumer,
            Action? nullConsumer = null);

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action<T6> t6Consumer,
            Action? nullConsumer = null);

        abstract static TSelf Of(T1 value);

        abstract static TSelf Of(T2 value);

        abstract static TSelf Of(T3 value);

        abstract static TSelf Of(T4 value);

        abstract static TSelf Of(T5 value);

        abstract static TSelf Of(T6 value);
    }
}
