using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Async
{
    /// <summary>
    /// Reresents operations for asynchronious executions
    /// </summary>
    public class AsyncOperation : IOperation, IResolvable<Task>
    {
        private OperationError _error;
        private readonly Task _task;
        private readonly AsyncAwaiter _taskAwaiter;

        /// <summary>
        /// Creates a new AsyncOperation.
        /// </summary>
        /// <param name="taskProducer">A delegate that returns the task that this operation encapsulates</param>
        internal AsyncOperation(Func<Task> taskProducer)
        {
            if (taskProducer == null)
                throw new ArgumentNullException(nameof(taskProducer));

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
        /// Creates a new AsyncOperation.        /// 
        /// <para>
        /// NOTE: if the task supplied was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="task">The task that this operation encapsulates</param>
        internal AsyncOperation(Task task)
        {
            if(task == null)
                throw new ArgumentNullException(nameof(task));

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


        public bool? Succeeded
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

        public IAwaiter GetAwaiter() => _taskAwaiter;

        public OperationError Error => _error;

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
        public IOperation Then(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(action);

            if (Succeeded == false)
                return this;

            //Succeeded == null
            return Operation.Try(async () =>
            {
                await this;
                action.Invoke();
            });
        }

        public IOperation Then(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(action);

            if (Succeeded == false)
                return this;

            //Succeeded == null
            return Operation.Try(async () =>
            {
                await this;
                await action.Invoke();
            });
        }

        public IOperation Then(Func<IOperation> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(action);

            if (Succeeded == false)
                return this;

            //Succeeded == null
            return Operation.Try(async () =>
            {
                await this;
                await action.Invoke();
            });
        }

        public IOperation<TOut> Then<TOut>(Func<TOut> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(action);

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            //Succeeded == null
            return Operation.Try(async () =>
            {
                await this;
                return action.Invoke();
            });
        }

        public IOperation<TOut> Then<TOut>(Func<Task<TOut>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(action);

            if (Succeeded == false)
                return Operation.Fail<TOut>();

            //Succeeded == null
            return Operation.Try(async () =>
            {
                await this;
                return await action.Invoke();
            });
        }

        public IOperation<TOut> Then<TOut>(Func<IOperation<TOut>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(action);

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            //Succeeded == null
            return Operation.Try(async () =>
            {
                await this;
                return await action.Invoke();
            });
        }
        #endregion

        #region Failure mappers
        public IOperation MapError(Action<OperationError> failureHandler)
        {
            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            if (Succeeded == true)
                return this;

            if (Succeeded == false)
                return Operation.Try(() => failureHandler.Invoke(Error));

            //Succeeded == null
            return Operation.Try(async () =>
            {
                try
                {
                    await this;
                }
                catch
                {
                    failureHandler.Invoke(Error);
                }
            });
        }

        public IOperation MapError(Func<OperationError, Task> failureHandler)
        {
            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            if (Succeeded == true)
                return this;

            if (Succeeded == false)
                return Operation.Try(() => failureHandler.Invoke(Error));

            //Succeeded == null
            return Operation.Try(async () =>
            {
                try
                {
                    await this;
                }
                catch
                {
                    await failureHandler.Invoke(Error);
                }
            });
        }

        public IOperation MapError(Func<OperationError, IOperation> failureHandler)
        {
            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            if (Succeeded == true)
                return this;

            if (Succeeded == false)
                return Operation.Try(() => failureHandler.Invoke(Error));

            //Succeeded == null
            return Operation.Try(async () =>
            {
                try
                {
                    await this;
                }
                catch
                {
                    await failureHandler.Invoke(Error);
                }
            });
        }
        #endregion

        #endregion


        public static implicit operator AsyncOperation(Func<Task> func) => new AsyncOperation(func);

        public static implicit operator AsyncOperation(Task task) => new AsyncOperation(task);
    }


    /// <summary>
    /// Reresents operations for asynchronious, result producing executions
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class AsyncOperation<TResult> : IOperation<TResult>, IResolvable<Task<TResult>>
    {
        private OperationError _error;
        private readonly Task<TResult> _task;
        private readonly AsyncAwaiter<TResult> _taskAwaiter;

        /// <summary>
        /// Creates a new AsyncOperation.
        /// </summary>
        /// <param name="taskProducer">A delegate that returns the task that this operation encapsulates</param>
        internal AsyncOperation(Func<Task<TResult>> taskProducer)
        {
            if (taskProducer == null)
                throw new ArgumentNullException("Invalid Task Producer Supplied");

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
            var _task = task ?? throw new ArgumentNullException("Invalid task supplied");

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
        
        public bool? Succeeded
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

        public OperationError Error => _error;
        
        public IAwaiter<TResult> GetAwaiter() => _taskAwaiter;

        internal Task<TResult> GetTask() => _task;

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
        public IOperation Then(Action<TResult> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail();

            //Succeeded == true || null
            return Operation.Try(async () => action.Invoke(await this));
        }

        public IOperation Then(Func<TResult, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail(Error);

            //Succeeded == true || null
            return Operation.Try(async () => await action.Invoke(await this));
        }

        public IOperation Then(Func<TResult, IOperation> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail(Error);

            //Succeeded == true || null
            return Operation.Try(async () => await action.Invoke(await this));
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, TOut> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            //Succeeded == true || null
            return Operation.Try(async () => action.Invoke(await this));
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, Task<TOut>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail<TOut>();

            //Succeeded == true || null
            return Operation.Try(async () => await action.Invoke(await this));
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, IOperation<TOut>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail<TOut>();

            //Succeeded == true || null
            return Operation.Try(async () => await action.Invoke(await this));
        }
        #endregion

        #endregion

        public static implicit operator AsyncOperation<TResult>(Func<Task<TResult>> func) => new AsyncOperation<TResult>(func);

        public static implicit operator AsyncOperation<TResult>(Task<TResult> task) => new AsyncOperation<TResult>(task);
    }
}
