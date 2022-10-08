using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation
{
    /// <summary>
    /// Enables railway programming.
    /// <para>
    /// An operation encapsulates the execution of logic that may have one of 3 outcomes:
    /// <list type="number">
    /// <item>Success: the execution ran to completion</item>
    /// <item>Busy: the execution has not completed running</item>
    /// <item>Failure: the execution threw an exception, which is captured by the operation instance.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Operations are awaitable, and support continuations. Much like <c>Promise</c>, or <c>Future</c>, 
    /// the true status of an operation cannot be known until it is either awaited, or resolved.
    /// </para>
    /// </summary>
    public interface IOperation: IAwaitable
    {
        /// <summary>
        /// Status of the operation.
        /// <list type="bullet">
        /// <item>true: operation succeeded</item>
        /// <item>false: operation failed</item>
        /// <item>null: operation is not concluded (may or may not have started execution)</item>
        /// </list>
        /// </summary>
        bool? Succeeded { get; }

        /// <summary>
        /// If the operation failed, returns the exception; null otherwise.
        /// </summary>
        OperationError Error { get; }

        #region Mappers

        #region Success mappers
        IOperation Then(Action action);
        IOperation Then(Func<Task> action);
        IOperation Then(Func<IOperation> action);
        IOperation<TOut> Then<TOut>(Func<TOut> action);
        IOperation<TOut> Then<TOut>(Func<Task<TOut>> action);
        IOperation<TOut> Then<TOut>(Func<IOperation<TOut>> action);
        #endregion

        #region Failure mappers
        IOperation MapError(Action<OperationError> failureHandler);
        IOperation MapError(Func<OperationError, Task> failureHandler);
        IOperation MapError(Func<OperationError, IOperation> failureHandler);
        #endregion

        #endregion
    }

    /// <summary>
    /// Enables railway programming.
    /// <para>
    /// An operation encapsulates the execution of logic that may have one of 3 outcomes:
    /// <list type="number">
    /// <item>Success: the execution ran to completion and returned a result, which is captured by the operation</item>
    /// <item>Busy: the execution has not completed running</item>
    /// <item>Failure: the execution threw an exception, which is captured by the operation instance.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Operations are awaitable, and support continuations. Much like <c>Promise</c>, or <c>Future</c>, 
    /// the true status of an operation cannot be known until it is either awaited, or resolved.
    /// </para>
    /// </summary>
    public interface IOperation<out TResult>: IAwaitable<TResult>
    {
        /// <summary>
        /// Status of the operation.
        /// <list type="bullet">
        /// <item>true: operation succeeded</item>
        /// <item>false: operation failed</item>
        /// <item>null: operation is not concluded (may or may not have started execution)</item>
        /// </list>
        /// </summary>
        bool? Succeeded { get; }

        /// <summary>
        /// If the operation failed, returns the exception; null otherwise.
        /// </summary>
        OperationError Error { get; }

        #region Mappers

        #region Success mappers
        IOperation Then(Action<TResult> action);
        IOperation Then(Func<TResult, Task> action);
        IOperation Then(Func<TResult, IOperation> action);
        IOperation<TOut> Then<TOut>(Func<TResult, TOut> action);
        IOperation<TOut> Then<TOut>(Func<TResult, Task<TOut>> action);
        IOperation<TOut> Then<TOut>(Func<TResult, IOperation<TOut>> action);
        #endregion

        #region Failure mappers
        // implemented as Extension methods because covariance will not allow any signature where TResult is passed IN as an argument to a non-generic method.
        // e.g public static IOperation<TResult> MapError(Func<OperationError, TResult> failureHandler)
        /// <see cref="OperationExtensions"/>
        #endregion

        #endregion
    }
}
