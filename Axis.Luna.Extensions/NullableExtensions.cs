using System;

namespace Axis.Luna.Extensions
{
    public static class NullableExtensions
    {
        /// <summary>
        /// If this nullable contains a value, map it to another <c>Nullable</c> struct, else return null.
        /// </summary>
        /// <typeparam name="TIn">The input struct type</typeparam>
        /// <typeparam name="TOut">The output struct type</typeparam>
        /// <param name="nullable">The input nullable</param>
        /// <param name="mapper">The mapper function</param>
        /// <returns></returns>
        public static Nullable<TOut> Map<TIn, TOut>(
            this Nullable<TIn> nullable,
            Func<TIn, TOut> mapper,
            Func<TOut> nullMapper = null)
        where TIn : struct
        where TOut : struct
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!nullable.HasValue)
                return nullMapper?.Invoke();

            return new TOut?(mapper.Invoke(nullable.Value));
        }

        /// <summary>
        /// Map this to another <c>Nullable</c> struct
        /// </summary>
        /// <typeparam name="TIn">The input struct type</typeparam>
        /// <typeparam name="TOut">The output struct type</typeparam>
        /// <param name="nullable">The input nullable</param>
        /// <param name="mapper">The mapper function</param>
        /// <returns></returns>
        public static Nullable<TOut> Bind<TIn, TOut>(
            this Nullable<TIn> nullable,
            Func<TIn, Nullable<TOut>> mapper,
            Func<TOut> nullMapper = null)
        where TIn : struct
        where TOut : struct
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!nullable.HasValue)
                return nullMapper?.Invoke().AsNullable();

            return mapper.Invoke(nullable.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="nullAction"></param>
        public static void Consume<TIn>(
            this Nullable<TIn> nullable,
            Action<TIn> action,
            Action nullAction = null)
            where TIn: struct
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            else if (nullable.HasValue)
                action.Invoke(nullable.Value);

            else nullAction?.Invoke();
        }

        public static TIn ValueOr<TIn>(this Nullable<TIn> @in, TIn @out)
            where TIn: struct
        {
            if (@in == null)
                return @in.Value;

            return @out;
        }


        public static Nullable<TOut> AsNullable<TOut>(this TOut value)
        where TOut : struct
        {
            return (TOut?)value;
        }
    }
}
