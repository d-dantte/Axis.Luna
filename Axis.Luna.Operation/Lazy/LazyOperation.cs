using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Lazy
{
    /// <summary>
    /// 
    /// </summary>
    public class LazyOperation : Operation, IResolvable
    {
        private OperationError _error;
        private readonly LazyAwaiter _awaiter;

        internal LazyOperation(Action action)
        {
            if (action == null) throw new NullReferenceException("Invalid delegate supplied");

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

        public override bool? Succeeded => _awaiter.IsSuccessful;

        public override OperationError Error => _error;

        public override IAwaiter GetAwaiter() => _awaiter;

        #region Resolvable
        void IResolvable.Resolve()
        {
            if (Succeeded == true)
                return;

            else
            {
                _awaiter.GetResult();
            }
        }

        bool IResolvable.TryResolve(out OperationError error)
        {
            if (Succeeded == null)
            {
                try
                {
                    (this as IResolvable).Resolve();

                    error = null;
                    return true;
                }
                catch
                {
                    error = _error;
                    return false;
                }
            }

            error = _error;
            return Succeeded.Value;
        }
        #endregion

        #region Mappers

        #region Success mappers
        public override Operation Then(Action action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(() =>
            {
                //this is safe from a lazy operation, and keeps us in the lazy operation domain.
                (this as IResolvable).Resolve(); 
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

            else return Operation.Try(() =>
            {
                //this is safe from a lazy operation, and keeps us in the lazy operatio domain.
                (this as IResolvable).Resolve();
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
                return await  action.Invoke();
            });
        }
        #endregion

        #region Failure mappers
        public override Operation MapError(Action<OperationError> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(() =>
            {
                try
                {
                    //this is safe from a lazy operation, and keeps us in the lazy operation domain.
                    (this as IResolvable).Resolve();
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

            else return Operation.Try(() =>
            {
                try
                {
                    //this is safe from a lazy operation, and keeps us in the lazy operation domain.
                    (this as IResolvable).Resolve();
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

        private void SetError(Exception e)
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

        public static implicit operator LazyOperation(Action action) => new LazyOperation(action);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class LazyOperation<TResult> : Operation<TResult>, IResolvable<TResult>
    {
        private OperationError _error;
        private readonly LazyAwaiter<TResult> _awaiter;

        internal LazyOperation(Func<TResult> func)
        {
            if (func == null) throw new NullReferenceException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter<TResult>(
                new Lazy<TResult>(func, true),
                SetError);
        }

        internal LazyOperation(Lazy<TResult> lazy)
        {
            _awaiter = new LazyAwaiter<TResult>(
                lazy ?? throw new NullReferenceException("Invalid Lazy factory supplied"),
                SetError);
        }

        public override bool? Succeeded => _awaiter.IsSuccessful;


        public override IAwaiter<TResult> GetAwaiter() => _awaiter;

        public override OperationError Error => _error;

		#region Resolvable
		TResult IResolvable<TResult>.Resolve() => _awaiter.GetResult();

        bool IResolvable<TResult>.TryResolve(out TResult result, out OperationError error)
        {
            if (Succeeded == null)
            {
                try
                {
                    result = (this as IResolvable<TResult>).Resolve();

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

            result = default;
            error = _error;
            return Succeeded.Value;
        }
        #endregion

        #region Mappers

        #region Success mappers
        public override Operation Then(Action<TResult> action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(() =>
            {
                //this is safe from a lazy operation, and keeps us in the lazy operation domain.
                var result = (this as IResolvable<TResult>).Resolve();
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

            else return Operation.Try(() =>
            {
                //this is safe from a lazy operation, and keeps us in the lazy operatio domain.
                var result = (this as IResolvable<TResult>).Resolve();
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

            else return Operation.Try(() =>
            {
                try
                {
                    //this is safe from a lazy operation, and keeps us in the lazy operation domain.
                    _ = (this as IResolvable<TResult>).Resolve();
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

            else return Operation.Try(() =>
            {
                try
                {
                    //this is safe from a lazy operation, and keeps us in the lazy operation domain.
                    var result = (this as IResolvable<TResult>).Resolve();

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

        private void SetError(Exception e)
        {
            switch(e)
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

		public static implicit operator LazyOperation<TResult>(Func<TResult> func) => new LazyOperation<TResult>(func);

        public static implicit operator LazyOperation<TResult>(Lazy<TResult> lazy) => new LazyOperation<TResult>(lazy);
    }

}
