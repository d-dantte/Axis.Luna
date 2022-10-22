using Axis.Luna.Extensions;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Value
{
    /// <summary>
    /// An operation that is always faulted.
    /// NOTE: This should be a readonly struct
    /// </summary>
    public class FaultedOperation : IOperation, IResolvable
    {
        private readonly OperationError _error;
        private readonly ValueAwaiter _awaiter;

        // <inheritdoc/>
        public bool? Succeeded => false;

        // <inheritdoc/>
        public OperationError Error => _error;

        internal FaultedOperation(OperationError error)
        {
            _error = error ?? throw new ArgumentNullException();
            _awaiter = new ValueAwaiter(_error.GetException());
        }

        // <inheritdoc/>
        public IAwaiter GetAwaiter() => _awaiter;

        #region Resolvable

        // <inheritdoc/>
        void IResolvable.Resolve() => _awaiter.GetResult();

        // <inheritdoc/>
        bool IResolvable.TryResolve(out OperationError error)
        {
            error = Error;
            return false;
        }
        #endregion

        #region Mappers

        #region Success mappers
        public IOperation Then(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return this;
        }

        public IOperation Then(Func<Task> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            return this;
        }

        public IOperation Then(Func<IOperation> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            return this;
        }

        public IOperation<TOut> Then<TOut>(Func<TOut> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return Operation.Fail<TOut>(Error);
        }

        public IOperation<TOut> Then<TOut>(Func<Task<TOut>> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            return Operation.Fail<TOut>(Error);
        }

        public IOperation<TOut> Then<TOut>(Func<IOperation<TOut>> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            return Operation.Fail<TOut>(Error);
        }
        #endregion

        #region Failure mappers
        public IOperation MapError(Action<OperationError> failureHandler)
        {
            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            else return Operation.Try(() =>
            {
                failureHandler.Invoke(this.Error);
            });
        }

        public IOperation MapError(Func<OperationError, Task> failureHandler)
        {
            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            else return Operation.Try(async () =>
            {
                await failureHandler.Invoke(this.Error);
            });
        }

        public IOperation MapError(Func<OperationError, IOperation> failureHandler)
        {
            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            else return Operation.Try(async () =>
            {
                await failureHandler.Invoke(this.Error);
            });
        }
        #endregion

        #endregion

        public static implicit operator FaultedOperation(OperationError error) => new FaultedOperation(error);
    }


    /// <summary>
    /// NOTE: This should be a readonly struct
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class ValueOperation<TResult> : IOperation<TResult>, IResolvable<TResult>
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

        public bool? Succeeded => _awaiter.IsSuccessful;

        public OperationError Error => _error;

        public IAwaiter<TResult> GetAwaiter() => _awaiter;

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

        #region Success mappers
        public IOperation Then(Action<TResult> action)
        {
            if(action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail(Error);

            else return Operation.Try(() =>
            {
                //this is safe from in a value operation.
                var result = this.As<IResolvable<TResult>>().Resolve();
                action.Invoke(result);
            });
        }

        public IOperation Then(Func<TResult, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail(Error);

            else return Operation.Try(async () =>
            {
                var result = await this;
                await action.Invoke(result);
            });
        }

        public IOperation Then(Func<TResult, IOperation> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail(Error);

            else return Operation.Try(async () =>
            {
                var result = await this;
                await action.Invoke(result);
            });
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, TOut> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            else return Operation.Try(() =>
            {
                //this is safe in a value operatoin.
                var result = this.As<IResolvable<TResult>>().Resolve();
                return action.Invoke(result);
            });
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, Task<TOut>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            else return Operation.Try(async () =>
            {
                var result = await this;
                return await action.Invoke(result);
            });
        }

        public IOperation<TOut> Then<TOut>(Func<TResult, IOperation<TOut>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Succeeded == false)
                return Operation.Fail<TOut>(Error);

            else return Operation.Try(async () =>
            {
                var result = await this;
                return await action.Invoke(result);
            });
        }
        #endregion

        public static implicit operator ValueOperation<TResult>(TResult value) => new ValueOperation<TResult>(value);

        public static implicit operator ValueOperation<TResult>(OperationError error) => new ValueOperation<TResult>(error);
    }
}
