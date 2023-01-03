using Axis.Luna.Extensions;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Common
{
    public interface IResult<TData>
    {
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
            public Types.Basic.BasicStruct? ErrorData => _cause?.ErrorResultData();

            internal ErrorResult(Exception exception)
            {
                _cause = exception ?? throw new ArgumentNullException(nameof(exception));
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


    /// <summary>
    /// Result extensions
    /// </summary>
    public static class Result
    {
        private static readonly string ExceptionDataKey = "Axis.Luna.Common.ErrorResult.ExceptionData";

        /// <summary>
        /// Creates an instance of <see cref="IResult{TData}.ErrorResult"/>
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="exceptionData">The optional exception data</param>
        public static IResult<TData> Of<TData>(Exception exception) => new IResult<TData>.ErrorResult(exception);


        /// <summary>
        /// Creates an instance of <see cref="IResult{TData}.DataResult"/>
        /// </summary>
        /// <typeparam name="TData">The type for the result's data</typeparam>
        /// <param name="data">the data instance</param>
        public static IResult<TData> Of<TData>(TData data) => new IResult<TData>.DataResult(data);

        /// <summary>
        /// Creates an <see cref="IResult{TData}"/> instance from the value returned by the function, or from 
        /// any thrown exception
        /// </summary>
        /// <typeparam name="TData">The result data</typeparam>
        /// <param name="valueSupplier">The function supplying the value</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IResult<TData> Of<TData>(Func<TData> valueSupplier)
        {
            if (valueSupplier is null)
                throw new ArgumentNullException(nameof(valueSupplier));

            try
            {
                return Result.Of(valueSupplier.Invoke());
            }
            catch (Exception e)
            {
                return Result.Of<TData>(e);
            }
        }

        public static IResult<TResult> ResolveResult<TResult>(this
            Lazy<TResult> lazyValue)
            => Result.Of(() => lazyValue.Value);


        public static Task<IResult<TResult>> AwaitResult<TResult>(this
            Task<TResult> task,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            return task.ContinueWith(
                continuationOptions: continuationOptions,
                continuationFunction: t =>
                {
                    return t.Status switch
                    {
                        TaskStatus.RanToCompletion => Result.Of(t.Result),
                        TaskStatus.Canceled => Result.Of<TResult>(new OperationCanceledException()),
                        TaskStatus.Faulted => Result.Of<TResult>(t.Exception),
                        _ => throw new InvalidOperationException($"Invalid task state: {t.Status}")
                    };
                });
        }


        public static IResult<TOut> Map<TIn, TOut>(this
            IResult<TIn> result,
            Func<TIn, TOut> mapper)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return result switch
            {
                IResult<TIn>.DataResult data => Result.Of(() => mapper.Invoke(data.Data)),

                IResult<TIn>.ErrorResult error => Result.Of<TOut>(error.Cause()),

                _ => throw new InvalidResultTypeException()
            };
        }

        public static void Consume<TResult>(this
            IResult<TResult> result,
            Action<TResult> valueConsumer)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (valueConsumer is null)
                throw new ArgumentNullException(nameof(valueConsumer));

            if (result is IResult<TResult>.DataResult data)
                valueConsumer.Invoke(data.Data);
        }


        public static IResult<TResult> MapError<TResult>(this
            IResult<TResult> result,
            Func<Exception, TResult> errorMapper)
        {
            if (errorMapper == null)
                throw new ArgumentNullException(nameof(errorMapper));

            return result switch
            {
                IResult<TResult>.DataResult data => data,

                IResult<TResult>.ErrorResult error => Result.Of(() => errorMapper.Invoke(error.Cause())),

                _ => throw new InvalidResultTypeException()
            };
        }

        public static void ConsumeError<TResult>(this
            IResult<TResult> result,
            Action<Exception> errorConsumer)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (errorConsumer is null)
                throw new ArgumentNullException(nameof(errorConsumer));

            if (result is IResult<TResult>.ErrorResult error)
                errorConsumer.Invoke(error.Cause());
        }


        public static IResult<TOut> Map<TIn, TOut>(this
            IResult<TIn> result,
            Func<TIn, TOut> valueMapper,
            Func<Exception, TIn> errorMapper)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (valueMapper == null)
                throw new ArgumentNullException(nameof(valueMapper));

            if (errorMapper == null)
                throw new ArgumentNullException(nameof(errorMapper));

            return result switch
            {
                IResult<TIn>.DataResult data => data.Map(valueMapper),

                IResult<TIn>.ErrorResult error => error.MapError(errorMapper).Map(valueMapper),

                _ => throw new InvalidResultTypeException()
            };
        }

        public static void Comsume<TIn>(this
            IResult<TIn> result,
            Action<TIn> valueConsumer,
            Action<Exception> errorConsumer = null)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (valueConsumer == null)
                throw new ArgumentNullException(nameof(valueConsumer));

            if (errorConsumer == null)
                throw new ArgumentNullException(nameof(errorConsumer));


            if (result is IResult<TIn>.DataResult data)
                data.Consume(valueConsumer);

            else if (result is IResult<TIn>.ErrorResult error)
                error.ConsumeError(errorConsumer);
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

        public static TException WithErrorResultData<TException>(this
            TException exception,
            Types.Basic.BasicStruct data)
            where TException : Exception
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            exception.Data[ExceptionDataKey] = data;
            return exception;
        }

        public static Types.Basic.BasicStruct? ErrorResultData<TException>(this
            TException exception)
            where TException : Exception
        {
            return exception.Data.TryGetValue(ExceptionDataKey, out var data)
                ? (Types.Basic.BasicStruct?)data
                : null;
        }
    }
}
