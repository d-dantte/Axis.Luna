using Axis.Luna.Extensions;
using System;

namespace Axis.Luna.Common
{
    public interface IResult<TData>
    {
        /// <summary>
        /// Creates an instance of <see cref="IResult{TData}.ErrorResult"/>
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="exceptionData">The optional exception data</param>
        public static IResult<TData> Of(Exception exception, Types.Basic.BasicStruct? exceptionData = null) => new ErrorResult(exception, exceptionData);


        /// <summary>
        /// Creates an instance of <see cref="IResult{TData}.DataResult"/>
        /// </summary>
        /// <typeparam name="TData">The type for the result's data</typeparam>
        /// <param name="data">the data instance</param>
        public static IResult<TData> Of(TData data) => new DataResult(data);


        #region Union Types

        /// <summary>
        /// Represents a faulted result, and contains the exception
        /// </summary>
        public struct ErrorResult : IResult<TData>
        {
            private readonly Exception _cause;

            /// <summary>
            /// The error message
            /// </summary>
            public string Message => _cause?.Message;

            /// <summary>
            /// The optional error data 
            /// </summary>
            public Types.Basic.BasicStruct? ErrorData { get; }

            internal ErrorResult(Exception exception, Types.Basic.BasicStruct? errorData = null)
            {
                _cause = exception ?? throw new ArgumentNullException(nameof(exception));
                ErrorData = errorData;
            }

            /// <summary>
            /// Gets the exception
            /// </summary>
            public Exception Cause() => _cause;


            public override string ToString() => $"Error Result: {_cause?.ToString()}";

            public override int GetHashCode() => HashCode.Combine(_cause, ErrorData);

            public override bool Equals(object obj)
            {
                return obj is ErrorResult other
                    && other._cause.NullOrEquals(_cause)
                    && other.ErrorData.NullOrEquals(ErrorData);
            }

            public static bool operator ==(ErrorResult first, ErrorResult second) => first.Equals(second);

            public static bool operator !=(ErrorResult first, ErrorResult second) => !first.Equals(second);

        }


        /// <summary>
        /// Represents data.
        /// </summary>
        public struct DataResult : IResult<TData>
        {
            /// <summary>
            /// The result data
            /// </summary>
            public TData Data { get; }

            internal DataResult(TData data)
            {
                Data = data;
            }

            public override string ToString() => Data?.ToString();

            public override int GetHashCode() => HashCode.Combine(Data);

            public override bool Equals(object obj)
            {
                return obj is DataResult other
                    && other.Data.NullOrEquals(Data);
            }

            public static bool operator ==(DataResult first, DataResult second) => first.Equals(second);

            public static bool operator !=(DataResult first, DataResult second) => !first.Equals(second);
        }
        #endregion
    }


    public class InvalidResultTypeException: Exception
    {
        public InvalidResultTypeException()
            :base($"the supplied result is not a valid {typeof(IResult<>)} implementation")
        {
        }
    }


    public static class ResultExtensions
    {
        public static IResult<TOut> Map<TIn, TOut>(this
            IResult<TIn> result,
            Func<TIn, TOut> mapper,
            Func<Exception, object, TOut> errorMapper = null)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            try
            {
                return result switch
                {
                    IResult<TIn>.DataResult data => IResult<TOut>.Of(mapper.Invoke(data.Data)),

                    IResult<TIn>.ErrorResult error => errorMapper != null
                        ? IResult<TOut>.Of(errorMapper.Invoke(error.Cause(), error.ErrorData))
                        : IResult<TOut>.Of(error.Cause()),

                    _ => throw new InvalidResultTypeException()
                };
            }
            catch (InvalidResultTypeException)
            {
                throw;
            }
            catch (Exception e)
            {
                return IResult<TOut>.Of(e);
            }
        }

        public static IResult<TResult> MapError<TResult>(this
            IResult<TResult> result,
            Func<Exception, object, TResult> errorMapper)
        {
            if (errorMapper == null)
                throw new ArgumentNullException(nameof(errorMapper));

            try
            {
                return result switch
                {
                    IResult<TResult>.DataResult data => data,

                    IResult<TResult>.ErrorResult error => IResult<TResult>.Of(errorMapper.Invoke(error.Cause(), error.ErrorData)),

                    _ => throw new InvalidResultTypeException()
                };
            }
            catch (InvalidResultTypeException)
            {
                throw;
            }
            catch (Exception e)
            {
                return IResult<TResult>.Of(e);
            }
        }

        public static IResult<TResult> Resolve<TResult>(this Lazy<TResult> lazyValue)
        {
            try
            {
                return IResult<TResult>.Of(lazyValue.Value);
            }
            catch(Exception e)
            {
                return IResult<TResult>.Of(e);
            }
        }

        public static IResult<TResult>.DataResult AsData<TResult>(this IResult<TResult> result)
        {
            if (result is IResult<TResult>.DataResult data)
                return data;

            throw new InvalidCastException($"The result is not a {nameof(IResult<TResult>.DataResult)} instance");
        }

        public static IResult<TResult>.ErrorResult AsError<TResult>(this IResult<TResult> result)
        {
            if (result is IResult<TResult>.ErrorResult error)
                return error;

            throw new InvalidCastException($"The result is not a {nameof(IResult<TResult>.ErrorResult)} instance");
        }
    }
}
