using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation
{
    /// <summary>
    /// NOTE: With respect to the <c>Then*</c> methods, the <c>errorHandler</c> function/action gives the opportunity to handle the error.
    /// If this invocation completes without any exceptions, the Operation is once again placed on the "successful" path. To propagate
    /// A faulted/failed operation, this function has to throw an exception.
    /// 
    /// NOTE: For all of the methods, find a way to detect failed operations that doesn't rely on catching an exception as the exception catching
    /// mechanism is EXTREMELY SLOW
    /// </summary>
    public static class OperationExtensions
    {
        #region Throw

        /// <summary>
        /// For a faulted operation, throws the new exception instead of the actual exception of the operation
        /// </summary>
        /// <param name="op"></param>
        /// <param name="newException"></param>
        /// <returns></returns>
        public static Operation ReThrow(this
            Operation op,
            Exception newException)
            => op.Catch(err => newException.Throw<Operation>());

        /// <summary>
        /// For a failed operation, throws the generated exception instead of the actual exception of the operation
        /// </summary>
        /// <param name="op"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static Operation ReThrow(this
            Operation op,
            Func<Exception, Exception> map)
            => op.Catch(err => map(err).Throw());

        /// <summary> 
        /// For a faulted operation, throws the new exception instead of the actual exception of the operation
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="op"></param>
        /// <param name="newException"></param>
        /// <returns></returns>
        public static Operation<Result> ReThrow<Result>(this 
            Operation<Result> op, 
            Exception newException) 
            => op.Catch(err =>
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
        public static Operation<Result> ReThrow<Result>(this 
            Operation<Result> op, 
            Func<Exception, Exception> map) 
            => op.Catch(err =>
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
            if (prev == null) 
                return Operation.Fail(new NullReferenceException());

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
            if (prev == null) 
                return Operation.Fail<Result>(new NullReferenceException());

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
        => Operation.Try(async () =>
        {
            foreach (var op in oplist)
                await op;
        });

        public static Operation<IEnumerable<In>> Fold<In>(this IEnumerable<Operation<In>> oplist)
        => Operation.Try(async () =>
        {
            var list = new List<In>();

            foreach (var _op in oplist)
            {
                list.Add(await _op);
            }

            return list.Cast<In>();
        });

        public static Operation<Out> Fold<In, Out>(this 
            IEnumerable<Operation<In>> operations, 
            Out seed, 
            Func<Out, In, Out> reducer)
        => Operation.Try(async () =>
        {
            var accumulator = seed;
            foreach (var op in operations)
                accumulator = reducer.Invoke(accumulator, await op);

            return accumulator;
        });

        public static Operation<Out> Fold<In, Out>(this 
            IEnumerable<Operation<In>> operations, 
            Out seed, Func<Out, In, Task<Out>> reducer)
        => Operation.Try(async () =>
        {
            var accumulator = seed;
            foreach (var op in operations)
                accumulator = await reducer.Invoke(accumulator, await op);

            return accumulator;
        });
        #endregion

        #region Reduce
        /// <summary>
        /// Use these functions in favor or <c>Operation.Result</c> & <c>Operation.Resole()</c>, except in the case of a faulted operation
        /// and the encapsulated exception is needed.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="operation"></param>
        /// <param name="default"></param>
        /// <param name="reducer"></param>
        /// <returns></returns>
        public static R Reduce<V, R>(this Operation<V> operation, V @default, Func<V, R> reducer)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            else if(reducer == null)
                throw new ArgumentNullException(nameof(reducer));

            else
                return operation
                    .Then(reducer)
                    .Catch(ex => reducer.Invoke(@default))
                    .Resolve();
        }

        public static R Reduce<V, R>(this 
            Operation<V> operation, 
            Func<V, R> reducer) 
            => Reduce(operation, default, reducer);
        #endregion

        #region Catch
        public static Operation Catch(this Operation op, Action<Exception> action)
        {
            if (op.Succeeded == true)
                return op;

            else if (!(op is Async.AsyncOperation))
            {
                return new Lazy.LazyOperation(() =>
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
            }
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
            if (op.Succeeded == true)
                return op;

            else if (!(op is Async.AsyncOperation))
            {
                try
                {
                    op.Resolve();
                    return Operation.FromVoid();
                }
                catch (Exception e)
                {
                    return new Async.AsyncOperation(() => action.Invoke(e));
                }
            }
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
        public static Operation Catch(this Operation op, Func<Exception, Operation> action)
        {
            if (op.Succeeded == true)
                return op;

            else if (op is Lazy.LazyOperation)
            {
                try
                {
                    return action.Invoke(op.Error.GetException());
                }
                catch (Exception e)
                {
                    return Operation.Fail(e);
                }
            }
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
            if (op.Succeeded == true)
                return op;

            else if (!(op is Async.AsyncOperation<R>))
            {
                return new Lazy.LazyOperation<R>(() =>
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
            }
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
            if (op.Succeeded == true)
                return op;

            else if (!(op is Async.AsyncOperation<R>))
            {
                return new Async.AsyncOperation<R>(async () =>
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
            }
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
        public static Operation<R> Catch<R>(this Operation<R> op, Func<Exception, Operation<R>> action)
        {
            if (op.Succeeded == true)
                return op;

            else if (!(op is Async.AsyncOperation<R>))
            {
                try
                {
                    return action.Invoke(op.Error.GetException());
                }
                catch (Exception e)
                {
                    return Operation.Fail<R>(e);
                }
            }
            else
            {
                var t = (op as Async.AsyncOperation<R>)
                    .GetTask()
                    .ContinueWith(async _t =>
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

        #region Then/Map

        #region 1

        #region 1
        public static Operation Then(this
            Operation prev,
            Action action,
            Action<Exception> errorHandler)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation(action);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            action.Invoke();

                        else if (errorHandler != null)
                            errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Lazy.LazyOperation(() =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    action.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }

        public static Operation Then(this
            Operation prev,
            Action action,
            Func<Exception, Task> errorHandler)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation(action);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            action.Invoke();

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    action.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }

        public static Operation Then(this
            Operation prev,
            Action action,
            Func<Exception, Operation> errorHandler)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation(action);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            action.Invoke();

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    action.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }
        #endregion

        #region 2
        public static Operation Then(this
            Operation prev,
            Func<Task> func,
            Action<Exception> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke();

                        else if (errorHandler != null)
                            errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }

        public static Operation Then(this
            Operation prev,
            Func<Task> func,
            Func<Exception, Task> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke();

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }

        public static Operation Then(this
            Operation prev,
            Func<Task> func,
            Func<Exception, Operation> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke();

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }
        #endregion

        #region 3
        public static Operation Then(this
            Operation prev,
            Func<Operation> func,
            Action<Exception> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(async () => await func.Invoke());

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke();

                        else if (errorHandler != null)
                            errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }

        public static Operation Then(this
            Operation prev,
            Func<Operation> func,
            Func<Exception, Task> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(async () => await func.Invoke());

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke();

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }

        public static Operation Then(this
            Operation prev,
            Func<Operation> func,
            Func<Exception, Operation> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(async () => await func.Invoke());

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke();

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return prev;
        }
        #endregion

        #endregion

        #region 2

        #region 1
        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Action<TIn> action,
            Action<Exception> errorHandler)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation(() => action.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            action.Invoke(t.Result);

                        else if (errorHandler != null)
                            errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Lazy.LazyOperation(() =>
                {
                    TIn @in = default;
                    try
                    {
                        @in = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    action.Invoke(@in);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }

        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Action<TIn> action,
            Func<Exception, Task> errorHandler)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation(() => action.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            action.Invoke(t.Result);

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    TIn @in = default;
                    try
                    {
                        @in = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    action.Invoke(@in);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }

        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Action<TIn> action,
            Func<Exception, Operation> errorHandler)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation(() => action.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            action.Invoke(t.Result);

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    TIn @in = default;
                    try
                    {
                        @in = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    action.Invoke(@in);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }
        #endregion

        #region 2
        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Func<TIn, Task> func,
            Action<Exception> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }

        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Func<TIn, Task> func,
            Func<Exception, Task> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }

        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Func<TIn, Task> func,
            Func<Exception, Operation> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }
        #endregion

        #region 3
        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Func<TIn, Operation> func,
            Action<Exception> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(async () => await func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }

        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Func<TIn, Operation> func,
            Func<Exception, Task> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(async () => await func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }

        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Func<TIn, Operation> func,
            Func<Exception, Operation> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation(async () => await func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw();
                    })
                    .Pipe(t => new Async.AsyncOperation(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            await errorHandler.Invoke(e);

                        else
                            ExceptionDispatchInfo
                                .Capture(e)
                                .Throw();

                        return;
                    }

                    await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail(prev.Error.GetException());
        }
        #endregion

        #endregion

        #region 3

        #region 1
        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<TOut> func,
            Func<Exception, TOut> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation<TOut>(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return func.Invoke();

                        else if (errorHandler != null)
                            return errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Lazy.LazyOperation<TOut>(() =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return errorHandler.Invoke(e);

                        else return ExceptionDispatchInfo
                            .Capture(e)
                            .Throw<TOut>();
                    }

                    return func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<TOut> func,
            Func<Exception, Task<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation<TOut>(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return func.Invoke();

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<TOut> func,
            Func<Exception, Operation<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation<TOut>(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return func.Invoke();

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }
        #endregion

        #region 2
        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<Task<TOut>> func,
            Func<Exception, TOut> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke();

                        else if (errorHandler != null)
                            return errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return errorHandler.Invoke(e);

                        else return ExceptionDispatchInfo
                            .Capture(e)
                            .Throw<TOut>();
                    }

                    return await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<Task<TOut>> func,
            Func<Exception, Task<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke();

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<Task<TOut>> func,
            Func<Exception, Operation<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(func);

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke();

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }
        #endregion

        #region 3
        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<Operation<TOut>> func,
            Func<Exception, TOut> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(async () => await func.Invoke());

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke();

                        else if (errorHandler != null)
                            return errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return errorHandler.Invoke(e);

                        else return ExceptionDispatchInfo
                            .Capture(e)
                            .Throw<TOut>();
                    }

                    return await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<Operation<TOut>> func,
            Func<Exception, Task<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(async () => await func.Invoke());

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke();

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<Operation<TOut>> func,
            Func<Exception, Operation<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(async () => await func.Invoke());

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke();

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return await func.Invoke();
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }
        #endregion

        #endregion

        #region 4

        #region 1
        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, TOut> func,
            Func<Exception, TOut> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation<TOut>(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Lazy.LazyOperation<TOut>(() =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return errorHandler.Invoke(e);

                        else return ExceptionDispatchInfo
                            .Capture(e)
                            .Throw<TOut>();
                    }

                    return func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, TOut> func,
            Func<Exception, Task<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation<TOut>(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, TOut> func,
            Func<Exception, Operation<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Lazy.LazyOperation<TOut>(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }
        #endregion

        #region 2
        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, Task<TOut>> func,
            Func<Exception, TOut> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return errorHandler.Invoke(e);

                        else return ExceptionDispatchInfo
                            .Capture(e)
                            .Throw<TOut>();
                    }

                    return await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, Task<TOut>> func,
            Func<Exception, Task<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, Task<TOut>> func,
            Func<Exception, Operation<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(() => func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return await func.Invoke(prev.Resolve());
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }
        #endregion

        #region 3
        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, Operation<TOut>> func,
            Func<Exception, TOut> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(async () => await func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return errorHandler.Invoke(e);

                        else return ExceptionDispatchInfo
                            .Capture(e)
                            .Throw<TOut>();
                    }

                    return await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Lazy.LazyOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, Operation<TOut>> func,
            Func<Exception, Task<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(async () => await func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    TIn result = default;
                    try
                    {
                        result = prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return await func.Invoke(result);
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(() => errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }

        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, Operation<TOut>> func,
            Func<Exception, Operation<TOut>> errorHandler)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (prev.Succeeded == true)
                return new Async.AsyncOperation<TOut>(async () => await func.Invoke(prev.Resolve()));

            //prev is an async op - handle it accordingly
            else if (prev is Async.AsyncOperation<TIn> asyncop)
            {
                return asyncop
                    .GetTask()
                    .ContinueWith(async t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                            return await func.Invoke(t.Result);

                        else if (errorHandler != null)
                            return await errorHandler.Invoke(t.Exception.InnerException);

                        else
                            return ExceptionDispatchInfo
                                .Capture(t.Exception.InnerException)
                                .Throw<TOut>();
                    })
                    .Pipe(t => new Async.AsyncOperation<TOut>(t.Unwrap()));
            }

            //prev is a lazy op that hasn't been executed
            else if (prev.Succeeded == null)
            {
                return new Async.AsyncOperation<TOut>(async () =>
                {
                    try
                    {
                        prev.Resolve();
                    }
                    catch (Exception e)
                    {
                        if (errorHandler != null)
                            return await errorHandler.Invoke(e);

                        else
                            return ExceptionDispatchInfo
                                .Capture(e)
                                .Throw<TOut>();
                    }

                    return await func.Invoke(prev.Resolve());
                });
            }

            //prev is either lazy or sync but is faulted 
            else if (errorHandler != null)
                return new Async.AsyncOperation<TOut>(async () => await errorHandler.Invoke(prev.Error.GetException()));

            //prev is lazy or sync, faulted, but no error handler was passed
            else
                return Operation.Fail<TOut>(prev.Error.GetException());
        }
        #endregion

        #endregion

        #region 5
        #region 1
        public static Operation Then(this
            Operation prev,
            Action action) => prev.Then(action, (Action<Exception>)null);

        public static Operation Then(this
            Operation prev,
            Func<Task> func) => prev.Then(func, (Action<Exception>)null);

        public static Operation Then(this
            Operation prev,
            Func<Operation> func) => prev.Then(func, (Action<Exception>)null);
        #endregion

        #region 2
        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Action<TIn> action) => prev.Then(action, (Action<Exception>)null);

        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Func<TIn, Task> func) => prev.Then(func, (Action<Exception>)null);

        public static Operation Then<TIn>(this
            Operation<TIn> prev,
            Func<TIn, Operation> func) => prev.Then(func, (Action<Exception>)null);
        #endregion

        #region 3
        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<TOut> func) => prev.Then(func, (Func<Exception, TOut>)null);

        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<Task<TOut>> func) => prev.Then(func, (Func<Exception, TOut>)null);

        public static Operation<TOut> Then<TOut>(this
            Operation prev,
            Func<Operation<TOut>> func) => prev.Then(func, (Func<Exception, TOut>)null);
        #endregion

        #region 4
        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, TOut> func) => prev.Then(func, (Func<Exception, TOut>)null);

        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, Task<TOut>> func) => prev.Then(func, (Func<Exception, TOut>)null);

        public static Operation<TOut> Then<TIn, TOut>(this
            Operation<TIn> prev,
            Func<TIn, Operation<TOut>> func) => prev.Then(func, (Func<Exception, TOut>)null);
        #endregion
        #endregion
        #endregion


        #region Misc
        internal static R Throw<R>(this ExceptionDispatchInfo info)
        {
            info.Throw();

            //never reached
            return default;
        }
        #endregion
    }

}
