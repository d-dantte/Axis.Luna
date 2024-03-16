using System;

namespace Axis.Luna.Optional
{
    public static class NullableExtensions
    {
        public delegate TOut RefMapper<in TIn, out TOut>(TIn arg) 
        where TIn: struct
        where TOut : class;

        /// <summary>
        /// Maps a nullable struct to another nullable struct
        /// </summary>
        /// <typeparam name="TIn">The input type</typeparam>
        /// <typeparam name="TOut">The output type</typeparam>
        /// <param name="nullable"></param>
        /// <param name="mapper">mapper function executed when a value exists in the nullable</param>
        /// <param name="nullMapper">mapper function executed when no value exists in the nullable</param>
        /// <returns></returns>
        public static Optional<TOut> Map<TIn, TOut>(
            this Nullable<TIn> nullable,
            RefMapper<TIn, TOut> mapper,
            Func<TOut>? nullMapper = null)
        where TIn: struct
        where TOut: class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!nullable.HasValue)
                return nullMapper?.Invoke() ?? Optional.Empty<TOut>();

            return new Optional<TOut>(mapper.Invoke(nullable.Value));
        }

        /// <summary>
        /// Maps a nullable struct to an optional
        /// </summary>
        /// <typeparam name="TIn">The input type</typeparam>
        /// <typeparam name="TOut">The output type</typeparam>
        /// <param name="nullable">The nullable</param>
        /// <param name="mapper">mapper function executed when a value exists in the nullable</param>
        /// <param name="nullMapper">mapper function executed when no value exists in the nullable</param>
        /// <returns></returns>
        public static Optional<TOut> Map<TIn, TOut>(
            this Nullable<TIn> nullable,
            Func<TIn, Optional<TOut>> mapper,
            Func<TOut>? nullMapper = null)
        where TIn : struct
        where TOut : class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!nullable.HasValue)
                return nullMapper?.Invoke() ?? Optional.Empty<TOut>();

            return mapper.Invoke(nullable.Value);
        }
    }
}
