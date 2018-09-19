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
        public static Operation<Result> Try<Result>(Func<Result> func, Func<Task> rollBack = null) => new Lazy.LazyOperation<Result>(func, rollBack);
        public static Operation Try(Action action, Func<Task> rollBack = null) => new Lazy.LazyOperation(action, rollBack);


        public static Operation<Result> Try<Result>(Func<Task<Result>> func, Func<Task> rollBack = null) => new Async.AsyncOperation<Result>(func, rollBack);
        public static Operation Try(Func<Task> action, Func<Task> rollBack = null) => new Async.AsyncOperation(action, rollBack);

        public static Operation<Result> Try<Result>(Task<Result> task, Func<Task> rollBack = null) => new Async.AsyncOperation<Result>(task, rollBack);
        public static Operation Try(Task task, Func<Task> rollBack = null) => new Async.AsyncOperation(task, rollBack);


        public static Operation<Result> Try<Result>(Func<Operation<Result>> op) => op.Invoke();
        public static Operation Try(Func<Operation> op) => op.Invoke();
        #endregion

        #region Fail
        public static Operation Fail(Exception exception = null) => new Sync.SyncOperation(new OperationError(exception)
        {
            Code = "GeneralError",
            Message = exception?.Message
        });

        public static Operation<Result> Fail<Result>(Exception exception = null) => new Sync.SyncOperation<Result>(new OperationError(exception)
        {
            Code = "GeneralError",
            Message = exception?.Message
        });

        public static Operation Fail(string message) => Fail(new Exception(message));

        public static Operation<Result> Fail<Result>(string message) => Fail<Result>(new Exception(message));
        #endregion

        #endregion
    }
}
