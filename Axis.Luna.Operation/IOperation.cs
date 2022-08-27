using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation
{
    /// <summary>
    /// Use interfaces as the base for operations, to support covariance. Is it worth it though?
    /// </summary>
    public interface IOperation
    {
        IAwaiter GetAwaiter();

        bool? Succeeded { get; }

        OperationError Error { get; }

        #region Mappers

        #region Success mappers
        IOperation Then(Action action);
        IOperation Then(Func<Task> action);
        IOperation Then(Func<Operation> action);
        IOperation<TOut> Then<TOut>(Func<TOut> action);
        IOperation<TOut> Then<TOut>(Func<Task<TOut>> action);
        IOperation<TOut> Then<TOut>(Func<Operation<TOut>> action);
        #endregion

        #region Failure mappers
        IOperation MapError(Action<OperationError> failureHandler);
        IOperation MapError(Func<OperationError, Task> failureHandler);
        IOperation MapError(Func<OperationError, Operation> failureHandler);
        IOperation<TOut> MapError<TOut>(Func<OperationError, TOut> failureHandler);
        IOperation<TOut> MapError<TOut>(Func<OperationError, Task<TOut>> failureHandler);
        IOperation<TOut> MapError<TOut>(Func<OperationError, Operation<TOut>> failureHandler);
        #endregion

        #endregion
    }

    /// <summary>
    /// Use interfaces as the base for operations, to support covariance. Is it worth it though?
    /// </summary>
    public interface IOperation<out TResult>
    {
        IAwaiter<TResult> GetAwaiter();

        bool? Succeeded { get; }

        OperationError Error { get; }

        #region Mappers

        #region Success mappers
        IOperation Then(Action<TResult> action);
        IOperation Then(Func<TResult, Task> action);
        IOperation Then(Func<TResult, Operation> action);
        IOperation<TOut> Then<TOut>(Func<TResult, TOut> action);
        IOperation<TOut> Then<TOut>(Func<TResult, Task<TOut>> action);
        IOperation<TOut> Then<TOut>(Func<TResult, Operation<TOut>> action);
        #endregion

        #region Failure mappers
        IOperation MapError(Action<OperationError> failureHandler);
        IOperation MapError(Func<OperationError, Task> failureHandler);
        IOperation MapError(Func<OperationError, Operation> failureHandler);
        IOperation<TOut> MapError<TOut>(Func<OperationError, TOut> failureHandler);
        IOperation<TOut> MapError<TOut>(Func<OperationError, Task<TOut>> failureHandler);
        IOperation<TOut> MapError<TOut>(Func<OperationError, Operation<TOut>> failureHandler);
        #endregion

        #endregion
    }
}
