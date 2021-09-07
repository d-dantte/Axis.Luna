using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Value
{
    /// <summary>
    /// 
    /// </summary>
    public class ValueOperation : Operation, IResolvable
    {
        private readonly OperationError _error;
        private readonly ValueAwaiter _awaiter;


        public override bool? Succeeded => _awaiter.IsSuccessful;

        public override OperationError Error => _error;

        internal ValueOperation(OperationError error)
        {
            _error = error ?? throw new ArgumentNullException();
            _awaiter = new ValueAwaiter(_error.GetException());
        }

        public override IAwaiter GetAwaiter() => _awaiter;

        #region Resolvable
        void IResolvable.Resolve() => _awaiter.GetResult();

        bool IResolvable.TryResolve(out OperationError error)
        {
            error = Error;
            return false;
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
                action.Invoke();
            });
        }

        public override Operation Then(Func<Task> action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                await action.Invoke();
            });
        }

        public override Operation Then(Func<Operation> action)
        {
            if (TryInvalidateArguments(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                await action.Invoke();
            });
        }

        public override Operation<TOut> Then<TOut>(Func<TOut> action)
        {
            if (TryInvalidateArguments<TOut>(this, action, out var next))
                return next;

            else return Operation.Try(() =>
            {
                return action.Invoke();
            });
        }

        public override Operation<TOut> Then<TOut>(Func<Task<TOut>> action)
        {
            if (TryInvalidateArguments<TOut>(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                return await action.Invoke();
            });
        }

        public override Operation<TOut> Then<TOut>(Func<Operation<TOut>> action)
        {
            if (TryInvalidateArguments<TOut>(this, action, out var next))
                return next;

            else return Operation.Try(async () =>
            {
                return await action.Invoke();
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
                failureHandler.Invoke(this.Error);
            });
        }

        public override Operation MapError(Func<OperationError, Task> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                await failureHandler.Invoke(this.Error);
            });
        }

        public override Operation MapError(Func<OperationError, Operation> failureHandler)
        {
            if (TryInvalidateArguments(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                await failureHandler.Invoke(this.Error);
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, TOut> failureHandler)
        {
            if (TryInvalidateArguments<TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(() =>
            {
                return failureHandler.Invoke(this.Error);
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, Task<TOut>> failureHandler)
        {
            if (TryInvalidateArguments<TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                return await failureHandler.Invoke(this.Error);
            });
        }

        public override Operation<TOut> MapError<TOut>(Func<OperationError, Operation<TOut>> failureHandler)
        {
            if (TryInvalidateArguments<TOut>(this, failureHandler, out var next, true))
                return next;

            else return Operation.Try(async () =>
            {
                return await failureHandler.Invoke(this.Error);
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

        public static implicit operator ValueOperation(OperationError error) => new ValueOperation(error);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class ValueOperation<TResult> : Operation<TResult>, IResolvable<TResult>
    {
        private readonly OperationError _error;
        private readonly ValueAwaiter<TResult> _awaiter;

        internal ValueOperation(TResult value)
        {
            _error = null;
            _awaiter = new ValueAwaiter<TResult>(value);
        }

        internal ValueOperation(OperationError error)
        {
            _error = error ?? throw new ArgumentNullException(nameof(error));
            _awaiter = new ValueAwaiter<TResult>(_error.GetException());
        }

        public override bool? Succeeded => _awaiter.IsSuccessful;

        public override OperationError Error => _error;

        public override IAwaiter<TResult> GetAwaiter() => _awaiter;

        #region Resolvable
        TResult IResolvable<TResult>.Resolve() => _awaiter.GetResult();

        bool IResolvable<TResult>.TryResolve(out TResult result, out OperationError error)
        {
            if (Succeeded == true)
            {
                result = _awaiter.GetResult();
                error = default;
                return true;
            }
            else // Succeeded = false
            {
                result = default;
                error = Error;
                return false;
            }
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

        public static implicit operator ValueOperation<TResult>(TResult value) => new ValueOperation<TResult>(value);

        public static implicit operator ValueOperation<TResult>(OperationError error) => new ValueOperation<TResult>(error);

        public static implicit operator ValueOperation<TResult>(Exception exception) => new ValueOperation<TResult>(exception);
    }
}
