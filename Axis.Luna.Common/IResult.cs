using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axis.Luna.Common
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
        /// Binds the encapsulated result (or error) to a new result instance
        /// </summary>
        /// <typeparam name="TOut">the type of the output result</typeparam>
        /// <param name="binder">the binding function</param>
        IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder);

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
        #endregion

        #region Union Types

        /// <summary>
        /// Represents a faulted result, and contains the exception
        /// </summary>
        public readonly struct ErrorResult : IResult<TData>
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

            public IResult<TOut> Map<TOut>(Func<TData, TOut> mapper)
            {
                if (mapper is null)
                    throw new ArgumentNullException(nameof(mapper));

                return new IResult<TOut>.ErrorResult(_cause);
            }

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

            /// <summary>
            /// Gets the exception
            /// </summary>
            public Exception Cause() => _cause;

            /// <summary>
            /// Throws the error
            /// </summary>
            public TData ThrowError() => _cause.Throw<TData>();


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
        public readonly struct DataResult : IResult<TData>
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

            public IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder)
            {
                if (binder is null)
                    throw new ArgumentNullException(nameof(binder));

                TData data = Data;
                return Result.Of(() => binder.Invoke(data).Resolve());
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

    /// <summary>
    /// Indicates that the encountered result instance is not one of either <see cref="IResult{TData}.DataResult"/>, or <see cref="IResult{TData}.ErrorResult"/>
    /// </summary>
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
        /// Resolves the result; returning its value, or throwing its error.
        /// </summary>
        /// <typeparam name="TData">The type of the result</typeparam>
        /// <param name="result">The result instance</param>
        /// <returns>The encapsulated value</returns>
        /// <exception cref="InvalidResultTypeException">If the result instance isn't valid</exception>
        public static TData Resolve<TData>(this IResult<TData> result)
        {
            return result switch
            {
                IResult<TData>.DataResult data => data.Data,
                IResult<TData>.ErrorResult error => error.ThrowError(),
                _ => throw new InvalidResultTypeException()
            };
        }

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


        #region Exception
        public static IResult<TException> OfError<TException>(Exception exception)
        where TException : Exception => new IResult<TException>.ErrorResult(exception);

        public static IResult<TException> OfException<TException>(TException data)
        where TException : Exception => new IResult<TException>.DataResult(data);

        public static IResult<TException> OfException<TException>(Func<TException> valueSupplier)
        where TException : Exception
        {
            if (valueSupplier is null)
                throw new ArgumentNullException(nameof(valueSupplier));

            try
            {
                return Result.OfException(valueSupplier.Invoke());
            }
            catch (Exception e)
            {
                return Result.OfError<TException>(e);
            }
        }
        #endregion

        /// <summary>
        /// Resolves the <see cref="Lazy{T}"/> into a result
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="lazyValue">the lazy instance</param>
        public static IResult<TResult> ResolveResult<TResult>(this
            Lazy<TResult> lazyValue)
            => Result.Of(() => lazyValue.Value);

        /// <summary>
        /// Maps the result of the task to a <see cref="IResult{TData}"/>. Awaiting the new task returns the result instance.
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="task">the task instance</param>
        /// <param name="continuationOptions">continuation options if available</param>
        /// <returns>the new task</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Consumes the value of the result, if available
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="valueConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>
        /// Maps the encapsulate error if available, into a result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorMapper"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidResultTypeException"></exception>
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

        /// <summary>
        /// Consumes the error of the result, if available
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="errorConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>
        /// Consumes either the encapsulated value or error
        /// </summary>
        /// <typeparam name="TIn">the results type</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="valueConsumer">the value consumer function</param>
        /// <param name="errorConsumer">the error consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Consume<TIn>(this
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

        public static TException WithErrorData<TException>(this
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

        /// <summary>
        /// Folds the list of results into a result of list of values. All erros encountered are grouped into
        /// an aggregate exception and returned in an <see cref="IResult{TData}.ErrorResult"/> instance. If no
        /// error is present, a <see cref="IResult{TData}.DataResult"/> is returned.
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="results">the list of results</param>
        /// <returns>the folded result instance</returns>
        public static IResult<IEnumerable<TResult>> Fold<TResult>(this IEnumerable<IResult<TResult>> results)
        {
            if (results is null)
                throw new ArgumentNullException(nameof(results));

            var valueList = new List<TResult>();
            var errorList = new List<Exception>();
            foreach (var result in results)
            {
                if (result is IResult<TResult>.DataResult dataResult)
                    valueList.Add(dataResult.Data);

                else if (result is IResult<TResult>.ErrorResult errorResult)
                    errorList.Add(errorResult.Cause());
            }

            if (errorList.Count > 0)
                return Result.Of<IEnumerable<TResult>>(new AggregateException(errorList.ToArray()));

            // else
            return Result.Of<IEnumerable<TResult>>(valueList);
        }

        /// <summary>
        /// Folds the list of results into a result of list of values, with all encountered errors being mapped to actual values:
        /// a failure in the mapping process then results in an <see cref="IResult{TData}.ErrorResult"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="results"></param>
        /// <param name="errorMapper"></param>
        /// <returns></returns>
        public static IResult<IEnumerable<TResult>> Fold<TResult>(this
            IEnumerable<IResult<TResult>> results,
            Func<Exception, TResult> errorMapper)
        {
            if (errorMapper is null)
                throw new ArgumentNullException(nameof(errorMapper));

            var errors = new List<Exception>();
            var values = results
                .ThrowIfNull(new ArgumentNullException(nameof(results)))
                .Select(iresult =>
                {
                    if (iresult is IResult<TResult>.DataResult dataResult)
                        return dataResult.Data;

                    else if (iresult is IResult<TResult>.ErrorResult errorResult)
                    {
                        try
                        {
                            return errorMapper.Invoke(errorResult.Cause());
                        }
                        catch (Exception e)
                        {
                            errors.Add(e);
                            return default;
                        }
                    }

                    else throw new InvalidOperationException($"Invalid result: {iresult}");
                })
                .ToList();

            if (errors.Count > 0)
                return Result.Of<IEnumerable<TResult>>(new AggregateException(errors.ToArray()));

            return Result.Of<IEnumerable<TResult>>(values);
        }

        /// <summary>
        /// Folds the list of results into a result of list of values, with all encountered errors being consumed and then skipped:
        /// a failure in any error consumption process is thrown
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="results"></param>
        /// <param name="errorConsumer"></param>
        /// <returns></returns>
        public static IResult<IEnumerable<TResult>> Fold<TResult>(this
            IEnumerable<IResult<TResult>> results,
            Action<Exception> errorConsumer)
        {
            if (errorConsumer is null)
                throw new ArgumentNullException(nameof(errorConsumer));

            return  results
                .ThrowIfNull(new ArgumentNullException(nameof(results)))
                .Aggregate(new List<TResult>(), (list, result) =>
                {
                    if (result is IResult<TResult>.DataResult dataResult)
                        list.Add(dataResult.Data);

                    else if (result is IResult<TResult>.ErrorResult errorResult)
                        errorConsumer.Invoke(errorResult.Cause());

                    return list;
                })
                .ApplyTo(values => Result.Of<IEnumerable<TResult>>(values));
        }
    }
}
