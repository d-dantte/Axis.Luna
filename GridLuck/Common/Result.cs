using GridLuck.Common.Extensions;
using GridLuck.Common.Unions;

namespace GridLuck.Common.Results
{
    public readonly struct Result<TValue> :
        IUnion<TValue, Exception, Result<TValue>>,
        IUnionOf<TValue, Exception, Result<TValue>>,
        IUnionImplicits<TValue, Exception, Result<TValue>>
    {
        private readonly object? _value;

        object? IUnion<TValue, Exception, Result<TValue>>.Value => _value;

        public Result(TValue value) => _value = value;

        public Result(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            _value = exception;
        }

        public static Result<TValue> Of(TValue value) => new(value);

        public static Result<TValue> Of(Exception exception) => new(exception);

        public static Result<TValue> Of(Func<TValue> func)
        {
            ArgumentNullException.ThrowIfNull(func);

            try
            {
                return new(func.Invoke());
            }
            catch (Exception error)
            {
                return new(error);
            }
        }

        public static implicit operator Result<TValue>(TValue value) => new(value);

        public static implicit operator Result<TValue>(Exception value) => new(value);

        public TValue Resolve()
        {
            return _value switch
            {
                null => default!,
                TValue value => value,
                Exception error => error.Throw<TValue>(),
                _ => throw new InvalidOperationException(
                    $"Invalid result type [expected: {typeof(TValue)}, available: {_value.GetType()}]")
            };
        }
    }

    public static class Result
    {
        public static Task<Result<TValue>> ToAsyncResult<TValue>(this Task<TValue> task)
        {
            ArgumentNullException.ThrowIfNull(task);

            return task.ContinueWith(t =>
            {
                if (TaskStatus.RanToCompletion.Equals(t.Status))
                    return Result<TValue>.Of(t.Result);

                else if (t.Exception is not null)
                    return Result<TValue>.Of(t.Exception);

                else // cancelled
                    return Result<TValue>.Of(new TaskCanceledException());
            });
        }

        public static Result<IEnumerable<TValue>> Fold<TValue>(
            this IEnumerable<Result<TValue>> results,
            Func<Exception, TValue>? errorMapper = null)
        {
            ArgumentNullException.ThrowIfNull(results);

            var list = new List<TValue>();
            var errorList = new List<Exception>();
            foreach (var item in results)
            {
                if (item.IsType(out Exception? error))
                {
                    if (errorMapper is null)
                        errorList.Add(error!);
                    else
                    {
                        try
                        {
                            list.Add(errorMapper!.Invoke(error!));
                        }
                        catch (Exception ex)
                        {
                            errorList.Add(ex);
                        }
                    }
                }
                else list.Add(item.Resolve());
            }

            if (errorList.Count > 0)
                return Result<IEnumerable<TValue>>.Of(new AggregateException(errorList));

            else return Result<IEnumerable<TValue>>.Of(list);
        }


        /// <summary>
        /// Equivalent to <c>Fold().Match(items => aggregator.Invoke(items));</c>
        /// </summary>
        public static Result<TOut> FoldInto<TValue, TOut>(
            this IEnumerable<Result<TValue>> results,
            Func<IEnumerable<TValue>, TOut> aggregator)
        {
            return results
                .Fold()
                .Match(
                    aggregator,
                    error => error.Throw<TOut>())!;
        }
    }
}