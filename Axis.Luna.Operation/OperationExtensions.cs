using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axis.Luna.Operation
{
    public static class OperationExtensions
    {

        #region Throw

        /// <summary>
        /// For a faulted operation, throws the new exception instead of the actual exception of the operation
        /// </summary>
        /// <param name="op"></param>
        /// <param name="newException"></param>
        /// <returns></returns>
        public static Operation ReThrow(this Operation op, Exception newException) => op.Catch(err => throw newException);

        /// <summary>
        /// For a failed operation, throws the generated exception instead of the actual exception of the operation
        /// </summary>
        /// <param name="op"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static Operation ReThrow(this Operation op, Func<Exception, Exception> map) => op.Catch(err => throw map(err));

        /// <summary> 
        /// For a faulted operation, throws the new exception instead of the actual exception of the operation
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="op"></param>
        /// <param name="newException"></param>
        /// <returns></returns>
        public static Operation<Result> ReThrow<Result>(this Operation<Result> op, Exception newException) => op.Catch(err =>
        {
            var condition = true;
            if (condition) throw newException;
            else return op.Resolve();
        });

        /// <summary>
        /// For a failed operation, throws the generated exception instead of the actual exception of the operation
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="op"></param>
        /// <param name="newException"></param>
        /// <returns></returns>
        public static Operation<Result> ReThrow<Result>(this Operation<Result> op, Func<Exception, Exception> map) => op.Catch(err =>
        {
            var condition = true;
            if (condition) throw map(err);
            else return op.Resolve();
        });
        #endregion

        #region Wait
        /// <summary>
        /// Returns a resolved operation whose "Successful" property will never be null
        /// </summary>
        /// <param name="prev"></param>
        /// <returns></returns>
        public static Operation Wait(this Operation prev)
        {
            if (prev == null) return Operation.Fail(new NullReferenceException());
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

        /// <summary>
        /// Returns a resolved operation whose "Successful" property will never be null
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="prev"></param>
        /// <returns></returns>
        public static Operation<Result> Wait<Result>(this Operation<Result> prev)
        {
            if (prev == null) return Operation.Fail<Result>(new NullReferenceException());
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
        #endregion

        #region Fold

        /// <summary>
        /// Folds multiple operations into a single one
        /// </summary>
        /// <param name="oplist"></param>
        /// <returns></returns>
        public static Operation Fold(this IEnumerable<Operation> oplist)
        => new Async.AsyncOperation(async () =>
        {
            var ops = oplist
                .Select(async _op => await _op)
                .ToArray();

            await Task.WhenAll(ops); //ensures all operations have been executed
        });

        public static Operation<IEnumerable<In>> Fold<In>(this IEnumerable<Operation<In>> oplist)
        => new Async.AsyncOperation<IEnumerable<In>>(async () =>
        {
            var list = new List<In>();

            foreach (var _op in oplist)
            {
                list.Add(await _op);
            }

            return list.Cast<In>();
        });

        public static Operation<Out> Fold<In, Out>(this IEnumerable<Operation<In>> operations, Out seed, Func<Out, In, Out> reducer)
        => Operation.Try(async () =>
        {
            var accumulator = seed;
            foreach (var op in operations)
                accumulator = reducer.Invoke(accumulator, await op);

            return accumulator;
        });

        public static Operation<Out> Fold<In, Out>(this IEnumerable<Operation<In>> operations, Out seed, Func<Out, In, Task<Out>> reducer)
        => Operation.Try(async () =>
        {
            var accumulator = seed;
            foreach (var op in operations)
                accumulator = await reducer.Invoke(accumulator, await op);

            return accumulator;
        });
        #endregion

        #region Catch
        public static Operation Catch(this Operation op, Action<Exception> action)
        {
            if (op is Lazy.LazyOperation || op is Sync.SyncOperation) return new Lazy.LazyOperation(() =>
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
        public static Operation Catch(this Operation op, Func<Exception, Task> action)
        {
            if (op is Lazy.LazyOperation || op is Sync.SyncOperation) return new Async.AsyncOperation(async () =>
            {
                try
                {
                    op.Resolve();
                }
                catch (Exception e)
                {
                    await action.Invoke(e);
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
                        await action.Invoke(e);
                    }
                });
                return new Async.AsyncOperation(t.Unwrap());
            }
        }
        public static Operation<R> Catch<R>(this Operation<R> op, Func<Exception, R> action)
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
        public static Operation<R> Catch<R>(this Operation<R> op, Func<Exception, Task<R>> action)
        {
            if (op is Lazy.LazyOperation<R> || op is Sync.SyncOperation<R>) return new Async.AsyncOperation<R>(async () =>
            {
                try
                {
                    return op.Resolve();
                }
                catch (Exception e)
                {
                    return await action.Invoke(e);
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
                        return await action.Invoke(e);
                    }
                });
                return new Async.AsyncOperation<R>(t.Unwrap());
            }
        }
        #endregion

        #region Then
        public static Operation Then(this Operation prev, Action next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation Then<In>(this Operation<In> prev, Action next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation Then<In>(this Operation<In> prev, Action<In> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);


        public static Operation<Out> Then<Out>(this Operation prev, Func<Out> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation<Out> Then<In, Out>(this Operation<In> prev, Func<Out> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation<Out> Then<In, Out>(this Operation<In> prev, Func<In, Out> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);


        public static Operation Then(this Operation prev, Func<Task> taskProducer, Action<Exception> error = null, Func<Task> rollback = null)
        {
            if (prev is Async.AsyncOperation)
            {
                var t = (prev as Async.AsyncOperation).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        await taskProducer.Invoke().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation(t.Unwrap(), rollback);
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
            }, rollback);
        }
        public static Operation Then<In>(this Operation<In> prev, Func<Task> taskProducer, Action<Exception> error = null, Func<Task> rollback = null)
        {
            if (prev is Async.AsyncOperation<In>)
            {
                var t = (prev as Async.AsyncOperation<In>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        await taskProducer.Invoke().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation(t.Unwrap(), rollback);
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
            }), rollback);
        }
        public static Operation Then<In>(this Operation<In> prev, Func<In, Task> taskProducer, Action<Exception> error = null, Func<Task> rollback = null)
        {
            if (prev is Async.AsyncOperation<In>)
            {
                var t = (prev as Async.AsyncOperation<In>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        var _in = _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        await taskProducer.Invoke(_in).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation(t.Unwrap(), rollback);
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
            }), rollback);
        }


        public static Operation<Out> Then<Out>(this Operation prev, Func<Task<Out>> taskProducer, Action<Exception> error = null, Func<Task> rollback = null)
        {
            if (prev is Async.AsyncOperation)
            {
                var t = (prev as Async.AsyncOperation).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        return await taskProducer.Invoke().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation<Out>(t.Unwrap(), rollback);
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
            }), rollback);
        }
        public static Operation<Out> Then<In, Out>(this Operation<In> prev, Func<Task<Out>> taskProducer, Action<Exception> error = null, Func<Task> rollback = null)
        {
            if (prev is Async.AsyncOperation<In>)
            {
                var t = (prev as Async.AsyncOperation<In>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        return await taskProducer.Invoke().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation<Out>(t.Unwrap(), rollback);
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
            }), rollback);
        }
        public static Operation<Out> Then<In, Out>(this Operation<In> prev, Func<In, Task<Out>> taskProducer, Action<Exception> error = null, Func<Task> rollback = null)
        {
            if (prev is Async.AsyncOperation<In>)
            {
                var t = (prev as Async.AsyncOperation<In>).GetTask().ContinueWith(async _t =>
                {
                    try
                    {
                        var _in = _t.GetAwaiter().GetResult(); //throws an exception if the previous task faulted
                        return await taskProducer.Invoke(_in).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                        throw;
                    }
                });
                return new Async.AsyncOperation<Out>(t.Unwrap(), rollback);
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
            }), rollback);
        }


        public static Operation Then(this Operation prev, Func<Operation> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation Then<In>(this Operation<In> prev, Func<Operation> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation Then<In>(this Operation<In> prev, Func<In, Operation> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation<Out> Then<Out>(this Operation prev, Func<Operation<Out>> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation<Out> Then<In, Out>(this Operation<In> prev, Func<Operation<Out>> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        }, rollback);
        public static Operation<Out> Then<In, Out>(this Operation<In> prev, Func<In, Operation<Out>> next, Action<Exception> error = null, Func<Task> rollback = null)
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
        },rollback);


        public static Operation Then(this Operation prev, Func<Task<Operation>> next, Action<Exception> error = null, Func<Task> rollback = null)
        => new Async.AsyncOperation(async () =>
        {
            try
            {
                prev.Resolve();
                await await next.Invoke().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        }, rollback);
        public static Operation Then<In>(this Operation<In> prev, Func<Task<Operation>> next, Action<Exception> error = null, Func<Task> rollback = null)
        => new Async.AsyncOperation(async () =>
        {
            try
            {
                prev.Resolve();
                await await next.Invoke().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        }, rollback);
        public static Operation Then<In>(this Operation<In> prev, Func<In, Task<Operation>> next, Action<Exception> error = null, Func<Task> rollback = null)
        => new Async.AsyncOperation(async () =>
        {
            try
            {
                var _in = prev.Resolve();
                await await next.Invoke(_in).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        }, rollback);
        public static Operation<Out> Then<Out>(this Operation prev, Func<Task<Operation<Out>>> next, Action<Exception> error = null, Func<Task> rollback = null)
        => new Async.AsyncOperation<Out>(async () =>
        {
            try
            {
                prev.Resolve();
                return await await next.Invoke().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        }, rollback);
        public static Operation<Out> Then<In, Out>(this Operation<In> prev, Func<Task<Operation<Out>>> next, Action<Exception> error = null, Func<Task> rollback = null)
        => new Async.AsyncOperation<Out>(async () =>
        {
            try
            {
                prev.Resolve();
                return await await next.Invoke().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        }, rollback);
        public static Operation<Out> Then<In, Out>(this Operation<In> prev, Func<In, Task<Operation<Out>>> next, Action<Exception> error = null, Func<Task> rollback = null)
        => new Async.AsyncOperation<Out>(async () =>
        {
            try
            {
                var _in = prev.Resolve();
                return await await next.Invoke(_in).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
        }, rollback);
        #endregion
    }
}
