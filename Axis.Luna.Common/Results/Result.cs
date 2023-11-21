using Axis.Luna.Extensions;
using System;

namespace Axis.Luna.Common.Results
{
    public static class Result
    {
        #region Of
        public static IResult<TData> Of<TData>(TData data)
        {
            return new DataResult<TData>(data);
        }

        public static IResult<TData> Of<TData>(Exception error)
        {
            return new ErrorResult<TData>(error);
        }

        public static IResult<TData> Of<TData>(Func<TData> dataProducer)
        {
            try
            {
                return new DataResult<TData>(dataProducer.Invoke());
            }
            catch(Exception e)
            {
                return new ErrorResult<TData>(e);
            }
        }

        public static IResult<TData> Of<TData>(Func<IResult<TData>> dataProducer)
        {
            try
            {
                return dataProducer.Invoke();
            }
            catch (Exception e)
            {
                return new ErrorResult<TData>(e);
            }
        }

        #endregion

        #region Resolve

        public static TData Resolve<TData>(this IResult<TData> result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result switch
            {
                DataResult<TData> d => d.Data,
                ErrorResult<TData> e => e.Error.Throw<TData>(),
                _ => throw new InvalidOperationException(
                    $"Invalid result type: '{result?.GetType()}'")
            };
        }

        #endregion

        #region Is

        public static bool IsDataResult<TData>(this IResult<TData> result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result is DataResult<TData>;
        }

        public static bool IsDataResult<TData>(this IResult<TData> result, out TData data)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result is DataResult<TData> dresult)
            {
                data = dresult.Data;
                return true;
            }

            data = default;
            return false;
        }

        public static bool IsErrorResult<TData>(this IResult<TData> result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result is ErrorResult<TData>;
        }

        public static bool IsErrorResult<TData>(this IResult<TData> result, out Exception error)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result is ErrorResult<TData> eresult)
            {
                error = eresult.Error;
                return true;
            }

            error = default;
            return false;
        }

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

            error = default;
            return false;
        }

        #endregion

        #region With

        public static IResult<TData> With<TData>(this IResult<TData> result, Action<TData> consumer)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(consumer);

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

        public static IResult<TData> WithError<TData>(
            this IResult<TData> result,
            Action<Exception> errorConsumer)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorConsumer);

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

        public static IResult<TData> WithError<TData, TError>(
            this IResult<TData> result,
            Action<Exception> errorConsumer)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorConsumer);

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

        public static IResult<TOut> MapError<TOut>(
            this IResult<TOut> result,
            Func<Exception, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

            if (result is ErrorResult<TOut> eresult)
                return Result.Of(() => mapper.Invoke(eresult.Error));

            return result;
        }

        public static IResult<TOut> MapError<TOut, TError>(
            this IResult<TOut> result,
            Func<TError, TOut> mapper)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

            if (result is ErrorResult<TOut> eresult
                && eresult.Error is TError terror)
                return Result.Of(() => mapper.Invoke(terror));

            return result;
        }

        #endregion

        #region BindError

        public static IResult<TOut> BindError<TOut>(
            this IResult<TOut> result,
            Func<Exception, IResult<TOut>> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

            if (result is ErrorResult<TOut> eresult)
                return Result.Of(() => mapper.Invoke(eresult.Error));

            return result;
        }

        public static IResult<TOut> BindError<TOut, TError>(
            this IResult<TOut> result,
            Func<TError, IResult<TOut>> mapper)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

            if (result is ErrorResult<TOut> eresult
                && eresult.Error is TError terror)
                return Result.Of(() => mapper.Invoke(terror));

            return result;
        }

        #endregion

        #region ConsumeError

        public static void ConsumeError<TData>(
            this IResult<TData> result,
            Action<Exception> errorConsumer)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorConsumer);

            if (result is ErrorResult<TData> eresult)
                errorConsumer.Invoke(eresult.Error);
        }

        public static void ConsumeError<TData, TError>(
            this IResult<TData> result,
            Action<Exception> errorConsumer)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorConsumer);

            if (result is ErrorResult<TData> eresult
                && eresult.Error is TError terror)
                errorConsumer.Invoke(terror);
        }

        #endregion

        #region TransformError

        public static IResult<TOut> TransformError<TOut>(
            this IResult<TOut> result,
            Func<Exception, Exception> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

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

        public static IResult<TOut> TransformError<TOut, TError>(
            this IResult<TOut> result,
            Func<TError, Exception> mapper)
            where TError : Exception
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

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
        public static IResult<TOut> Continue<TIn, TOut>(
            this IResult<TIn> result,
            Func<object, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(mapper);

            try
            {
                return result switch
                {
                    DataResult<TIn> dr => Result.Of(() => mapper.Invoke(dr.Data)),
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


        #endregion
    }
}
