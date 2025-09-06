namespace GridLuck.Common.Extensions
{
    public static class TaskExtensions
    {
        public static Task<TOut> Then<TIn, TOut>(
            this Task<TIn> task,
            Func<TIn, TOut> valueMapper,
            Func<Exception, TOut>? exceptionMapper = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(valueMapper);

            return task.ContinueWith(t =>
            {
                var exMapper = exceptionMapper switch
                {
                    null => (Exception ex) => ex.Throw<TOut>(),
                    _ => exceptionMapper
                };

                return t.Status switch
                {
                    TaskStatus.RanToCompletion => valueMapper.Invoke(t.Result),
                    TaskStatus.Canceled => exMapper.Invoke(new TaskCanceledException()),
                    _ => exMapper.Invoke(t.Exception!)
                };
            });
        }

        public static Task<TOut> Then<TOut>(
            this Task task,
            Func<TOut> valueProducer,
            Func<Exception, TOut>? exceptionMapper = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(valueProducer);

            return task.ContinueWith(t =>
            {
                var exMapper = exceptionMapper switch
                {
                    null => (Exception ex) => ex.Throw<TOut>(),
                    _ => exceptionMapper
                };

                return t.Status switch
                {
                    TaskStatus.RanToCompletion => valueProducer.Invoke(),
                    TaskStatus.Canceled => exMapper.Invoke(new TaskCanceledException()),
                    _ => exMapper.Invoke(t.Exception!)
                };
            });
        }

        public static Task<TOut> Then<TIn, TOut>(
            this Task<TIn> task,
            Func<TIn, Task<TOut>> asyncValueMapper,
            Func<Exception, Task<TOut>>? asyncExceptionMapper = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(asyncValueMapper);

            return task
                .ContinueWith(t =>
                {
                    var exMapper = asyncExceptionMapper switch
                    {
                        null => (Exception ex) => ex.Throw<Task<TOut>>(),
                        _ => asyncExceptionMapper
                    };

                    return t.Status switch
                    {
                        TaskStatus.RanToCompletion => asyncValueMapper.Invoke(t.Result),
                        TaskStatus.Canceled => exMapper.Invoke(new TaskCanceledException()),
                        _ => exMapper.Invoke(t.Exception!)
                    };
                })
                .Unwrap();
        }

        public static Task<TOut> Then<TOut>(
            this Task task,
            Func<Task<TOut>> asyncValueProducer,
            Func<Exception, Task<TOut>>? asyncExceptionMapper = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(asyncValueProducer);

            return task
                .ContinueWith(t =>
                {
                    var exMapper = asyncExceptionMapper switch
                    {
                        null => (Exception ex) => ex.Throw<Task<TOut>>(),
                        _ => asyncExceptionMapper
                    };

                    return t.Status switch
                    {
                        TaskStatus.RanToCompletion => asyncValueProducer.Invoke(),
                        TaskStatus.Canceled => exMapper.Invoke(new TaskCanceledException()),
                        _ => exMapper.Invoke(t.Exception!)
                    };
                })
                .Unwrap();
        }

        public static Task Then(
            this Task task,
            Action action,
            Action<Exception>? exceptionConsumer = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(action);

            return task.ContinueWith(t =>
            {
                var exMapper = exceptionConsumer switch
                {
                    null => (Exception ex) => ex.Throw(),
                    _ => exceptionConsumer
                };

                if (TaskStatus.RanToCompletion.Equals(t.Status))
                    action.Invoke();

                else if (TaskStatus.Canceled.Equals(t.Status))
                    exMapper.Invoke(new TaskCanceledException());

                else exMapper.Invoke(t.Exception!);
            });
        }

        public static Task Then<TOut>(
            this Task task,
            Func<Task> asyncValueProducer,
            Func<Exception, Task>? asyncExceptionMapper = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(asyncValueProducer);

            return task
                .ContinueWith(t =>
                {
                    var exMapper = asyncExceptionMapper switch
                    {
                        null => (Exception ex) => ex.Throw<Task<TOut>>(),
                        _ => asyncExceptionMapper
                    };

                    return t.Status switch
                    {
                        TaskStatus.RanToCompletion => asyncValueProducer.Invoke(),
                        TaskStatus.Canceled => exMapper.Invoke(new TaskCanceledException()),
                        _ => exMapper.Invoke(t.Exception!)
                    };
                })
                .Unwrap();
        }

        public static Task ThenConsume<TResult>(
            this Task<TResult> task,
            Action<TResult> consumer,
            Action<Exception>? exceptionConsumer = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(consumer);

            return task.ContinueWith(t =>
            {
                var exConsumer = exceptionConsumer switch
                {
                    null => (Exception ex) => ex.Throw(),
                    _ => exceptionConsumer
                };

                if (t.Status == TaskStatus.RanToCompletion)
                    consumer.Invoke(t.Result);

                else if (t.Status == TaskStatus.Canceled)
                    exConsumer.Invoke(new TaskCanceledException());

                else exConsumer.Invoke(t.Exception!);
            });
        }

        public static Task ThenConsume<TResult>(
            this Task<TResult> task,
            Func<TResult, Task> asyncConsumer,
            Func<Exception, Task>? asyncExceptionConsumer = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(asyncConsumer);

            return task
                .ContinueWith(t =>
                {
                    var exConsumer = asyncExceptionConsumer switch
                    {
                        null => (Exception ex) => ex.Throw<Task>(),
                        _ => asyncExceptionConsumer
                    };

                    if (t.Status == TaskStatus.RanToCompletion)
                        return asyncConsumer.Invoke(t.Result);

                    else if (t.Status == TaskStatus.Canceled)
                        return exConsumer.Invoke(new TaskCanceledException());

                    else return exConsumer.Invoke(t.Exception!);
                })
                .Unwrap();
        }
    }
}