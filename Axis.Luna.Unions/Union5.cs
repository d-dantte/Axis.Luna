﻿namespace Axis.Luna.Unions
{
    public interface IUnion<T1, T2, T3, T4, T5, TSelf>
    where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
    {
        internal protected object? Value { get; }
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
}
