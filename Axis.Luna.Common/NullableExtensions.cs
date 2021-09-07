using System;

namespace Axis.Luna.Common
{
    public static class NullableExtensions
    {
        public delegate TOut RefMapper<in TIn, out TOut>(TIn arg) 
        where TIn: struct
        where TOut : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="nullable"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Optional<TOut> Map<TIn, TOut>(
            this Nullable<TIn> nullable,
            RefMapper<TIn, TOut> mapper,
            Func<TOut> nullMapper = null)
        where TIn: struct
        where TOut: class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!nullable.HasValue)
                return nullMapper?.Invoke()?.AsOptional() ?? default;

            return new Optional<TOut>(mapper.Invoke(nullable.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="nullable"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Optional<TOut> Map<TIn, TOut>(
            this Nullable<TIn> nullable,
            Func<TIn, Optional<TOut>> mapper,
            Func<TOut> nullMapper = null)
        where TIn : struct
        where TOut : class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!nullable.HasValue)
                return nullMapper?.Invoke()?.AsOptional() ?? default;

            return mapper.Invoke(nullable.Value);
        }
    }
}
