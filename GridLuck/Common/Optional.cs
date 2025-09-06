using GridLuck.Common.Extensions;

namespace GridLuck.Common
{
    public readonly struct Optional<TValue>
        where TValue : class
    {
        private readonly TValue? _value;

        public bool IsEmpty => _value is null;

        public Optional(TValue? value)
        {
            _value = value;
        }

        public static Optional<TValue> Of(TValue? value) => new(value);

        public static Optional<TValue> Empty() => new(null);

        public static implicit operator Optional<TValue>(TValue? value) => new(value);

        public TValue Resolve(Exception? errorOnEmpty = null) => _value ?? throw errorOnEmpty ?? new NullReferenceException();
    }

    public static class OptionalExtensions
    {
        #region value -> optional
        public static Optional<TOut> MapOptional<TIn, TOut>(
            this TIn? @in,
            Func<TIn, TOut?> mapper,
            Func<TOut?>? nullMapper = null)
            where TIn : struct
            where TOut : class
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (@in is null)
            {
                if (nullMapper is not null)
                    return nullMapper.Invoke();

                return default;
            }

            return mapper.Invoke(@in!.Value);
        }

        public static async Task<Optional<TOut>> MapOptional<TIn, TOut>(
            this TIn? @in,
            Func<TIn, Task<TOut?>> mapper,
            Func<Task<TOut?>>? nullMapper = null)
            where TIn : struct
            where TOut : class
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (@in is null)
            {
                if (nullMapper is not null)
                    return await nullMapper.Invoke();

                return default;
            }

