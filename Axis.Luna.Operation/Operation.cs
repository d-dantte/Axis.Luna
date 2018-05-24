using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Utils
{
    public static class Operation
    {
        #region Try
        public static IOperation<Result> Try<Result>(Func<Result> func) => new Lazy.LazyOperation<Result>(func);
        public static IOperation Try(Action action) => new Lazy.LazyOperation(action);


        public static IOperation<Result> Try<Result>(Func<Task<Result>> func) => new Async.AsyncOperation<Result>(func);
        public static IOperation Try(Func<Task> action) => new Async.AsyncOperation(action);

        public static IOperation<Result> Try<Result>(Task<Result> task) => new Async.AsyncOperation<Result>(task);
        public static IOperation Try(Task task) => new Async.AsyncOperation(task);

        public static IOperation<Result> Try<Result>(Lazy<Result> lazy) => new Lazy.LazyOperation<Result>(lazy);


        public static IOperation<Result> Try<Result>(Func<IOperation<Result>> op) => new Lazy.LazyOperation<Result>(() => op.Invoke().Resolve());
        public static IOperation Try(Func<IOperation> op) => new Lazy.LazyOperation(() => op.Invoke().Resolve());
        #endregion


        #region Wait
        public static IOperation Wait(this IOperation prev)
        {
            if (prev == null) return Fail(new NullReferenceException());
            else
            {
                try
                {
                    prev.Resolve();
                }
                catch { }
                return prev;
            }
        }
        public static IOperation<Result> Wait<Result>(this IOperation<Result> prev)
        {
            if (prev == null) return Fail<Result>(new NullReferenceException());
            else
            {
                try
                {
                    prev.Resolve();
                }
                catch { }
                return prev;
            }
        }
        public static IOperation Wait(this IEnumerable<IOperation> oplist)
        => new Async.AsyncOperation(async () =>
        {
            var awaited = oplist.Select(async _op => await _op);
            await Task.WhenAll(awaited); //ensures all operations have been executed
        });
        #endregion


        #region Fold
        public static IOperation<Out> Fold<In, Out>(this IEnumerable<IOperation<In>> operations, Out seed, Func<Out, In, Out> reducer)
        => Operation.Try(async () =>
        {
            var accumulator = seed;
            foreach (var op in operations)
                accumulator = reducer.Invoke(accumulator, await op);

            return accumulator;
        });
        public static IOperation<Out> Fold<In, Out>(this IEnumerable<IOperation<In>> operations, Out seed, Func<Out, In, Task<Out>> reducer)
        => Operation.Try(async () =>
        {
            var accumulator = seed;
            foreach (var op in operations)
                accumulator = await reducer.Invoke(accumulator, await op);

            return accumulator;
        });
        #endregion


        #region Fail
        public static IOperation Fail(Exception exception = null)
        => new Lazy.LazyOperation(() =>
        {
            if (!string.IsNullOrWhiteSpace(exception?.StackTrace))
                ExceptionDispatchInfo
                    .Capture(exception)
                    .Throw();

            else
                throw exception ?? new InvalidOperationException();
        });

        public static IOperation<Result> Fail<Result>(Exception exception = null)
        => new Lazy.LazyOperation<Result>(() =>
        {
            if (!string.IsNullOrWhiteSpace(exception?.StackTrace))
                ExceptionDispatchInfo
                    .Capture(exception)
                    .Throw();

            else
                throw exception ?? new InvalidOperationException();

            //NOTE! this statement is never reached
            return default(Result);
        });
        #endregion


        #region Catch
        /// <summary>
        /// This will return a successfull operation unless the action delegate is null, or an exception is thrown from within the action delegate
        /// </summary>
        /// <param name="op"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IOperation Catch(this IOperation op, Action<Exception> action)
        {
            if (op is Lazy.LazyOperation) return new Lazy.LazyOperation(() =>
            {
                try
                {
                    op.Resolve();
                }
                catch (Exception e)
                {
                    action.Invoke(e);
                }
            });
            else
            {
                var t = (op as Async.AsyncOperation).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        await _t;
                    }
                    catch (Exception e)
                    {
                        action.Invoke(e);
                    }
                });
                return new Async.AsyncOperation(t.Unwrap());
            }
        }

        /// <summary>
        /// This will return a successfull operation unless the action delegate is null, or an exception is thrown from within the action delegate
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="op"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IOperation<R> Catch<R>(this IOperation<R> op, Func<Exception, R> action)
        {
            if (op is Lazy.LazyOperation<R>) return new Lazy.LazyOperation<R>(() =>
            {
                try
                {
                    return op.Resolve();
                }
                catch (Exception e)
                {
                    return action.Invoke(e);
                }
            });
            else
            {
                var t = (op as Async.AsyncOperation<R>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        return await _t;
                    }
                    catch (Exception e)
                    {
                        return action.Invoke(e);
                    }
                });
                return new Async.AsyncOperation<R>(t.Unwrap());
            }
        }
        #endregion


        #region Finally
        public static IOperation Finally(this IOperation op, Action @finally)
        {
            if (op is Lazy.LazyOperation) return new Lazy.LazyOperation(() =>
            {
                try
                {
                    op.Resolve();
                }
                finally
                {
                    @finally.Invoke();
                }
            });
            else
            {
                var t = (op as Async.AsyncOperation).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        await _t;
                    }
                    finally
                    {
                        @finally.Invoke();
                    }
                });
                return new Async.AsyncOperation(t.Unwrap());
            }
        }
        public static IOperation<R> Finally<R>(this IOperation<R> op, Action @finally)
        {
            if (op is Lazy.LazyOperation<R>) return new Lazy.LazyOperation<R>(() =>
            {
                try
                {
                    return op.Resolve();
                }
                finally
                {
                    @finally.Invoke();
                }
            });
            else
            {
                var t = (op as Async.AsyncOperation<R>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        return await _t;
                    }
                    finally
                    {
                        @finally.Invoke();
                    }
                });
                return new Async.AsyncOperation<R>(t.Unwrap());
            }
        }
        #endregion


        #region Then
        public static IOperation Then(this IOperation prev, Action next, Action<Exception> error = null)
        => new Lazy.LazyOperation(() =>
        {
            try
            {
                prev.Resolve();
                next?.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation Then<In>(this IOperation<In> prev, Action next, Action<Exception> error = null)
        => new Lazy.LazyOperation(() =>
        {
            try
            {
                prev.Resolve();
                next.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation Then<In>(this IOperation<In> prev, Action<In> next, Action<Exception> error = null)
        => new Lazy.LazyOperation(() =>
        {
            try
            {
                var _in = prev.Resolve();
                next.Invoke(_in);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });


        public static IOperation<Out> Then<Out>(this IOperation prev, Func<Out> next, Action<Exception> error = null)
        => new Lazy.LazyOperation<Out>(() =>
        {
            try
            {
                prev.Resolve();
                return next.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<Out> next, Action<Exception> error = null)
        => new Lazy.LazyOperation<Out>(() =>
        {
            try
            {
                prev.Resolve();
                return next.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<In, Out> next, Action<Exception> error = null)
        => new Lazy.LazyOperation<Out>(() =>
        {
            try
            {
                var _in = prev.Resolve();
                return next.Invoke(_in);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });


        public static IOperation Then(this IOperation prev, Func<Task> taskProducer, Action<Exception> error = null)
        {
            if (prev is Async.AsyncOperation)
            {
                var t = (prev as Async.AsyncOperation).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        await taskProducer.Invoke();
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation(t.Unwrap());
            }
            else return new Async.AsyncOperation(async () =>
            {
                try
                {
                    prev.Resolve();
                    await taskProducer.Invoke();
                }
                catch (Exception e)
                {
                    error?.Invoke(e);
                    throw;
                }
            });
        }
        public static IOperation Then<In>(this IOperation<In> prev, Func<Task> taskProducer, Action<Exception> error = null)
        {
            if (prev is Async.AsyncOperation<In>)
            {
                var t = (prev as Async.AsyncOperation<In>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        await taskProducer.Invoke();
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation(t.Unwrap());
            }
            else return new Async.AsyncOperation(Task.Run(() =>
            {
                try
                {
                    var _in = prev.Resolve();
                    taskProducer().GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    error?.Invoke(e);
                    throw;
                }
            }));
        }
        public static IOperation Then<In>(this IOperation<In> prev, Func<In, Task> taskProducer, Action<Exception> error = null)
        {
            if (prev is Async.AsyncOperation<In>)
            {
                var t = (prev as Async.AsyncOperation<In>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        var _in = _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        await taskProducer.Invoke(_in);
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation(t.Unwrap());
            }
            else return new Async.AsyncOperation(Task.Run(() =>
            {
                try
                {
                    var _in = prev.Resolve();
                    taskProducer(_in).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    error?.Invoke(e);
                    throw;
                }
            }));
        }


        public static IOperation<Out> Then<Out>(this IOperation prev, Func<Task<Out>> taskProducer, Action<Exception> error = null)
        {
            if (prev is Async.AsyncOperation)
            {
                var t = (prev as Async.AsyncOperation).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        return await taskProducer.Invoke();
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation<Out>(t.Unwrap());
            }
            else return new Async.AsyncOperation<Out>(Task.Run(() =>
            {
                try
                {
                    //using a task here instead of awaiting the previous operation because awaiting a lazy operation causes it to resolve,
                    //meaning this method will block till all of the previous operations have resolved - something we do not want happening here.
                    prev.Resolve();
                    return taskProducer().GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    error?.Invoke(e);
                    throw;
                }
            }));
        }
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<Task<Out>> taskProducer, Action<Exception> error = null)
        {
            if (prev is Async.AsyncOperation<In>)
            {
                var t = (prev as Async.AsyncOperation<In>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        return await taskProducer.Invoke();
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation<Out>(t.Unwrap());
            }
            else return new Async.AsyncOperation<Out>(Task.Run(() =>
            {
                try
                {
                    //using a task here instead of awaiting the previous operation because awaiting a lazy operation causes it to resolve,
                    //meaning this method will block till all of the previous operations have resolved - something we do not want happening here.
                    var _in = prev.Resolve();
                    return taskProducer().GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    error?.Invoke(e);
                    throw;
                }
            }));
        }
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<In, Task<Out>> taskProducer, Action<Exception> error = null)
        {
            if (prev is Async.AsyncOperation<In>)
            {
                var t = (prev as Async.AsyncOperation<In>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        var _in = _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        return await taskProducer.Invoke(_in);
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation<Out>(t.Unwrap());
            }
            else return new Async.AsyncOperation<Out>(Task.Run(() =>
            {
                try
                {
                    //using a task here instead of awaiting the previous operation because awaiting a lazy operation causes it to resolve,
                    //meaning this method will block till all of the previous operations have resolved - something we do not want happening here.
                    var _in = prev.Resolve();
                    return taskProducer(_in).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    error?.Invoke(e);
                    throw;
                }
            }));
        }


        public static IOperation Then(this IOperation prev, Func<IOperation> next, Action<Exception> error = null)
        => new Lazy.LazyOperation(() =>
        {
            try
            {
                prev.Resolve();
                next.Invoke().Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation Then<In>(this IOperation<In> prev, Func<IOperation> next, Action<Exception> error = null)
        => new Lazy.LazyOperation(() =>
        {
            try
            {
                prev.Resolve();
                next.Invoke().Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation Then<In>(this IOperation<In> prev, Func<In, IOperation> next, Action<Exception> error = null)
        => new Lazy.LazyOperation(() =>
        {
            try
            {
                var _in = prev.Resolve();
                next.Invoke(_in).Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation<Out> Then<Out>(this IOperation prev, Func<IOperation<Out>> next, Action<Exception> error = null)
        => new Lazy.LazyOperation<Out>(() =>
        {
            try
            {
                prev.Resolve();
                return next.Invoke().Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<IOperation<Out>> next, Action<Exception> error = null)
        => new Lazy.LazyOperation<Out>(() =>
        {
            try
            {
                prev.Resolve();
                return next.Invoke().Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<In, IOperation<Out>> next, Action<Exception> error = null)
        => new Lazy.LazyOperation<Out>(() =>
        {
            try
            {
                var _in = prev.Resolve();
                return next.Invoke(_in).Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });


        public static IOperation Then(this IOperation prev, Func<Task<IOperation>> next, Action<Exception> error = null)
        => new Async.AsyncOperation(async () =>
        {
            try
            {
                prev.Resolve();
                await await next.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation Then<In>(this IOperation<In> prev, Func<Task<IOperation>> next, Action<Exception> error = null)
        => new Async.AsyncOperation(async () =>
        {
            try
            {
                prev.Resolve();
                await await next.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation Then<In>(this IOperation<In> prev, Func<In, Task<IOperation>> next, Action<Exception> error = null)
        => new Async.AsyncOperation(async () =>
        {
            try
            {
                var _in = prev.Resolve();
                await await next.Invoke(_in);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation<Out> Then<Out>(this IOperation prev, Func<Task<IOperation<Out>>> next, Action<Exception> error = null)
        => new Async.AsyncOperation<Out>(async () =>
        {
            try
            {
                prev.Resolve();
                return await await next.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<Task<IOperation<Out>>> next, Action<Exception> error = null)
        => new Async.AsyncOperation<Out>(async () =>
        {
            try
            {
                prev.Resolve();
                return await await next.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<In, Task<IOperation<Out>>> next, Action<Exception> error = null)
        => new Async.AsyncOperation<Out>(async () =>
        {
            try
            {
                var _in = prev.Resolve();
                return await await next.Invoke(_in);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        });
        #endregion
    }
}