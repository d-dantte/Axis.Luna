using Axis.Luna.Extensions;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Lazy
{
    /// <summary>
    /// 
    /// </summary>
    public class LazyOperation : IOperation, IResolvable
    {
        private OperationError _error;
        private readonly LazyAwaiter _awaiter;

        internal LazyOperation(Action action)
        {
            if (action == null) 
                throw new ArgumentNullException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter(
                errorSetter: SetError,
                lazy: new Lazy<object>(
                    isThreadSafe: true,
                    valueFactory: () =>
                    {
                        action.Invoke();
                        return true;
                    }));
        }

        // <inheritdoc/>
        public bool? Succeeded => _awaiter.IsSuccessful;

        // <inheritdoc/>
        public OperationError Error => _error;

        // <inheritdoc/>
        public IAwaiter GetAwaiter() => _awaiter;

        #region Resolvable
        // <inheritdoc/>
        void IResolvable.Resolve()
        {
            if (Succeeded == true)
                return;

            else
            {
                _awaiter.GetResult();
            }
        }

        // <inheritdoc/>
        bool IResolvable.TryResolve(out OperationError error)
        {
            if (Succeeded == null)
            {
                try
                {
                    this.As<IResolvable>().Resolve();
                    error = null;
                    return true;
                }
                catch
                {
                    error = _error;
                    return false;
                }
            }
            else
            {
                error = _error;
                return Succeeded.Value;
            }
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
            return Operation.Try(() =>
            {
                //this is safe from a lazy operation, and keeps us in the lazy operation domain.
                this.As<IResolvable>().Resolve();
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
                this.As<IResolvable>().Resolve();
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
                this.As<IResolvable>().Resolve();
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
            return Operation.Try(() =>
            {
                this.As<IResolvable>().Resolve();
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
                return Operation.Fail<TOut>(Error);

            //Succeeded == null
            return Operation.Try(async () =>
            {
                this.As<IResolvable>().Resolve();
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
                this.As<IResolvable>().Resolve();
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
            return Operation.Try(() =>
            {
                try
                {
                    this.As<IResolvable>().Resolve();
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
                    this.As<IResolvable>().Resolve();
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

        private void SetError(Exception e)
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

        public static implicit operator LazyOperation(Action action) => new LazyOperation(action);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class LazyOperation<TResult> : IOperation<TResult>, IResolvable<TResult>
    {
        private OperationError _error;
        private readonly LazyAwaiter<TResult> _awaiter;

        internal LazyOperation(Func<TResult> func)
        {
            _awaiter = new LazyAwaiter<TResult>(
                errorSetter: SetError,
                lazy: new Lazy<TResult>(
                    func ?? throw new ArgumentNullException("Invalid delegate supplied"),
                    true));
        }

        internal LazyOperation(Lazy<TResult> lazy)
        {
            _awaiter = new LazyAwaiter<TResult>(
                lazy ?? throw new ArgumentNullException("Invalid Lazy factory supplied"),
                SetError);
        }

        // <inheritdoc/>
        public bool? Succeeded => _awaiter.IsSuccessful;

        // <inheritdoc/>
        public OperationError Error => _error;

        // <inheritdoc/>
        public IAwaiter<TResult> GetAwaiter() => _awaiter;

        #region Resolvable
        // <inheritdoc/>
        TResult IResolvable<TResult>.Resolve() => _awaiter.GetResult();

        // <inheritdoc/>
        bool IResolvable<TResult>.TryResolve(out TResult result, out OperationError error)
        {
            if (Succeeded == null)
            {
                try
                {
                    result = this.As<IResolvable<TResult>>().Resolve();
                    error = null;
                    return true;
                }
                catch
                {
                    result = default;
                    error = _error;
                    return false;
                }
            }
            else
            {
                error = _error;
                result = Succeeded == true ? this.As<IResolvable<TResult>>().Resolve() : default;
                return Succeeded.Value;
            }
        }
        #endregion

        #region Mappers

        #region Success mappers
        public IOperation Then(Action<TResult> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(() => action.Invoke(this.As<IResolvable<TResult>>().Resolve()));

            if (Succeeded == false)
                return Operation.Fail(Error);

            //Succeeded == null
            return Operation.Try(() =>
            {
                var result = this.As<IResolvable<TResult>>().Resolve();
                action.Invoke(result);
            });
        }

        public IOperation Then(Func<TResult, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(() => action.Invoke(this.As<IResolvable<TResult>>().Resolve()));

            if (Succeeded == false)
                return Operation.Fail(Error);

            //Succeeded == null
            return Operation.Try(() =>
            {
                var result = this.As<IResolvable<TResult>>().Resolve();
                return action.Invoke(result);
            });
        }

        public IOperation Then(Func<TResult, IOperation> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(() => action.Invoke(this.As<IResolvable<TResult>>().Resolve()));

            if (Succeeded == false)
                return Operation.Fail(Error);

            //Succeeded == null
            return Operation.Try(() =>
            {
                var result = this.As<IResolvable<TResult>>().Resolve();
                return action.Invoke(result);
            });
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, TOut> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(() => action.Invoke(this.As<IResolvable<TResult>>().Resolve()));

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            //Succeeded == null
            return Operation.Try(() =>
            {
                var result = this.As<IResolvable<TResult>>().Resolve();
                return action.Invoke(result);
            });
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, Task<TOut>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(() => action.Invoke(this.As<IResolvable<TResult>>().Resolve()));

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            //Succeeded == null
            return Operation.Try(() =>
            {
                var result = this.As<IResolvable<TResult>>().Resolve();
                return action.Invoke(result);
            });
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, IOperation<TOut>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == true)
                return Operation.Try(() => action.Invoke(this.As<IResolvable<TResult>>().Resolve()));

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            //Succeeded == null
            return Operation.Try(() =>
            {
                var result = this.As<IResolvable<TResult>>().Resolve();
                return action.Invoke(result);
            });
        }
        #endregion

        #endregion

        private void SetError(Exception e)
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

		public static implicit operator LazyOperation<TResult>(Func<TResult> func) => new LazyOperation<TResult>(func);

        public static implicit operator LazyOperation<TResult>(Lazy<TResult> lazy) => new LazyOperation<TResult>(lazy);
    }

}
