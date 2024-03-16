using Axis.Luna.Result;

namespace Axis.Luna.Result
{
    public static class Result
    {
        #region Of

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IResult<TData> Of<TData>(TData data)
        {
            return new DataResult<TData>(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IResult<TData> Of<TData>(Exception error)
        {
            return new ErrorResult<TData>(error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="dataProducer"></param>
        /// <returns></returns>
        public static IResult<TData> Of<TData>(Func<TData> dataProducer)
        {
            ArgumentNullException.ThrowIfNull(dataProducer);

            try
            {
                return new DataResult<TData>(dataProducer.Invoke());
            }
            catch(Exception e)
            {
                return new ErrorResult<TData>(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="resultProducer"></param>
        /// <returns></returns>
        public static IResult<TData> Of<TData>(Func<IResult<TData>> resultProducer)
        {
            ArgumentNullException.ThrowIfNull(resultProducer);

            try
            {
                return resultProducer.Invoke();
            }
            catch (Exception e)
            {
                return new ErrorResult<TData>(e);
            }
        }

        #endregion

        #region Resolve

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static TData Resolve<TData>(this IResult<TData> result)
        {
            ArgumentNullException.ThrowIfNull(result);
            AssertValidResultType(result);

            return result switch
            {
                DataResult<TData> d => d.Data,
                ErrorResult<TData> e => e.Error.Throw<TData>(),
                _ => throw new ArgumentException(
                    $"Invalid result type: '{result?.GetType()}'")
            };
        }

        #endregion

        #region Is

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsDataResult<TData>(this IResult<TData> result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result is DataResult<TData>;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsDataResult<TData>(
            this IResult<TData> result,
            out TData data)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result is DataResult<TData> dresult)
            {
                data = dresult.Data;
                return true;
            }

            data = default!;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsErrorResult<TData>(this IResult<TData> result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result is ErrorResult<TData>;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool IsErrorResult<TData>(
            this IResult<TData> result,
            out Exception error)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result is ErrorResult<TData> eresult)
            {
                error = eresult.Error;
                return true;
            }

            error = default!;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool IsErrorResult<TData, TError>(
            this IResult<TData> result,
            out TError error)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result is ErrorResult<TData> eresult
                && eresult.Error is TError err)
            {
                error = err;
                return true;
            }

            error = default!;
            return false;
        }

        #endregion

        #region With

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="result"></param>
        /// <param name="consumer"></param>
        /// <returns></returns>
        public static IResult<TData> WithData<TData>(this IResult<TData> result, Action<TData> consumer)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(consumer);
            AssertValidResultType(result);

            try
            {
                result.Consume(consumer);
                return result;
            }
            catch(Exception e)
            {
                return new ErrorResult<TData>(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorConsumer"></param>
        /// <returns></returns>
        public static IResult<TData> WithError<TData>(
            this IResult<TData> result,
            Action<Exception> errorConsumer)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorConsumer);
            AssertValidResultType(result);

            try
            {
                if (result is ErrorResult<TData> eresult)
                    errorConsumer.Invoke(eresult.Error);

                return result;
            }
            catch (Exception e)
            {
                return new ErrorResult<TData>(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorConsumer"></param>
        /// <returns></returns>
        public static IResult<TData> WithError<TData, TError>(
            this IResult<TData> result,
            Action<Exception> errorConsumer)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorConsumer);
            AssertValidResultType(result);

            try
            {
                if (result is ErrorResult<TData> eresult
                    && eresult.Error is TError terror)
                    errorConsumer.Invoke(terror);

                return result;
            }
            catch (Exception e)
            {
                return new ErrorResult<TData>(e);
            }
        }

        #endregion

        #region MapError

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="result"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IResult<TOut> MapError<TOut>(
            this IResult<TOut> result,
            Func<Exception, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);
            AssertValidResultType(result);

            if (result is ErrorResult<TOut> eresult)
                return Result.Of(() => mapper.Invoke(eresult.Error));

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IResult<TOut> MapError<TOut, TError>(
            this IResult<TOut> result,
            Func<TError, TOut> mapper)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);
            AssertValidResultType(result);

            if (result is ErrorResult<TOut> eresult
                && eresult.Error is TError terror)
                return Result.Of(() => mapper.Invoke(terror));

            return result;
        }

        #endregion

        #region BindError

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="result"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IResult<TOut> BindError<TOut>(
            this IResult<TOut> result,
            Func<Exception, IResult<TOut>> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);
            AssertValidResultType(result);

            if (result is ErrorResult<TOut> eresult)
                return Result.Of(() => mapper.Invoke(eresult.Error));

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IResult<TOut> BindError<TOut, TError>(
            this IResult<TOut> result,
            Func<TError, IResult<TOut>> mapper)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);
            AssertValidResultType(result);

            if (result is ErrorResult<TOut> eresult
                && eresult.Error is TError terror)
                return Result.Of(() => mapper.Invoke(terror));

            return result;
        }

        #endregion

        #region ConsumeError

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorConsumer"></param>
        public static void ConsumeError<TData>(
            this IResult<TData> result,
            Action<Exception> errorConsumer)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorConsumer);
            AssertValidResultType(result);

            if (result is ErrorResult<TData> eresult)
                errorConsumer.Invoke(eresult.Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="errorConsumer"></param>
        public static void ConsumeError<TData, TError>(
            this IResult<TData> result,
            Action<TError> errorConsumer)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorConsumer);
            AssertValidResultType(result);

            if (result is ErrorResult<TData> eresult
                && eresult.Error is TError terror)
                errorConsumer.Invoke(terror);
        }

