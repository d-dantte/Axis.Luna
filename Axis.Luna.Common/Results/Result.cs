﻿using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axis.Luna.Common.Results
{
    /// <summary>
    /// Result extensions
    /// </summary>
    public static class Result
    {
        private static readonly string ExceptionDataKey = "Axis.Luna.Common.ErrorResult.ExceptionData";

        #region Type Check
        /// <summary>
        /// Checks if the supplied result is an instance of <see cref="IResult{TData}.DataResult"/>
        /// </summary>
        /// <typeparam name="TData">The type of the result</typeparam>
        /// <param name="result">The result instance</param>
        /// <returns>True if the instance is a Data result, false otherwise</returns>
        public static bool IsDataResult<TData>(this IResult<TData> result) => result is IResult<TData>.DataResult;

        public static bool IsDataResult<TData>(
            this IResult<TData> result,
            out TData data,
            Func<TData, bool> predicate = null)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (!result.IsDataResult())
            {
                data = default;
                return false;
            }

            data = result.Resolve();

            if (predicate is not null)
                return predicate.Invoke(data);

            return true;
        }

        public static bool IsErrorResult<TData, TError>(
            this IResult<TData> result,
            out TError error,
            Func<TError, bool> predicate = null)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsErrorResult()
                && result.AsError().ActualCause() is TError terror)
            {
                error = terror;

                if (predicate is not null)
                    return predicate.Invoke(error);

                return true;
            }

            error = default;
            return false;
        }

        /// <summary>
        /// Checks if the supplied result is an instance of <see cref="IResult{TData}.ErrorResult"/>
        /// </summary>
        /// <typeparam name="TData">The type of the result</typeparam>
        /// <param name="result">The result instance</param>
        /// <returns>True if the instance is a Error result, false otherwise</returns>
        public static bool IsErrorResult<TData>(this IResult<TData> result) => result is IResult<TData>.ErrorResult;
        #endregion

        #region Of

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
                return Of(valueSupplier.Invoke());
            }
            catch (Exception e)
            {
                return Of<TData>(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="resultSupplier"></param>
        /// <returns></returns>
        public static IResult<TOut> Of<TOut>(Func<IResult<TOut>> resultSupplier)
        {
            try
            {
                return resultSupplier.Invoke();
            }
            catch (Exception e)
            {
                return Result.Of<TOut>(e);
            }
        }

        #endregion

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
                return OfException(valueSupplier.Invoke());
            }
            catch (Exception e)
            {
                return OfError<TException>(e);
            }
        }
        #endregion

        #region Data/Task/Lazy Resolvers

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
                null => throw new ArgumentNullException(nameof(result)),
                _ => throw new InvalidResultTypeException(result.GetType())
            };
        }

        /// <summary>
        /// Resolves the <see cref="Lazy{T}"/> into a result
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="lazyValue">the lazy instance</param>
        public static IResult<TResult> ResolveResult<TResult>(this
            Lazy<TResult> lazyValue)
            => Of(() => lazyValue.Value);

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
                        TaskStatus.RanToCompletion => Of(t.Result),
                        TaskStatus.Canceled => Of<TResult>(new OperationCanceledException()),
                        TaskStatus.Faulted => Of<TResult>(t.Exception),
                        _ => throw new InvalidOperationException($"Invalid task state: {t.Status}")
                    };
                });
        }

        #endregion

        #region Consume

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
        /// Consumes the ResultError if available.
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="errorConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ConsumeError<TResult>(this
            IResult<TResult> result,
            Action<ResultException> errorConsumer)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (errorConsumer is null)
                throw new ArgumentNullException(nameof(errorConsumer));

            if (result is IResult<TResult>.ErrorResult error)
                errorConsumer.Invoke(error.Cause());
        }

        /// <summary>
        /// Consumes the cause of the result error, if available.
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <typeparam name="TCause">the type of error to consume, if available</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="errorConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ConsumeCause<TResult, TCause>(this
            IResult<TResult> result,
            Action<TCause> errorConsumer)
            where TCause : Exception
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (errorConsumer is null)
                throw new ArgumentNullException(nameof(errorConsumer));

            if (result.IsErrorResult(out TCause cause))
                errorConsumer.Invoke(cause);
        }
        #endregion

        #region With

        /// <summary>
        /// Consumes the value of the result, if available
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="valueConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IResult<TResult> WithValue<TResult>(this
            IResult<TResult> result,
            Action<TResult> valueConsumer)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (valueConsumer is null)
                throw new ArgumentNullException(nameof(valueConsumer));

            if (result.IsDataResult(out TResult data))
                valueConsumer.Invoke(data);

            return result;
        }

        /// <summary>
        /// Consumes the ResultError if available.
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="errorConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IResult<TResult> WithError<TResult>(this
            IResult<TResult> result,
            Action<ResultException> errorConsumer)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (errorConsumer is null)
                throw new ArgumentNullException(nameof(errorConsumer));

            if (result is IResult<TResult>.ErrorResult error)
                errorConsumer.Invoke(error.Cause());

            return result;
        }

        /// <summary>
        /// Consumes the cause of the result error, if available.
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <typeparam name="TCause">the type of error to consume, if available</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="errorConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IResult<TResult> WithCause<TResult, TCause>(this
            IResult<TResult> result,
            Action<TCause> errorConsumer)
            where TCause : Exception
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (errorConsumer is null)
                throw new ArgumentNullException(nameof(errorConsumer));

            if (result.IsErrorResult(out TCause cause))
                errorConsumer.Invoke(cause);

            return result;
        }

        #endregion

        #region Map Error

        /// <summary>
        /// Maps the encapsulated error if available, into a result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorMapper"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidResultTypeException"></exception>
        public static IResult<TResult> MapError<TResult>(this
            IResult<TResult> result,
            Func<ResultException, TResult> errorMapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorMapper);

            if (result is IResult<TResult>.ErrorResult errorResult)
                return Result.Of(() => errorMapper.Invoke(errorResult.Cause()));

            return result;
        }

        /// <summary>
        /// Maps the encapsulated cause if available, into a result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TCause"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorMapper"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidResultTypeException"></exception>
        public static IResult<TResult> MapCause<TResult, TCause>(this
            IResult<TResult> result,
            Func<TCause, TResult> errorMapper)
            where TCause : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorMapper);

            if (result.IsErrorResult(out TCause cause))
                return Result.Of(() => errorMapper.Invoke(cause));

            return result;
        }

        #endregion

        #region Bind Error

        /// <summary>
        /// Binds the encapsulated error if available, into a result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorMapper"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidResultTypeException"></exception>
        public static IResult<TResult> BindError<TResult>(this
            IResult<TResult> result,
            Func<ResultException, IResult<TResult>> errorMapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorMapper);

            if (result is IResult<TResult>.ErrorResult errorResult)
                return Result.Of(() => errorMapper.Invoke(errorResult.Cause()));

            return result;
        }

        /// <summary>
        /// Binds the encapsulated cause if available, into a result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TCause"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorMapper"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidResultTypeException"></exception>
        public static IResult<TResult> BindCause<TResult, TCause>(this
            IResult<TResult> result,
            Func<TCause, IResult<TResult>> errorMapper)
            where TCause : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorMapper);

            if (result.IsErrorResult(out TCause cause))
                return Result.Of(() => errorMapper.Invoke(cause));

            return result;
        }

        #endregion

        #region Transform Error

        /// <summary>
        /// Transforms the cause if available.
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="errorConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IResult<TResult> TransformCause<TResult>(this
            IResult<TResult> result,
            Func<Exception, Exception> errorMapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorMapper);

            if (result is IResult<TResult>.ErrorResult error)
                Result.Of<TResult>

            return result;
        }

        /// <summary>
        /// Consumes the cause of the result error, if available.
        /// </summary>
        /// <typeparam name="TResult">the encapsulated type</typeparam>
        /// <typeparam name="TCause">the type of error to consume, if available</typeparam>
        /// <param name="result">the result instance</param>
        /// <param name="errorConsumer">the consumer function</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IResult<TResult> WithCause<TResult, TCause>(this
            IResult<TResult> result,
            Action<TCause> errorConsumer)
            where TCause : Exception
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (errorConsumer is null)
                throw new ArgumentNullException(nameof(errorConsumer));

            if (result.IsErrorResult(out TCause cause))
                errorConsumer.Invoke(cause);

            return result;
        }

        #endregion

        #region Continue

        /// <summary>
        /// Maps the encapsulated payload - be it an exception, or data, using the <paramref name="mapper"/> function.
        /// </summary>
        /// <typeparam name="TIn">The input result type</typeparam>
        /// <typeparam name="TOut">The output result type</typeparam>
        /// <param name="result">The result instance</param>
        /// <param name="mapper">The mapper function</param>
        /// <returns>A result instance encapsulating the outcome of the mapping operation</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IResult<TOut> MapPayload<TIn, TOut>(
            this IResult<TIn> result,
            Func<object, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

            if (result is IResult<TIn>.DataResult dataResult)
                return Result.Of(() => mapper.Invoke(dataResult.Data));

            else if (result is IResult<TIn>.ErrorResult error)
                return Result.Of(() => mapper.Invoke(error.ActualCause()));

            else throw new InvalidOperationException($"Invalid result: '{result?.GetType()}'");
        }


        public static IResult<TOut> BindPayload<TIn, TOut>(
            this IResult<TIn> result,
            Func<object, IResult<TOut>> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

            if (result is IResult<TIn>.DataResult dataResult)
                return Result.Of(() => mapper.Invoke(dataResult.Data));

            else if (result is IResult<TIn>.ErrorResult error)
                return Result.Of(() => mapper.Invoke(error.ActualCause()));

            else throw new InvalidOperationException($"Invalid result: '{result?.GetType()}'");
        }

        #endregion


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
            object data)
            where TException : Exception
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            exception.Data[ExceptionDataKey] = data;
            return exception;
        }

        public static object ErrorResultData<TException>(this
            TException exception)
            where TException : Exception
        {
            return exception.Data.TryGetValue(ExceptionDataKey, out var data) ? data : null;
        }

        #region Fold

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
                return Of<IEnumerable<TResult>>(errorList
                    .Select(err => err is ResultException rex ? rex.InnerException : err)
                    .ToArray()
                    .ApplyTo(list => new AggregateException(list)));

            // else
            return Of<IEnumerable<TResult>>(valueList);
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
                return Of<IEnumerable<TResult>>(errors
                    .Select(err => err is ResultException rex ? rex.InnerException: err)
                    .ToArray()
                    .ApplyTo(list => new AggregateException(list)));

            return Of<IEnumerable<TResult>>(values);
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

            return results
                .ThrowIfNull(new ArgumentNullException(nameof(results)))
                .Aggregate(new List<TResult>(), (list, result) =>
                {
                    if (result is IResult<TResult>.DataResult dataResult)
                        list.Add(dataResult.Data);

                    else if (result is IResult<TResult>.ErrorResult errorResult)
                        errorConsumer.Invoke(errorResult.Cause());

                    return list;
                })
                .ApplyTo(values => Of<IEnumerable<TResult>>(values));
        }


        /// <summary>
        /// Equivalent to <c>Fold().Map(items => aggregator.Invoke(items));</c>
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="results"></param>
        /// <param name="aggregator"></param>
        /// <returns></returns>
        public static IResult<TOut> FoldInto<TItem, TOut>(
            this IEnumerable<IResult<TItem>> results,
            Func<IEnumerable<TItem>, TOut> aggregator)
        {
            return results.Fold().Map(aggregator);
        }

        #endregion
    }
}
