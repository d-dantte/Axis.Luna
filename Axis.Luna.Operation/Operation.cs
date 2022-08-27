using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Axis.Luna.Operation.Test")]

namespace Axis.Luna.Operation
{
    /// <summary>
    /// A construct that enables railway - style coding using delegates. It is also awaitable
    /// </summary>
    /// <typeparam name="R"></typeparam>
    public abstract class Operation : IAwaitable //, IOperation
    {
        public abstract IAwaiter GetAwaiter();

        public abstract bool? Succeeded { get; }

        public abstract OperationError Error { get; }


        #region Mappers

        #region Success mappers
        public abstract Operation Then(Action action);
        public abstract Operation Then(Func<Task> action);
        public abstract Operation Then(Func<Operation> action);
        public abstract Operation<TOut> Then<TOut>(Func<TOut> action);
        public abstract Operation<TOut> Then<TOut>(Func<Task<TOut>> action);
        public abstract Operation<TOut> Then<TOut>(Func<Operation<TOut>> action);
        #endregion

        #region Failure mappers
        public abstract Operation MapError(Action<OperationError> failureHandler);
        public abstract Operation MapError(Func<OperationError, Task> failureHandler);
        public abstract Operation MapError(Func<OperationError, Operation> failureHandler);
        public abstract Operation<TOut> MapError<TOut>(Func<OperationError, TOut> failureHandler);
        public abstract Operation<TOut> MapError<TOut>(Func<OperationError, Task<TOut>> failureHandler);
        public abstract Operation<TOut> MapError<TOut>(Func<OperationError, Operation<TOut>> failureHandler);
        #endregion

        #endregion


        #region Static helpers

        #region Value
        /// <summary>
        /// Returns an already resolved operation with a value ready for returning
        /// </summary>
        /// <typeparam name="Result">type of the result</typeparam>
        /// <param name="result">result value</param>
        /// <returns>Operation of result</returns>
        public static Operation<Result> FromResult<Result>(Result result) => new Value.ValueOperation<Result>(result);

        /// <summary>
        /// Creates a no-op operation that may or may not have been resolved, but will be successful when resolved.
        /// </summary>
        /// <returns>A no-op operation</returns>
        public static Operation FromVoid() => new Lazy.LazyOperation(() => { }); //<-- is there a better way to do this?
        #endregion

        #region Try
        public static Operation<Result> Try<Result>(Func<Result> func) => new Lazy.LazyOperation<Result>(func);
        public static Operation Try(Action action) => new Lazy.LazyOperation(action);


        public static Operation<Result> Try<Result>(Func<Task<Result>> func) => new Async.AsyncOperation<Result>(func);
        public static Operation Try(Func<Task> action) => new Async.AsyncOperation(action);

        public static Operation<Result> Try<Result>(Task<Result> task) => new Async.AsyncOperation<Result>(task);
        public static Operation Try(Task task) => new Async.AsyncOperation(task);


        public static Operation<Result> Try<Result>(Func<Operation<Result>> op)
        {
            try
            {
                return op.Invoke();
            }
            catch(Exception e)
            {
                return Fail<Result>(e);
            }
        }
        public static Operation Try(Func<Operation> op)
        {
            try
            {
                return op.Invoke();
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }
        #endregion

        #region Fail
        public static Operation Fail(Exception exception = null) 
            => new Value.ValueOperation(
                new OperationError(
                    code: "GeneralError",
                    message: exception?.Message,
                    exception: exception));

        public static Operation<Result> Fail<Result>(Exception exception = null) 
            => new Value.ValueOperation<Result>(
                new OperationError(
                    code: "GeneralError",
                    message: exception?.Message,
                    exception: exception));


        public static Operation Fail(string message) => Fail(new Exception(message));

        public static Operation<Result> Fail<Result>(string message) => Fail<Result>(new Exception(message));


        public static Operation Fail(OperationError error) 
            => new Value.ValueOperation(error ?? throw new ArgumentNullException(nameof(error)));

        public static Operation<Result> Fail<Result>(OperationError error)
            => new Value.ValueOperation<Result>(error ?? throw new ArgumentNullException(nameof(error)));
        #endregion

        #endregion
    }

    /// <summary>
    /// A construct that enables railway - style coding using delegates. It is also awaitable
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class Operation<TResult> : IAwaitable<TResult> //, IOperation<TResult>
    {
        public abstract IAwaiter<TResult> GetAwaiter();

        public abstract bool? Succeeded { get; }

        public abstract OperationError Error { get; }

        #region Mappers

        #region Success mappers
        public abstract Operation Then(Action<TResult> action);
        public abstract Operation Then(Func<TResult, Task> action);
        public abstract Operation Then(Func<TResult, Operation> action);
        public abstract Operation<TOut> Then<TOut>(Func<TResult, TOut> action);
        public abstract Operation<TOut> Then<TOut>(Func<TResult, Task<TOut>> action);
        public abstract Operation<TOut> Then<TOut>(Func<TResult, Operation<TOut>> action);
        #endregion

        #region Failure mappers
        public abstract Operation MapError(Action<OperationError> failureHandler);
        public abstract Operation MapError(Func<OperationError, Task> failureHandler);
        public abstract Operation MapError(Func<OperationError, Operation> failureHandler);
        public abstract Operation<TOut> MapError<TOut>(Func<OperationError, TOut> failureHandler);
        public abstract Operation<TOut> MapError<TOut>(Func<OperationError, Task<TOut>> failureHandler);
        public abstract Operation<TOut> MapError<TOut>(Func<OperationError, Operation<TOut>> failureHandler);
        #endregion

        #endregion
    }
}