        #endregion

        #region TransformError

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="result"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IResult<TOut> TransformError<TOut>(
            this IResult<TOut> result,
            Func<Exception, Exception> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);
            AssertValidResultType(result);

            try
            {
                if (result is ErrorResult<TOut> eresult)
                    return Result.Of<TOut>(mapper.Invoke(eresult.Error));

                return result;
            }
            catch(Exception e)
            {
                return new ErrorResult<TOut>(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="result"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IResult<TOut> TransformError<TOut, TError>(
            this IResult<TOut> result,
            Func<TError, Exception> mapper)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);
            AssertValidResultType(result);

            try
            {
                if (result is ErrorResult<TOut> eresult
                    && eresult.Error is TError terror)
                    return Result.Of<TOut>(mapper.Invoke(terror));

                return result;
            }
            catch (Exception e)
            {
                return new ErrorResult<TOut>(e);
            }
        }

        #endregion

        #region Continue

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="result"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IResult<TOut> Continue<TIn, TOut>(
            this IResult<TIn> result,
            Func<object, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);
            AssertValidResultType(result);

            try
            {
                return result switch
                {
                    DataResult<TIn> dr => Result.Of(() => mapper.Invoke(dr.Data!)),
                    ErrorResult<TIn> er => Result.Of(() => mapper.Invoke(er.Error)),
                    _ => throw new InvalidOperationException(
                        $"Invalid result type: '{result?.GetType()}'")
                };
            }
            catch (Exception e)
            {
                return new ErrorResult<TOut>(e);
            }
        }

        #endregion

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
            ArgumentNullException.ThrowIfNull(results);

            var valueList = new List<TResult>();
            var errorList = new List<Exception>();
            foreach (var result in results)
            {
                if (result is DataResult<TResult> dataResult)
                    valueList.Add(dataResult.Data);

                else if (result is ErrorResult<TResult> errorResult)
                    errorList.Add(errorResult.Error);

                else throw new InvalidOperationException(
                    $"Invalid result type: '{result?.GetType()}'");
            }

            if (errorList.Count > 0)
                return Of<IEnumerable<TResult>>(errorList
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
            ArgumentNullException.ThrowIfNull(results);
            ArgumentNullException.ThrowIfNull(errorMapper);

            var errors = new List<Exception>();
            var values = results
                .Select(iresult =>
                {
                    if (iresult is DataResult<TResult> dataResult)
                        return dataResult.Data;

                    else if (iresult is ErrorResult<TResult> errorResult)
                    {
                        try
                        {
                            return errorMapper.Invoke(errorResult.Error);
                        }
                        catch (Exception e)
                        {
                            errors.Add(e);
                            return default;
                        }
                    }

                    else throw new InvalidOperationException($"Invalid result type: '{iresult?.GetType()}'");
                })
                .ToList();

            if (errors.Count > 0)
                return Of<IEnumerable<TResult>>(errors
                    .ToArray()
                    .ApplyTo(list => new AggregateException(list)));

            return Of<IEnumerable<TResult>>(values!);
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
            ArgumentNullException.ThrowIfNull(results);
            ArgumentNullException.ThrowIfNull(errorConsumer);

            return results
                .Aggregate(new List<TResult>(), (list, result) =>
                {
                    if (result is DataResult<TResult> dataResult)
                        list.Add(dataResult.Data);

                    else if (result is ErrorResult<TResult> errorResult)
                        errorConsumer.Invoke(errorResult.Error);

                    else throw new InvalidOperationException(
                        $"Invalid result type: '{result?.GetType()}'");

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

        private static void AssertValidResultType<T>(IResult<T> result)
        {
            if (result is not ErrorResult<T>
                && result is not DataResult<T>)
                throw new ArgumentException($"Invalid result type: '{result?.GetType()}'");
        }
    }
}
