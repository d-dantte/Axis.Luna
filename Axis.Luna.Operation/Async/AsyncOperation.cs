using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Async
{
    /// <summary>
    /// 
    /// </summary>
    public class AsyncOperation : Operation, IResolvable<Task>
    {
        private OperationError _error;
        private readonly Task _task;
        private readonly AsyncAwaiter _taskAwaiter;

        /// <summary>
        /// Creates a new AsyncOperation.
        /// 
        /// <para>
        /// NOTE: if the task producer returns a task that was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="taskProducer">A delegate that returns the task that this operation encapsulates</param>
        internal AsyncOperation(Func<Task> taskProducer)
        {
            if (taskProducer == null)
                throw new NullReferenceException("Invalid Task Producer Supplied");

            try
            {
                var _task = taskProducer?.Invoke();

                //start the task else it cannot be awaited
                if (_task.Status == TaskStatus.Created)
                    _task.Start();

               this._task = _task.ContinueWith(_t =>
                {
                    if (_t.Status == TaskStatus.Faulted
                        || _t.Status == TaskStatus.Canceled)
                    {
                        var exception =
                            _t.Exception?.InnerException
                            ?? new Exception($"Task status is '{_t.Status}', yet no exception was thrown within the task");
                        SetError(exception);

                        ExceptionDispatchInfo.Capture(exception).Throw();
                    }
                });

                _taskAwaiter = new AsyncAwaiter(this._task);
            }

            #region Exceptions thrown from the producer
            catch (OperationException oe)
            {
                _error = oe.Error;
                _task = Task.FromException(_error.GetException());
                _taskAwaiter = new AsyncAwaiter(_task);
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    code: "GeneralError",
                    message: e.Message,
                    exception: e);

                _task = Task.FromException(e);
                _taskAwaiter = new AsyncAwaiter(_task);
            }
            #endregion
        }

        /// <summary>
        /// Creates a new AsyncOperation.
        /// 
        /// <para>
        /// NOTE: if the task supplied was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="task">The task that this operation encapsulates</param>
        internal AsyncOperation(Task task)
        {
            if(task == null)
                throw new NullReferenceException("Invalid task supplied");

            //start the task else it cannot be awaited
            if (task.Status == TaskStatus.Created)
                task.Start();

            _task = task.ContinueWith(_t =>
            {
                if (_t.Status == TaskStatus.Faulted
                    || _t.Status == TaskStatus.Canceled)
                {
                    var exception =
                        _t.Exception?.InnerException
                        ?? new Exception($"Task status is '{_t.Status}', yet no exception was thrown within the task");
                    SetError(exception);

                    ExceptionDispatchInfo.Capture(exception).Throw();
                }
            });

            _taskAwaiter = new AsyncAwaiter(_task);
        }


        public override bool? Succeeded
        {
            get
            {
                switch (_task.Status)
                {
                    case TaskStatus.RanToCompletion: return true;

                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled: return false;

                    default: return null;
                }
            }
        }

        public override IAwaiter GetAwaiter() => _taskAwaiter;

        public override OperationError Error => _error;

        public Task GetTask() => _task;

        private void SetError(Exception e)
        {
            if (_error == null)
            {
                _error = e switch
                {
                    OperationException oe => oe.Error,

                    _ => new OperationError(
                       message: e.Message,
                       code: "GeneralError",
                       exception: e),
                };
            }
        }


        #region Resolvable

        Task IResolvable<Task>.Resolve() => _task;

        bool IResolvable<Task>.TryResolve(out Task result, out OperationError error)
        {
            error = null;
            result = _task;
            return true;
        }
        #endregion


        #region Mappers

        #region Success mappers
        public override Operation Then(Action action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                await this;
                action.Invoke();
            });
        }

        public override Operation Then(Func<Task> action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                await this;
                await action.Invoke();
            });
        }

        public override Operation Then(Func<Operation> action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                await this;
                await action.Invoke();
            });
        }

        public override Operation<TOut> Then<TOut>(Func<TOut> action)
        {
            if (TryInvalidateArguments<TOut>(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                await this;
                return action.Invoke();
            });
        }

        public override Operation<TOut> Then<TOut>(Func<Task<TOut>> action)
        {
            if (TryInvalidateArguments<TOut>(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                await this;
                return await action.Invoke();
            });
        }

        public override Operation<TOut> Then<TOut>(Func<Operation<TOut>> action)
        {
            if (TryInvalidateArguments<TOut>(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                await this;
                return await action.Invoke();
            });
        }
        #endregion

        #region Failure mappers
        public override Operation MapError(Action<OperationError> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    await this;
                }
                catch
                {
                    failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation MapError(Func<OperationError, Task> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    await this;
                }
                catch
                {
                    await failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation MapError(Func<OperationError, Operation> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    await this;
                }
                catch
                {
                    await failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, TOut> failureHandler)
        {
            if (TryInvalidateArguments<TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    await this;
                    return default;
                }
                catch
                {
                    return failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, Task<TOut>> failureHandler)
        {
            if (TryInvalidateArguments<TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    await this;
                    return default;
                }
                catch
                {
                    return await failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, Operation<TOut>> failureHandler)
        {
            if (TryInvalidateArguments<TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    await this;
                    return default;
                }
                catch
                {
                    return await failureHandler.Invoke(this.Error);
                }
            });
        }
        #endregion

        private static bool TryInvalidateArguments(
            Operation prev,
            object next,
            out Operation @out,
            bool invalidTryState = false)
        {
            if (prev == null)
                @out = Operation.Fail(new ArgumentNullException("Previous operation cannot be null"));

            else if (next == null)
                @out = Operation.Fail(new ArgumentNullException("Next action cannot be null"));

            else if (invalidTryState == false && prev.Succeeded == invalidTryState)
                @out = Operation.Fail(prev.Error);

            else if (invalidTryState == true && prev.Succeeded == invalidTryState)
                @out = prev;

            else
            {
                @out = null;
                return false;
            }

            return true;
        }

        private static bool TryInvalidateArguments<TOut>(
            Operation prev,
            object next,
            out Operation<TOut> @out,
            bool invalidTryState = false)
        {
            if (prev == null)
                @out = Operation.Fail<TOut>(new ArgumentNullException("Previous operation cannot be null"));

            else if (next == null)
                @out = Operation.Fail<TOut>(new ArgumentNullException("Next action cannot be null"));

            else if (invalidTryState == false && prev.Succeeded == invalidTryState)
                @out = Operation.Fail<TOut>(prev.Error);

            else if (invalidTryState == true && prev.Succeeded == invalidTryState)
                @out = Operation.FromResult<TOut>(default);

            else
            {
                @out = null;
                return false;
            }

            return true;
        }

        #endregion


        public static implicit operator AsyncOperation(Func<Task> func) => new AsyncOperation(func);

        public static implicit operator AsyncOperation(Task task) => new AsyncOperation(task);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class AsyncOperation<TResult> : Operation<TResult>, IResolvable<Task<TResult>>
    {
        private OperationError _error;
        private readonly Task<TResult> _task;
        private readonly AsyncAwaiter<TResult> _taskAwaiter;

        /// <summary>
        /// Creates a new AsyncOperation.
        /// 
        /// <para>
        /// NOTE: if the task producer returns a task that was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="taskProducer">A delegate that returns the task that this operation encapsulates</param>
        internal AsyncOperation(Func<Task<TResult>> taskProducer)
        {
            if (taskProducer == null)
                throw new NullReferenceException("Invalid Task Producer Supplied");

            try
            {
                var _task = taskProducer.Invoke();

                if (_task.Status == TaskStatus.Created)
                    _task.Start();

                this._task = _task.ContinueWith(_t =>
                {
                    if (_t.Status == TaskStatus.Faulted
                        || _t.Status == TaskStatus.Canceled)
                    {
                        var exception =
                            _t.Exception?.InnerException
                            ?? new Exception($"Task status is '{_t.Status}', yet no exception was thrown within the task");
                        SetError(exception);

                        ExceptionDispatchInfo.Capture(exception).Throw();

                        //this is not reached
                        return default;
                    }

                    else return _t.Result;
                });

                _taskAwaiter = new AsyncAwaiter<TResult>(this._task);
            }

            #region Exceptions thrown from the producer
            catch (OperationException oe)
            {
                _error = oe.Error;
                _task = Task.FromException<TResult>(_error.GetException());
                _taskAwaiter = new AsyncAwaiter<TResult>(_task);
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    code: "GeneralError",
                    message: e.Message,
                    exception: e);

                _task = Task.FromException<TResult>(e);
                _taskAwaiter = new AsyncAwaiter<TResult>(_task);
            }
            #endregion
        }

        /// <summary>
        /// Creates a new AsyncOperation.
        /// 
        /// <para>
        /// NOTE: if the task supplied was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="task">The task that this operation encapsulates</param>
        internal AsyncOperation(Task<TResult> task)
        {
            var _task = task ?? throw new NullReferenceException("Invalid task supplied");

            if (_task.Status == TaskStatus.Created)
                _task.Start();

            this._task = _task.ContinueWith(_t =>
            {
                if (_t.Status == TaskStatus.Faulted
                    || _t.Status == TaskStatus.Canceled)
                {
                    var exception =
                        _t.Exception?.InnerException
                        ?? new Exception($"Task status is '{_t.Status}', yet no exception was thrown within the task");
                    SetError(exception);

                    ExceptionDispatchInfo.Capture(exception).Throw();

                    //this is not reached
                    return default;
                }

                else return _t.Result;
            });

            _taskAwaiter = new AsyncAwaiter<TResult>(this._task);
        }
        
        public override bool? Succeeded
        {
            get
            {
                switch (_task.Status)
                {
                    case TaskStatus.RanToCompletion: return true;

                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled: return false;

                    default: return null;
                }
            }
        }

        public override OperationError Error => _error;
        
        public override IAwaiter<TResult> GetAwaiter() => _taskAwaiter;

        internal Task<TResult> GetTask() => _task;

        private void SetError(Exception e)
        {
            if (_error == null)
            {
                switch (e)
                {
                    case OperationException oe:
                        _error = oe.Error;
                        break;

                    default:
                        _error = new OperationError(
                            message: e.Message,
                            code: "GeneralError",
                            exception: e);
                        break;
                }
            }
        }

        #region Resolvable

        Task<TResult> IResolvable<Task<TResult>>.Resolve() => _task;

        bool IResolvable<Task<TResult>>.TryResolve(out Task<TResult> result, out OperationError error)
        {
            error = null;
            result = _task;
            return true;
        }
        #endregion

        #region Mappers

        #region Success mappers
        public override Operation Then(Action<TResult> action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                var result = await this;
                action.Invoke(result);
            });
        }

        public override Operation Then(Func<TResult, Task> action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                var result = await this;
                await action.Invoke(result);
            });
        }

        public override Operation Then(Func<TResult, Operation> action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                var result = await this;
                await action.Invoke(result);
            });
        }

        public override Operation<TOut> Then<TOut>(Func<TResult, TOut> action)
        {
            if (TryInvalidateArguments<TResult, TOut>(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                var result = await this;
                return action.Invoke(result);
            });
        }

        public override Operation<TOut> Then<TOut>(Func<TResult, Task<TOut>> action)
        {
            if (TryInvalidateArguments<TResult, TOut>(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                var result = await this;
                return await action.Invoke(result);
            });
        }

        public override Operation<TOut> Then<TOut>(Func<TResult, Operation<TOut>> action)
        {
            if (TryInvalidateArguments<TResult, TOut>(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                var result = await this;
                return await action.Invoke(result);
            });
        }
        #endregion

        #region Failure mappers
        public override Operation MapError(Action<OperationError> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    _ = await this;
                }
                catch
                {
                    failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation MapError(Func<OperationError, Task> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    _ = await this;
                }
                catch
                {
                    await failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation MapError(Func<OperationError, Operation> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    _ = await this;
                }
                catch
                {
                    await failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, TOut> failureHandler)
        {
            if (TryInvalidateArguments<TResult, TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    var result = await this;

                    if (result is TOut @out)
                        return @out;

                    else
                        return default;
                }
                catch
                {
                    return failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, Task<TOut>> failureHandler)
        {
            if (TryInvalidateArguments<TResult, TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    var result = await this;

                    if (result is TOut @out)
                        return @out;

                    else
                        return default;
                }
                catch
                {
                    return await failureHandler.Invoke(this.Error);
                }
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, Operation<TOut>> failureHandler)
        {
            if (TryInvalidateArguments<TResult, TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                try
                {
                    var result = await this;

                    if (result is TOut @out)
                        return @out;

                    else
                        return default;
                }
                catch
                {
                    return await failureHandler.Invoke(this.Error);
                }
            });
        }
        #endregion

        private static bool TryInvalidateArguments<TIn>(
            Operation<TIn> prev,
            object next,
            out Operation @out,
            bool invalidTryState = false)
        {
            if (prev == null)
                @out = Operation.Fail(new ArgumentNullException("Previous operation cannot be null"));

            else if (next == null)
                @out = Operation.Fail(new ArgumentNullException("Next action cannot be null"));

            else if (invalidTryState == false && prev.Succeeded == invalidTryState)
                @out = Operation.Fail(prev.Error);

            else if (invalidTryState == true && prev.Succeeded == invalidTryState)
                @out = Operation.FromVoid();

            else
            {
                @out = null;
                return false;
            }

            return true;
        }

        private static bool TryInvalidateArguments<TIn, TOut>(
            Operation<TIn> prev,
            object next,
            out Operation<TOut> @out,
            bool invalidTryState = false)
        {
            if (prev == null)
                @out = Operation.Fail<TOut>(new ArgumentNullException("Previous operation cannot be null"));

            else if (next == null)
                @out = Operation.Fail<TOut>(new ArgumentNullException("Next action cannot be null"));

            else if (invalidTryState == false && prev.Succeeded == invalidTryState)
                @out = Operation.Fail<TOut>(prev.Error);

            else if (invalidTryState == true && prev.Succeeded == invalidTryState)
                @out = Operation.FromResult<TOut>(default);

            else
            {
                @out = null;
                return false;
            }

            return true;
        }

        #endregion

        public static implicit operator AsyncOperation<TResult>(Func<Task<TResult>> func) => new AsyncOperation<TResult>(func);

        public static implicit operator AsyncOperation<TResult>(Task<TResult> task) => new AsyncOperation<TResult>(task);
    }
}
