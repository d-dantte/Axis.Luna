using Axis.Luna.Extensions;
using System;

namespace Axis.Luna.Common.Results
{
    /// <summary>
    /// A monadic tagged-union type encapsulating the binary states of any function's execution: a value, or an error.
    /// </summary>
    /// <typeparam name="TData">the encapsulated data type</typeparam>
    public interface IResult<TData>
    {
        #region Members
        /// <summary>
        /// Maps the encapsulated result (or error) into a new result instance
        /// </summary>
        /// <typeparam name="TOut">the type of the output result</typeparam>
        /// <param name="mapper">the mapping function</param>
        IResult<TOut> Map<TOut>(Func<TData, TOut> mapper);

        /// <summary>
        /// executes <see cref="Luna.Extensions.Common.As{T}(object)"/> on this results data.
        /// </summary>
        /// <typeparam name="TOut">the type of the output result</typeparam>
        IResult<TOut> MapAs<TOut>();

        /// <summary>
        /// Maps the encapsulated value to a new value, or maps the encapsulated error into the <see cref="TData"/> type, then uses the <paramref name="valueMapper"/>
        /// to map the resulting value into the final <typeparamref name="TOut"/> type.
        /// </summary>
        /// <typeparam name="TOut">the output type</typeparam>
        /// <param name="valueMapper">the value mapper</param>
        /// <param name="errorMapper">the error mapper</param>
        /// <returns></returns>
        IResult<TOut> Map<TOut>(
            Func<TData, TOut> valueMapper,
            Func<Exception, TData> errorMapper);

        /// <summary>
        /// Binds the encapsulated result (or error) to a new result instance
        /// </summary>
        /// <typeparam name="TOut">the type of the output result</typeparam>
        /// <param name="binder">the binding function</param>
        IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder);

        /// <summary>
        /// Combines the encapsulated data of this result, and another result, into a Result of the combined type.
        /// <para>
        /// If either or both results are errored, the output result is a combination of both errors
        /// </para>
        /// </summary>
        /// <typeparam name="TOther"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="otherResult"></param>
        /// <param name="combiner"></param>
        /// <returns></returns>
        IResult<TOut> Combine<TOther, TOut>(
            IResult<TOther> otherResult,
            Func<TData, TOther, TOut> combiner);
        #endregion

        #region Union Types

        /// <summary>
        /// Represents a faulted result, and contains the exception
        /// </summary>
        public readonly struct ErrorResult :
            IResult<TData>,
            IDefaultValueProvider<ErrorResult>
        {
            private readonly ResultException _cause;

            /// <summary>
            /// The error message
            /// </summary>
            public string Message => TrueCause.Message;

            /// <summary>
            /// The optional error data 
            /// </summary>
            public object ErrorData => TrueCause.ErrorResultData();

            private Exception TrueCause => _cause.InnerException;

            internal ErrorResult(Exception exception)
            {
                _cause = new ResultException(
                    exception is null ? throw new ArgumentNullException(nameof(exception)) :
                    exception is ResultException re ? re.InnerException :
                    exception);
            }

            public IResult<TOut> Map<TOut>(Func<TData, TOut> mapper)
            {
                if (mapper is null)
                    throw new ArgumentNullException(nameof(mapper));

                return new IResult<TOut>.ErrorResult(_cause);
            }

            public IResult<TOut> MapAs<TOut>() => new IResult<TOut>.ErrorResult(_cause);

            public IResult<TOut> Map<TOut>(
                Func<TData, TOut> valueMapper,
                Func<Exception, TData> errorMapper)
            {
                if (valueMapper is null)
                    throw new ArgumentNullException(nameof(valueMapper));

                if (errorMapper is null)
                    throw new ArgumentNullException(nameof(errorMapper));

                Exception cause = _cause;
                return Result.Of(() =>
                {
                    var value = errorMapper.Invoke(cause);
                    return valueMapper.Invoke(value);
                });
            }

            public IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder)
            {
                if (binder is null)
                    throw new ArgumentNullException(nameof(binder));

                return new IResult<TOut>.ErrorResult(_cause);
            }

