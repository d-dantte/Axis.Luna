using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation
{
    /// <summary>
    /// A construct that enables railway - style coding using delegates. It is also awaitable
    /// </summary>
    /// <typeparam name="R"></typeparam>
    public abstract class Operation<R> : IAwaitable<R>
    {
        public abstract R Resolve();

        public abstract IAwaiter<R> GetAwaiter();

        public abstract bool? Succeeded { get; }

        public abstract OperationError Error { get; }

        /// <summary>
        /// Safely Resovle the operation and return it's result, returning a default value if the operation was faulted
        /// </summary>
        public R Result
        {
            get
            {
                try
                {
                    return Resolve();
                }
                catch
                {
                    return default(R);
                }
            }
        }
    }

    /// <summary>
    /// A construct that enables railway - style coding using delegates. It is also awaitable
    /// </summary>
    /// <typeparam name="R"></typeparam>
    public abstract class Operation : IAwaitable
    {
        public abstract void Resolve();

        public abstract IAwaiter GetAwaiter();

        public abstract bool? Succeeded { get; }

        public abstract OperationError Error { get; }


        #region Static helpers

        #region Value
        public static Operation<Result> FromResult<Result>(Result result) => new Sync.SyncOperation<Result>(result);
        #endregion

        #region Try
        public static Operation<Result> Try<Result>(Func<Result> func) => new Lazy.LazyOperation<Result>(func);
        public static Operation Try(Action action) => new Lazy.LazyOperation(action);


        public static Operation<Result> Try<Result>(Func<Task<Result>> func) => new Async.AsyncOperation<Result>(func);
        public static Operation Try(Func<Task> action) => new Async.AsyncOperation(action);

        public static Operation<Result> Try<Result>(Task<Result> task) => new Async.AsyncOperation<Result>(task);
        public static Operation Try(Task task) => new Async.AsyncOperation(task);


        public static Operation<Result> Try<Result>(Func<Operation<Result>> op) => op.Invoke();
        public static Operation Try(Func<Operation> op) => op.Invoke();
        #endregion

        #region Fail
        public static Operation Fail(Exception exception = null) => new Sync.SyncOperation(
            new OperationError(
                code: "GeneralError",
                message: exception?.Message,
                exception: exception));

        public static Operation<Result> Fail<Result>(Exception exception = null) => new Sync.SyncOperation<Result>(
            new OperationError(
                code: "GeneralError",
                message: exception?.Message,
                exception: exception));

        public static Operation Fail(string message) => Fail(new Exception(message));

        public static Operation<Result> Fail<Result>(string message) => Fail<Result>(new Exception(message));
        #endregion

        #endregion
    }
}
