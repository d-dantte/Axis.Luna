﻿using Axis.Luna.Result;

namespace Axis.Luna.Optional
{
    /// <summary>
    /// Essentially, a <c>Maybe</c> of ref-type values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Optional<T>
    where T: class
    {

        public delegate TResult StructMapper<out TResult>(T arg) where TResult: struct;


        public delegate TResult RefMapper<out TResult>(T arg) where TResult: class;


        private readonly T? _value;


        /// <summary>
        /// Indicates if there is a value contained
        /// </summary>
        public bool HasValue => _value != null;

        /// <summary>
        /// Indicates that there is no value - opposite of <c>Optional.HasValue</c>
        /// </summary>
        public bool IsEmpty => !HasValue;

        /// <summary>
        /// Construct a new Optional instance
        /// </summary>
        /// <param name="value"></param>
        public Optional(T? value)
        {
            _value = value;
        }

        #region Map/Bind

        /// <summary>
        /// Map to another ref type
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public Optional<TOut> Map<TOut>(RefMapper<TOut> mapper, Func<TOut>? nullMapper = null)
        where TOut : class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!HasValue)
                return nullMapper?.Invoke()!;

            return new Optional<TOut>(mapper.Invoke(_value!));
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
                return nullMapper?.Invoke()!;

            return mapper.Invoke(_value!);
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
                return nullMapper?.Invoke();

            return new TOut?(mapper.Invoke(_value!));
        }

        /// <summary>
        /// Map to another struct type
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public Nullable<TOut> Map<TOut>(Func<T, Nullable<TOut>> mapper, Func<TOut>? nullMapper = null)
        where TOut : struct
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (!HasValue)
                return nullMapper?.Invoke();

            return mapper.Invoke(_value!);
        }

        #endregion

        #region Consume
        public void Consume(Action<T> action, Action nullAction = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            else if (HasValue)
                action.Invoke(_value);

            else nullAction?.Invoke();
        }
        #endregion

        #region Combine
        public Optional<TOut> Combine<T2, TOut>(
            Optional<T2> optional,
            Func<(T, T2), TOut> mapper,
            Func<TOut>? nullMapper = null)
            where TOut : class
            where T2 : class
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (!HasValue || !optional.HasValue)
                return nullMapper?.Invoke()!;

            return mapper.Invoke((_value!, optional._value!));
        }
        #endregion

        #region Value
        /// <summary>
        /// Returns the value if it exists, else throws an exception
        /// </summary>
        /// <returns>The value if present</returns>
        public T Value() => _value ?? throw new InvalidOperationException("Optional is empty");

        /// <summary>
        /// Returns the value if it exists, or default(T);
        /// </summary>
        /// <returns></returns>
        public T ValueOrDefault() => ValueOr(default(T)!);

        /// <summary>
        /// Returns the given value if this <c>Optional</c> is empty.
        /// </summary>
        /// <param name="alternateValue"></param>
        /// <returns></returns>
        public T ValueOr(T alternateValue) => HasValue ? _value! : alternateValue;

        /// <summary>
        /// Returns the given value if this <c>Optional</c> is empty.
        /// </summary>
        /// <param name="alternateValue"></param>
        /// <returns></returns>
        public T ValueOr(Func<T> alternateValueProvider)
        {
            ArgumentNullException.ThrowIfNull(alternateValueProvider);

            return HasValue ? _value! : alternateValueProvider.Invoke();
        }
        #endregion

        #region Equality
        public override bool Equals(object? obj)
        {
            var comparer = EqualityComparer<T>.Default;
            return obj is Optional<T> other
                && comparer.Equals(_value, other._value);
        }

        public override int GetHashCode() => HashCode.Combine(_value);
        #endregion

        #region DefaultProvider
        public bool IsDefault => IsEmpty;

        public static Optional<T> Default => default;
        #endregion

        #region Operators
        public static implicit operator Optional<T>(T? value)
        {
            if (value is null)
                return Optional.Empty<T>();

            return new Optional<T>(value);
        }

        public static bool operator ==(Optional<T> op1, Optional<T> op2) => op1.Equals(op2);

        public static bool operator !=(Optional<T> op1, Optional<T> op2) => !op1.Equals(op2);
        #endregion
    }

    /// <summary>
    /// Optional util class
    /// </summary>
    public static class Optional
    {
        public static Optional<TOut> Empty<TOut>()
        where TOut: class => default;

        public static Optional<TOut> AsOptional<TOut>(this TOut? value)
        where TOut: class
        {
            return new Optional<TOut>(value);
        }

        public static Optional<TOut> MapToOptional<TIn, TOut>(this TIn value, Func<TIn, TOut> mapper)
        where TOut : class
        where TIn : class
        {
            return value
                .AsOptional()
                .Map(mapper.Invoke);
        }

        public static IResult<TValue> AsResult<TValue>(this
            Optional<TValue> optional,
            Exception? exception = null)
            where TValue: class
        {
            return optional.IsEmpty
                ? Result.Result.Of<TValue>(exception ?? new Exception("Value is null"))
                : Result.Result.Of(optional.Value());
        }
    }
}
