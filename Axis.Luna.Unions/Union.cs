namespace Axis.Luna.Unions
{
    public static class Union
    {
        #region Union-2

        #region Is

        public static bool Is<T1, T2, TSelf>(
            this IUnion<T1, T2, TSelf> union,
            out T1? value)
            where TSelf : IUnion<T1, T2, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, TSelf>(
            this IUnion<T1, T2, TSelf> union,
            out T2? value)
            where TSelf : IUnion<T1, T2, TSelf>
            => union.Value.Is(out value);

        public static bool IsNull<T1, T2, TSelf>(
            this IUnion<T1, T2, TSelf> union)
            where TSelf : IUnion<T1, T2, TSelf>
            => union.Value is null;

        #endregion

        #region Match

        public static TOut? Match<T1, T2, TSelf, TOut>(
            this IUnion<T1, T2, TSelf> union,
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<TOut>? nullMapper = null)
            where TSelf : IUnion<T1, T2, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);

            return (union.Value, nullMapper) switch
            {
                (T1 t1, _) => t1Mapper.Invoke(t1),
                (T2 t2, _) => t2Mapper.Invoke(t2),
                (null, Func<TOut> _nullMapper) => _nullMapper.Invoke(),
                (null, null) => default,
                _ => throw new InvalidOperationException(
                    $"Invalid union type: '{union.Value.GetType()}'")
            };
        }

        #endregion

        #region Consume

        public static void Consume<T1, T2, TSelf>(
            this IUnion<T1, T2, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);

            if (union.Value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (union.Value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (union.Value is null)
                nullConsumer?.Invoke();
        }

        #endregion

        #region With

        public static TSelf With<T1, T2, TSelf>(
            this IUnion<T1, T2, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, TSelf>
        {
            union.Consume(t1Consumer, t2Consumer, nullConsumer);

            return (TSelf)union;
        }

        #endregion

        #endregion

        #region Union-3

        #region Is

        public static bool Is<T1, T2, T3, TSelf>(
            this IUnion<T1, T2, T3, TSelf> union,
            out T1? value)
            where TSelf : IUnion<T1, T2, T3, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, TSelf>(
            this IUnion<T1, T2, T3, TSelf> union,
            out T2? value)
            where TSelf : IUnion<T1, T2, T3, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, TSelf>(
            this IUnion<T1, T2, T3, TSelf> union,
            out T3? value)
            where TSelf : IUnion<T1, T2, T3, TSelf>
            => union.Value.Is(out value);

        public static bool IsNull<T1, T2, T3, TSelf>(
            this IUnion<T1, T2, T3, TSelf> union)
            where TSelf : IUnion<T1, T2, T3, TSelf>
            => union.Value is null;

        #endregion

        #region Match

        public static TOut? Match<T1, T2, T3, TSelf, TOut>(
            this IUnion<T1, T2, T3, TSelf> union,
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<TOut>? nullMapper = null)
            where TSelf : IUnion<T1, T2, T3, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);
            ArgumentNullException.ThrowIfNull(t3Mapper);

            return (union.Value, nullMapper) switch
            {
                (T1 t1, _) => t1Mapper.Invoke(t1),
                (T2 t2, _) => t2Mapper.Invoke(t2),
                (T3 t3, _) => t3Mapper.Invoke(t3),
                (null, Func<TOut> _nullMapper) => _nullMapper.Invoke(),
                (null, null) => default,
                _ => throw new InvalidOperationException(
                    $"Invalid union type: '{union.Value.GetType()}'")
            };
        }

        #endregion

        #region Consume

        public static void Consume<T1, T2, T3, TSelf>(
            this IUnion<T1, T2, T3, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);
            ArgumentNullException.ThrowIfNull(t3Consumer);

            if (union.Value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (union.Value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (union.Value is T3 t3)
                t3Consumer.Invoke(t3);

            else if (union.Value is null)
                nullConsumer?.Invoke();
        }

        #endregion

        #region With

        public static TSelf With<T1, T2, T3, TSelf>(
            this IUnion<T1, T2, T3, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, TSelf>
        {
            union.Consume(t1Consumer, t2Consumer, t3Consumer, nullConsumer);

            return (TSelf)union;
        }

        #endregion

        #endregion

        #region Union-4

        #region Is

        public static bool Is<T1, T2, T3, T4, TSelf>(
            this IUnion<T1, T2, T3, T4, TSelf> union,
            out T1? value)
            where TSelf : IUnion<T1, T2, T3, T4, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, TSelf>(
            this IUnion<T1, T2, T3, T4, TSelf> union,
            out T2? value)
            where TSelf : IUnion<T1, T2, T3, T4, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, TSelf>(
            this IUnion<T1, T2, T3, T4, TSelf> union,
            out T3? value)
            where TSelf : IUnion<T1, T2, T3, T4, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, TSelf>(
            this IUnion<T1, T2, T3, T4, TSelf> union,
            out T4? value)
            where TSelf : IUnion<T1, T2, T3, T4, TSelf>
            => union.Value.Is(out value);

        public static bool IsNull<T1, T2, T3, T4, TSelf>(
            this IUnion<T1, T2, T3, T4, TSelf> union)
            where TSelf : IUnion<T1, T2, T3, T4, TSelf>
            => union.Value is null;

        #endregion

        #region Match

        public static TOut? Match<T1, T2, T3, T4, TSelf, TOut>(
            this IUnion<T1, T2, T3, T4, TSelf> union,
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<TOut>? nullMapper = null)
            where TSelf : IUnion<T1, T2, T3, T4, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);
            ArgumentNullException.ThrowIfNull(t3Mapper);
            ArgumentNullException.ThrowIfNull(t4Mapper);

            return (union.Value, nullMapper) switch
            {
                (T1 t1, _) => t1Mapper.Invoke(t1),
                (T2 t2, _) => t2Mapper.Invoke(t2),
                (T3 t3, _) => t3Mapper.Invoke(t3),
                (T4 t4, _) => t4Mapper.Invoke(t4),
                (null, Func<TOut> _nullMapper) => _nullMapper.Invoke(),
                (null, null) => default,
                _ => throw new InvalidOperationException(
                    $"Invalid union type: '{union.Value.GetType()}'")
            };
        }

        #endregion

        #region Consume

        public static void Consume<T1, T2, T3, T4, TSelf>(
            this IUnion<T1, T2, T3, T4, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, T4, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);
            ArgumentNullException.ThrowIfNull(t3Consumer);
            ArgumentNullException.ThrowIfNull(t4Consumer);

            if (union.Value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (union.Value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (union.Value is T3 t3)
                t3Consumer.Invoke(t3);

            else if (union.Value is T4 t4)
                t4Consumer.Invoke(t4);

            else if (union.Value is null)
                nullConsumer?.Invoke();
        }

        #endregion

        #region With

        public static TSelf With<T1, T2, T3, T4, TSelf>(
            this IUnion<T1, T2, T3, T4, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, T4, TSelf>
        {
            union.Consume(t1Consumer, t2Consumer, t3Consumer, t4Consumer, nullConsumer);

            return (TSelf)union;
        }

        #endregion

        #endregion

        #region Union-5

        #region Is

        public static bool Is<T1, T2, T3, T4, T5, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union,
            out T1? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union,
            out T2? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union,
            out T3? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union,
            out T4? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union,
            out T5? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
            => union.Value.Is(out value);

        public static bool IsNull<T1, T2, T3, T4, T5, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
            => union.Value is null;

        #endregion

        #region Match

        public static TOut? Match<T1, T2, T3, T4, T5, TSelf, TOut>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union,
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<T5, TOut> t5Mapper,
            Func<TOut>? nullMapper = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);
            ArgumentNullException.ThrowIfNull(t3Mapper);
            ArgumentNullException.ThrowIfNull(t4Mapper);
            ArgumentNullException.ThrowIfNull(t5Mapper);

            return (union.Value, nullMapper) switch
            {
                (T1 t1, _) => t1Mapper.Invoke(t1),
                (T2 t2, _) => t2Mapper.Invoke(t2),
                (T3 t3, _) => t3Mapper.Invoke(t3),
                (T4 t4, _) => t4Mapper.Invoke(t4),
                (T5 t5, _) => t5Mapper.Invoke(t5),
                (null, Func<TOut> _nullMapper) => _nullMapper.Invoke(),
                (null, null) => default,
                _ => throw new InvalidOperationException(
                    $"Invalid union type: '{union.Value.GetType()}'")
            };
        }

        #endregion

        #region Consume

        public static void Consume<T1, T2, T3, T4, T5, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);
            ArgumentNullException.ThrowIfNull(t3Consumer);
            ArgumentNullException.ThrowIfNull(t4Consumer);
            ArgumentNullException.ThrowIfNull(t5Consumer);

            if (union.Value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (union.Value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (union.Value is T3 t3)
                t3Consumer.Invoke(t3);

            else if (union.Value is T4 t4)
                t4Consumer.Invoke(t4);

            else if (union.Value is T5 t5)
                t5Consumer.Invoke(t5);

            else if (union.Value is null)
                nullConsumer?.Invoke();
        }

        #endregion

        #region With

        public static TSelf With<T1, T2, T3, T4, T5, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, TSelf>
        {
            union.Consume(
                t1Consumer, t2Consumer, t3Consumer,
                t4Consumer, t5Consumer, nullConsumer);

            return (TSelf)union;
        }

        #endregion

        #endregion

        #region Union-6

        #region Is

        public static bool Is<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            out T1? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            out T2? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            out T3? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            out T4? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            out T5? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            out T6? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
            => union.Value.Is(out value);

        public static bool IsNull<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
            => union.Value is null;

        #endregion

        #region Match

        public static TOut? Match<T1, T2, T3, T4, T5, T6, TSelf, TOut>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<T5, TOut> t5Mapper,
            Func<T6, TOut> t6Mapper,
            Func<TOut>? nullMapper = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);
            ArgumentNullException.ThrowIfNull(t3Mapper);
            ArgumentNullException.ThrowIfNull(t4Mapper);
            ArgumentNullException.ThrowIfNull(t5Mapper);
            ArgumentNullException.ThrowIfNull(t6Mapper);

            return (union.Value, nullMapper) switch
            {
                (T1 t1, _) => t1Mapper.Invoke(t1),
                (T2 t2, _) => t2Mapper.Invoke(t2),
                (T3 t3, _) => t3Mapper.Invoke(t3),
                (T4 t4, _) => t4Mapper.Invoke(t4),
                (T5 t5, _) => t5Mapper.Invoke(t5),
                (T6 t6, _) => t6Mapper.Invoke(t6),
                (null, Func<TOut> _nullMapper) => _nullMapper.Invoke(),
                (null, null) => default,
                _ => throw new InvalidOperationException(
                    $"Invalid union type: '{union.Value.GetType()}'")
            };
        }

        #endregion

        #region Consume

        public static void Consume<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action<T6> t6Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);
            ArgumentNullException.ThrowIfNull(t3Consumer);
            ArgumentNullException.ThrowIfNull(t4Consumer);
            ArgumentNullException.ThrowIfNull(t5Consumer);
            ArgumentNullException.ThrowIfNull(t6Consumer);

            if (union.Value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (union.Value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (union.Value is T3 t3)
                t3Consumer.Invoke(t3);

            else if (union.Value is T4 t4)
                t4Consumer.Invoke(t4);

            else if (union.Value is T5 t5)
                t5Consumer.Invoke(t5);

            else if (union.Value is T6 t6)
                t6Consumer.Invoke(t6);

            else if (union.Value is null)
                nullConsumer?.Invoke();
        }

        #endregion

        #region With

        public static TSelf With<T1, T2, T3, T4, T5, T6, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action<T6> t6Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, TSelf>
        {
            union.Consume(
                t1Consumer, t2Consumer, t3Consumer,
                t4Consumer, t5Consumer, t6Consumer,
                nullConsumer);

            return (TSelf)union;
        }

        #endregion

        #endregion

        #region Union-7

        #region Is

        public static bool Is<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            out T1? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            out T2? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            out T3? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            out T4? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            out T5? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            out T6? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
            => union.Value.Is(out value);

        public static bool Is<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            out T7? value)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
            => union.Value.Is(out value);

        public static bool IsNull<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
            => union.Value is null;

        #endregion

        #region Match

        public static TOut? Match<T1, T2, T3, T4, T5, T6, T7, TSelf, TOut>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<T3, TOut> t3Mapper,
            Func<T4, TOut> t4Mapper,
            Func<T5, TOut> t5Mapper,
            Func<T6, TOut> t6Mapper,
            Func<T7, TOut> t7Mapper,
            Func<TOut>? nullMapper = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Mapper);
            ArgumentNullException.ThrowIfNull(t2Mapper);
            ArgumentNullException.ThrowIfNull(t3Mapper);
            ArgumentNullException.ThrowIfNull(t4Mapper);
            ArgumentNullException.ThrowIfNull(t5Mapper);
            ArgumentNullException.ThrowIfNull(t6Mapper);
            ArgumentNullException.ThrowIfNull(t7Mapper);

            return (union.Value, nullMapper) switch
            {
                (T1 t1, _) => t1Mapper.Invoke(t1),
                (T2 t2, _) => t2Mapper.Invoke(t2),
                (T3 t3, _) => t3Mapper.Invoke(t3),
                (T4 t4, _) => t4Mapper.Invoke(t4),
                (T5 t5, _) => t5Mapper.Invoke(t5),
                (T6 t6, _) => t6Mapper.Invoke(t6),
                (T7 t7, _) => t7Mapper.Invoke(t7),
                (null, Func<TOut> _nullMapper) => _nullMapper.Invoke(),
                (null, null) => default,
                _ => throw new InvalidOperationException(
                    $"Invalid union type: '{union.Value.GetType()}'")
            };
        }

        #endregion

        #region Consume

        public static void Consume<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action<T6> t6Consumer,
            Action<T7> t7Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
        {
            ArgumentNullException.ThrowIfNull(t1Consumer);
            ArgumentNullException.ThrowIfNull(t2Consumer);
            ArgumentNullException.ThrowIfNull(t3Consumer);
            ArgumentNullException.ThrowIfNull(t4Consumer);
            ArgumentNullException.ThrowIfNull(t5Consumer);
            ArgumentNullException.ThrowIfNull(t6Consumer);
            ArgumentNullException.ThrowIfNull(t7Consumer);

            if (union.Value is T1 t1)
                t1Consumer.Invoke(t1);

            else if (union.Value is T2 t2)
                t2Consumer.Invoke(t2);

            else if (union.Value is T3 t3)
                t3Consumer.Invoke(t3);

            else if (union.Value is T4 t4)
                t4Consumer.Invoke(t4);

            else if (union.Value is T5 t5)
                t5Consumer.Invoke(t5);

            else if (union.Value is T6 t6)
                t6Consumer.Invoke(t6);

            else if (union.Value is T7 t7)
                t7Consumer.Invoke(t7);

            else if (union.Value is null)
                nullConsumer?.Invoke();
        }

        #endregion

        #region With

        public static TSelf With<T1, T2, T3, T4, T5, T6, T7, TSelf>(
            this IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf> union,
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action<T3> t3Consumer,
            Action<T4> t4Consumer,
            Action<T5> t5Consumer,
            Action<T6> t6Consumer,
            Action<T7> t7Consumer,
            Action? nullConsumer = null)
            where TSelf : IUnion<T1, T2, T3, T4, T5, T6, T7, TSelf>
        {
            union.Consume(
                t1Consumer, t2Consumer, t3Consumer,
                t4Consumer, t5Consumer, t6Consumer,
                t7Consumer, nullConsumer);

            return (TSelf)union;
        }

        #endregion

        #endregion

        #region Common

        private static bool Is<T>(this object? unionValue, out T? value)
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
    }
}