            return await mapper.Invoke(@in!.Value);
        }

        public static Task<Optional<TOut>> MapOptional<TIn, TOut>(
            this Task<TIn?> @in,
            Func<TIn, TOut?> mapper,
            Func<TOut?>? nullMapper = null)
            where TIn : struct
            where TOut : class
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(@in);

            var tcs = new TaskCompletionSource<Optional<TOut>>();
            _ = @in.ContinueWith(t =>
            {
                if (TaskStatus.RanToCompletion.Equals(t.Status))
                {
                    try
                    {
                        if (t.Result.HasValue)
                            tcs.SetResult(mapper.Invoke(t.Result.Value));

                        else tcs.SetResult(nullMapper?.Invoke() ?? default);
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                }

                else if (TaskStatus.Canceled.Equals(t.Status))
                    tcs.SetCanceled();

                else tcs.SetException(t.Exception!);
            });

            return tcs.Task;
        }

        public static Task<Optional<TOut>> MapOptional<TIn, TOut>(
            this Task<TIn?> @in,
            Func<TIn, Task<TOut?>> mapper,
            Func<Task<TOut?>>? nullMapper = null)
            where TIn : struct
            where TOut : class
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(@in);

            var tcs = new TaskCompletionSource<Optional<TOut>>();
            _ = @in.ContinueWith(t =>
            {
                if (TaskStatus.RanToCompletion.Equals(t.Status))
                {
                    try
                    {
                        var mt = t.Result.HasValue switch
                        {
                            true => mapper.Invoke(t.Result.Value),
                            false => nullMapper?.Invoke() ?? Task.FromResult<TOut?>(default)
                        };

                        _ = mt.ContinueWith(mmt =>
                        {
                            if (TaskStatus.RanToCompletion.Equals(mmt.Status))
                                tcs.SetResult(mmt.Result);

                            else if (TaskStatus.Canceled.Equals(mmt.Status))
                                tcs.SetCanceled();

                            else tcs.SetException(mmt.Exception!);
                        });
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                }

                else if (TaskStatus.Canceled.Equals(t.Status))
                    tcs.SetCanceled();

                else tcs.SetException(t.Exception!);
            });

            return tcs.Task;
        }
        #endregion

        #region optional -> optional

        public static Optional<TOut> MapOptional<TIn, TOut>(
            this Optional<TIn> @in,
            Func<TIn, TOut?> mapper,
            Func<TOut?>? nullMapper = null)
            where TIn : class
            where TOut : class
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (@in.IsEmpty)
                return nullMapper?.Invoke() ?? default;

            return mapper.Invoke(@in.Resolve());
        }

        public static async Task<Optional<TOut>> MapOptional<TIn, TOut>(
            this Optional<TIn> @in,
            Func<TIn, Task<TOut?>> mapper,
            Func<Task<TOut?>>? nullMapper = null)
            where TIn : class
            where TOut : class
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (@in.IsEmpty)
            {
                if (nullMapper is not null)
                    return await nullMapper.Invoke();

                else return default;
            }

            return await mapper.Invoke(@in.Resolve());
        }

        public static Task<Optional<TOut>> MapOptional<TIn, TOut>(
            this Task<TIn?> task,
            Func<TIn, TOut?> mapper,
            Func<TOut?>? nullMapper = null)
            where TIn : class
            where TOut : class
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(mapper);

            var taskCompletionSource = new TaskCompletionSource<Optional<TOut>>();
            task.ContinueWith(t =>
            {
                if (TaskStatus.RanToCompletion.Equals(t.Status))
                {
                    try
                    {
                        if (t.Result is not null)
                            taskCompletionSource.SetResult(mapper.Invoke(t.Result));

                        else if (nullMapper is not null)
                            taskCompletionSource.SetResult(nullMapper.Invoke());

                        else taskCompletionSource.SetResult(default);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }
                }
                else if (t.Exception is AggregateException ae)
                    taskCompletionSource.SetException(ae);

                else //if (TaskStatus.Canceled.Equals(t.Status))
                    taskCompletionSource.SetCanceled();
            });

            return taskCompletionSource.Task;
        }

        public static Task<Optional<TOut>> MapOptional<TIn, TOut>(
            this Task<Optional<TIn>> task,
            Func<TIn, Task<TOut?>> mapper,
            Func<Task<TOut?>>? nullMapper = null)
            where TIn : class
            where TOut : class
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(task);

            var tcs = new TaskCompletionSource<Optional<TOut>>();
            _ = task.ContinueWith(t =>
            {
                if (TaskStatus.RanToCompletion.Equals(t.Status))
                {
                    try
                    {
                        var mt = t.Result.IsEmpty switch
                        {
                            false => mapper.Invoke(t.Result.Resolve()),
                            true => nullMapper?.Invoke() ?? Task.FromResult<TOut?>(default)
                        };

                        _ = mt.ContinueWith(mmt =>
                        {
                            if (TaskStatus.RanToCompletion.Equals(mmt.Status))
                                tcs.SetResult(mmt.Result);

                            else if (TaskStatus.Canceled.Equals(mmt.Status))
                                tcs.SetCanceled();

                            else tcs.SetException(mmt.Exception!);
                        });
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                }

                else if (TaskStatus.Canceled.Equals(t.Status))
                    tcs.SetCanceled();

                else tcs.SetException(t.Exception!);
            });

            return tcs.Task;
        }
        #endregion

        #region optional -> value
        public static TOut? MapNullable<TIn, TOut>(
            this Optional<TIn> optional,
            Func<TIn, TOut?> mapper,
            Func<TOut?>? nullMapper = null)
            where TIn : class
            where TOut : struct
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (optional.IsEmpty)
            {
                if (nullMapper is not null)
                    return nullMapper.Invoke();

                return default;
            }

            return mapper.Invoke(optional.Resolve());
        }

        public static async Task<TOut?> MapNullable<TIn, TOut>(
            this Optional<TIn> optional,
            Func<TIn, Task<TOut?>> mapper,
            Func<Task<TOut?>>? nullMapper = null)
            where TIn : class
            where TOut : struct
        {
            ArgumentNullException.ThrowIfNull(mapper);

            if (optional.IsEmpty)
            {
                if (nullMapper is not null)
                    return await nullMapper.Invoke();

                return default;
            }

            return await mapper.Invoke(optional.Resolve());
        }

        public static Task<TOut?> MapNullable<TIn, TOut>(
            this Task<Optional<TIn>> task,
            Func<TIn, TOut?> mapper,
            Func<TOut?>? nullMapper = null)
            where TIn : class
            where TOut : struct
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(mapper);

            var taskCompletionSource = new TaskCompletionSource<TOut?>();
            task.ContinueWith(t =>
            {
                if (TaskStatus.RanToCompletion.Equals(t.Status))
                {
                    try
                    {
                        taskCompletionSource.SetResult(t.Result.MapNullable(mapper, nullMapper));
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }
                }
                else if (t.Exception is AggregateException ae)
                    taskCompletionSource.SetException(ae);

                else //if (TaskStatus.Canceled.Equals(t.Status))
                    taskCompletionSource.SetCanceled();
            });

            return taskCompletionSource.Task;
        }

        public static Task<TOut?> MapNullable<TIn, TOut>(
            this Task<Optional<TIn>> task,
            Func<TIn, Task<TOut?>> mapper,
            Func<Task<TOut?>>? nullMapper = null)
            where TIn : class
            where TOut : struct
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(task);

            var tcs = new TaskCompletionSource<TOut?>();
            _ = task.ContinueWith(t =>
            {
                if (TaskStatus.RanToCompletion.Equals(t.Status))
                {
                    try
                    {
                        var mt = t.Result.IsEmpty switch
                        {
                            false => mapper.Invoke(t.Result.Resolve()),
                            true => nullMapper?.Invoke() ?? Task.FromResult<TOut?>(default)
                        };

                        _ = mt.ContinueWith(mmt =>
                        {
                            if (TaskStatus.RanToCompletion.Equals(mmt.Status))
                                tcs.SetResult(mmt.Result);

                            else if (TaskStatus.Canceled.Equals(mmt.Status))
                                tcs.SetCanceled();

                            else tcs.SetException(mmt.Exception!);
                        });
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                }

                else if (TaskStatus.Canceled.Equals(t.Status))
                    tcs.SetCanceled();

                else tcs.SetException(t.Exception!);
            });

            return tcs.Task;
        }
        #endregion

        #region value -> value
        // use ApplyTo(...) :p
        #endregion

        #region Optoinal -> *
        public static void ConsumeOptional<TValue>(
            this Optional<TValue> optional,
            Action<TValue> consumer,
            Action? onEmpty = null)
            where TValue : class
        {
            ArgumentNullException.ThrowIfNull(consumer);

            if (!optional.IsEmpty)
                consumer.Invoke(optional.Resolve());

            else onEmpty?.Invoke();
        }

        public static async Task ConsumeOptionalAsync<TValue>(
            this Optional<TValue> optional,
            Func<TValue, Task> asyncConsumer,
            Func<Task>? asyncEmptyHandler = null)
            where TValue : class
        {
            ArgumentNullException.ThrowIfNull(asyncConsumer);

            if (!optional.IsEmpty)
                await asyncConsumer.Invoke(optional.Resolve());

            else if (asyncEmptyHandler is not null)
                await asyncEmptyHandler.Invoke();
        }

        public static async Task ConsumeOptionalAsync<TValue>(
            this Task<Optional<TValue>> task,
            Func<TValue, Task> asyncConsumer,
            Func<Task>? asyncEmptyHandler = null)
            where TValue : class
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(asyncConsumer);

            await task
                .Then(async optional =>
                {
                    if (!optional.IsEmpty)
                        await asyncConsumer.Invoke(optional.Resolve());

                    else if (asyncEmptyHandler is not null)
                        await asyncEmptyHandler.Invoke();
                })
                .Unwrap();
        }

        public static Optional<TValue> WithOptional<TValue>(
            this Optional<TValue> optional,
            Action<TValue> consumer,
            Action? onEmpty = null)
            where TValue : class
        {
            ArgumentNullException.ThrowIfNull(consumer);

            if (!optional.IsEmpty)
                consumer.Invoke(optional.Resolve());

            else onEmpty?.Invoke();

            return optional;
        }

        public static async Task<Optional<TValue>> WithOptionalAsync<TValue>(
            this Optional<TValue> optional,
            Func<TValue, Task> asyncConsumer,
            Func<Task>? asyncEmptyHandler = null)
            where TValue : class
        {
            ArgumentNullException.ThrowIfNull(asyncConsumer);

            if (!optional.IsEmpty)
                await asyncConsumer.Invoke(optional.Resolve());

            else if (asyncEmptyHandler is not null)
                await asyncEmptyHandler.Invoke();

            return optional;
        }

        public static async Task<Optional<TValue>> WithOptionalAsync<TValue>(
            this Task<Optional<TValue>> task,
            Func<TValue, Task> asyncConsumer,
            Func<Task>? asyncEmptyHandler = null)
            where TValue : class
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(asyncConsumer);

            return await task.Then(async optional =>
            {
                if (!optional.IsEmpty)
                    await asyncConsumer.Invoke(optional.Resolve());

                else if (asyncEmptyHandler is not null)
                    await asyncEmptyHandler.Invoke();

                return optional;
            });
        }
        #endregion

        public static TStruct? AsNullable<TStruct>(this TStruct @struct) where TStruct : struct
        {
            return @struct;
        }

        public static TStruct Resolve<TStruct>(this TStruct? @struct) where TStruct : struct
        {
            if (@struct.HasValue)
                return @struct.Value;

            throw new NullReferenceException();
        }

        public static Optional<TItem> FirstOrOptional<TItem>(
            this IEnumerable<TItem> items,
            Func<TItem, bool> predicate)
            where TItem : class
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(predicate);

            return items
                .FirstOrDefault(predicate)
                .ApplyTo(Optional<TItem>.Of);
        }


        public static Optional<TItem> FirstOrOptional<TItem>(
            this IEnumerable<TItem> items)
            where TItem : class
        {
            ArgumentNullException.ThrowIfNull(items);

            return items
                .FirstOrDefault()
                .ApplyTo(Optional<TItem>.Of);
        }
    }
}