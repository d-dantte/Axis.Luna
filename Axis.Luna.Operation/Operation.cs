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
        public abstract IAwaiter<R> GetAwaiter();

        public abstract bool? Succeeded { get; }

        public abstract OperationError Error { get; }
    }

    /// <summary>
    /// A construct that enables railway - style coding using delegates. It is also awaitable
    /// </summary>
    /// <typeparam name="R"></typeparam>
    public abstract class Operation : IAwaitable
    {
        public abstract IAwaiter GetAwaiter();

        public abstract bool? Succeeded { get; }

        public abstract OperationError Error { get; }


        #region Static helpers

        #region Value
        public static Operation<Result> FromResult<Result>(Result result) => new Sync.SyncOperation<Result>(result);

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
                return Operation.Fail<Result>(e);
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
                return Operation.Fail(e);
            }
        }
        #endregion

        #region Fail
        public static Operation Fail(Exception exception = null) 
            => new Sync.SyncOperation(
                new OperationError(
                    code: "GeneralError",
                    message: exception?.Message,
                    exception: exception));

        public static Operation<Result> Fail<Result>(Exception exception = null) 
            => new Sync.SyncOperation<Result>(
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