            public IResult<TOut> Combine<TOther, TOut>(
                IResult<TOther> otherResult,
                Func<TData, TOther, TOut> combiner)
            {
                if (otherResult is null)
                    throw new ArgumentNullException(nameof(otherResult));

                if (combiner is null)
                    throw new ArgumentNullException(nameof(combiner));

                if (otherResult is IResult<TOther>.ErrorResult otherError)
                    return new IResult<TOut>.ErrorResult(
                        new AggregateException(
                            TrueCause,
                            otherError._cause.InnerException));

                else return new IResult<TOut>.ErrorResult(TrueCause);
            }

            /// <summary>
            /// Gets the exception
            /// </summary>
            public ResultException Cause() => _cause;

            /// <summary>
            /// Throws the error
            /// </summary>
            public TData ThrowError() => _cause.Throw<TData>();


            public override string ToString() => $"Error Result: {_cause?.InnerException.ToString()}";

            public override int GetHashCode() => HashCode.Combine(_cause, ErrorData);

            public override bool Equals(object obj)
            {
                return obj is ErrorResult other
                    && other._cause.NullOrEquals(_cause)
                    && other.ErrorData.NullOrEquals(ErrorData);
            }

            public bool IsDefault => _cause is null;

            public static ErrorResult Default => default;

            public static bool operator ==(ErrorResult first, ErrorResult second) => first.Equals(second);

            public static bool operator !=(ErrorResult first, ErrorResult second) => !first.Equals(second);

        }


        /// <summary>
        /// Represents data.
        /// </summary>
        public readonly struct DataResult :
            IResult<TData>,
            IDefaultValueProvider<DataResult>
        {
            /// <summary>
            /// The result data
            /// </summary>
            public TData Data { get; }

            internal DataResult(TData data)
            {
                Data = data;
            }

            public IResult<TOut> Map<TOut>(Func<TData, TOut> mapper)
            {
                if (mapper is null)
                    throw new ArgumentNullException(nameof(mapper));

                TData data = Data;
                return Result.Of(() => mapper.Invoke(data));
            }

            public IResult<TOut> Map<TOut>(
                Func<TData, TOut> valueMapper,
                Func<Exception, TData> errorMapper)
            {
                if (valueMapper is null)
                    throw new ArgumentNullException(nameof(valueMapper));

                if (errorMapper is null)
                    throw new ArgumentNullException(nameof(errorMapper));

                TData data = Data;
                return Result.Of(() => valueMapper.Invoke(data));
            }

            public IResult<TOut> MapAs<TOut>() => Map(data => data.As<TOut>());

            public IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder)
            {
                if (binder is null)
                    throw new ArgumentNullException(nameof(binder));

                TData data = Data;
                return Result.Of(() => binder.Invoke(data).Resolve());
            }

            public IResult<TOut> Combine<TOther, TOut>(
                IResult<TOther> otherResult,
                Func<TData, TOther, TOut> combiner)
            {
                if (otherResult is null)
                    throw new ArgumentNullException(nameof(otherResult));

                if (combiner is null)
                    throw new ArgumentNullException(nameof(combiner));

                if (otherResult is IResult<TOther>.ErrorResult otherError)
                    return Result.Of<TOut>(otherError.Cause());

                var other = otherResult.Resolve();
                var data = Data;
                return Result.Of(() => combiner.Invoke(data, other));
            }

            public override string ToString() => Data?.ToString();

            public override int GetHashCode() => HashCode.Combine(Data);

            public override bool Equals(object obj)
            {
                return obj is DataResult other
                    && other.Data.NullOrEquals(Data);
            }

            public static DataResult Default => default;

            public bool IsDefault => Data.NullOrEquals(default);

            public static bool operator ==(DataResult first, DataResult second) => first.Equals(second);

            public static bool operator !=(DataResult first, DataResult second) => !first.Equals(second);
        }
        #endregion
    }
}
