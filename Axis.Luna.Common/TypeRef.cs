using System;
using System.Collections.Immutable;

namespace Axis.Luna.Common
{
    public readonly struct TypeRef<TType>
    {
        public Type Type => typeof(TType);
    }

    public readonly struct TypeRef<TType1, TType2>
    {
        public ImmutableArray<Type> Types => ArrayUtil
            .Of(typeof(TType1), typeof(TType2))
            .ToImmutableArray();
    }

    public readonly struct TypeRef<TType1, TType2, TType3>
    {
        public ImmutableArray<Type> Types => ArrayUtil
            .Of(typeof(TType1), typeof(TType2), typeof(TType3))
            .ToImmutableArray();
    }

    public readonly struct TypeRef<TType1, TType2, TType3, TType4>
    {
        public ImmutableArray<Type> Types => ArrayUtil
            .Of(typeof(TType1), typeof(TType2), typeof(TType3), typeof(TType4))
            .ToImmutableArray();
    }

    public readonly struct TypeRef<TType1, TType2, TType3, TType4, TType5>
    {
        public ImmutableArray<Type> Types => ArrayUtil
            .Of(typeof(TType1), typeof(TType2), typeof(TType3), typeof(TType4), typeof(TType5))
            .ToImmutableArray();
    }

    public readonly struct TypeRef<TType1, TType2, TType3, TType4, TType5, TType6>
    {
        public ImmutableArray<Type> Types => ArrayUtil
            .Of(typeof(TType1), typeof(TType2), typeof(TType3), typeof(TType4), typeof(TType5), typeof(TType6))
            .ToImmutableArray();
    }

    public readonly struct TypeRef<TType1, TType2, TType3, TType4, TType5, TType6, TType7>
    {
        public ImmutableArray<Type> Types => ArrayUtil
            .Of(typeof(TType1), typeof(TType2), typeof(TType3), typeof(TType4), typeof(TType5), typeof(TType6), typeof(TType7))
            .ToImmutableArray();
    }
}
