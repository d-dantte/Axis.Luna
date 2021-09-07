using Axis.Luna.Extensions;
using System;

namespace Axis.Luna.Common
{
    /// <summary>
    /// Essentially, a <c>Maybe</c> of ref-type values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Optional<T> where T: class
    {

        public delegate TResult StructMapper<out TResult>(T arg) where TResult: struct;


        public delegate TResult RefMapper<out TResult>(T arg) where TResult: class;

        /// <summary>
        /// The reference value
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Indicates if there is a value contained
        /// </summary>
        public bool HasValue => Value != null;

        /// <summary>
        /// Indicates that there is no value - opposite of <c>Optional.HasValue</c>
        /// </summary>
        public bool IsEmpty => !HasValue;

        /// <summary>
        /// Construct a new Optional instance
        /// </summary>
        /// <param name="value"></param>
        public Optional(T value)
        {
            Value = value;
        }

        #region Map

        /// <summary>
        /// Map to another ref type
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public Optional<TOut> Map<TOut>(RefMapper<TOut> mapper, Func<TOut> nullMapper = null)
        where TOut : class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!HasValue)
                return nullMapper?.Invoke()?.AsOptional() ?? default;

            return new Optional<TOut>(mapper.Invoke(Value));
        }

        /// <summary>
        /// Map to another ref type
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public Optional<TOut> Bind<TOut>(Func<T, Optional<TOut>> mapper, Func<TOut> nullMapper = null)
        where TOut : class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!HasValue)
                return nullMapper?.Invoke()?.AsOptional() ?? default;

            return mapper.Invoke(Value);
        }

        /// <summary>
        /// Map to another struct type
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public Nullable<TOut> Map<TOut>(StructMapper<TOut> mapper, Func<TOut> nullMapper = null)
        where TOut : struct
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!HasValue)
                return nullMapper?.Invoke().AsNullable() ?? default;

            return new TOut?(mapper.Invoke(Value));
        }

        /// <summary>
        /// Map to another struct type
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public Nullable<TOut> Map<TOut>(Func<T, Nullable<TOut>> mapper, Func<TOut> nullMapper = null)
        where TOut : struct
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!HasValue)
                return nullMapper?.Invoke().AsNullable() ?? default;

            return mapper.Invoke(Value);
        }

        #endregion

        #region Do
        public void Do(Action<T> action, Action nullAction = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            else if (HasValue)
                action.Invoke(Value);

            else nullAction?.Invoke();
        }
        #endregion

        /// <summary>
        /// Returns the given value if this <c>Optional</c> is empty.
        /// </summary>
        /// <param name="alternateValue"></param>
        /// <returns></returns>
        public T ValueOr(T alternateValue) => HasValue ? Value : alternateValue;

        public override bool Equals(object obj)
        {
            return obj is Optional<T> other
                && Value.NullOrEquals(other.Value);
        }

        public override int GetHashCode() => HashCode.Combine(Value);


        public static implicit operator Optional<T>(T value)
        {
            if (value == null)
                return Optional.Empty<T>();

            return new Optional<T>(value);
        }

        public static bool operator ==(Optional<T> op1, Optional<T> op2) => op1.Value.NullOrEquals(op2.Value);

        public static bool operator !=(Optional<T> op1, Optional<T> op2) => !(op1 == op2);
    }

    public static class Optional
    {
        public static Optional<TOut> Empty<TOut>()
        where TOut: class => default;

        public static Optional<TOut> AsOptional<TOut>(this TOut value)
        where TOut: class
        {
            return new Optional<TOut>(value);
        }
    }
}
