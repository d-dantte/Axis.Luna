using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Luna.Extensions
{
    public static class NullableExtensions
    {
        /// <summary>
        /// Map this to another <c>Nullable</c> struct
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
                return nullMapper?.Invoke().AsNullable() ?? default;

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
        public static Nullable<TOut> Map<TIn, TOut>(
            this Nullable<TIn> nullable,
            Func<TIn, Nullable<TOut>> mapper,
            Func<TOut> nullMapper = null)
        where TIn : struct
        where TOut : struct
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!nullable.HasValue)
                return nullMapper?.Invoke().AsNullable() ?? default;

            return mapper.Invoke(nullable.Value);
        }


        public static Nullable<TOut> AsNullable<TOut>(this TOut value)
        where TOut : struct
        {
            return new TOut?(value);
        }
    }
}
