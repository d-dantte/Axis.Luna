namespace Axis.Luna
{
    using System;
    using System.Threading.Tasks;
    using MetaTypes;

    using static Extensions.ObjectExtensions;
    using static Extensions.ExceptionExtensions;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class Operation<R>
    {
        protected Operation() { }


        internal Operation(Func<R> func)
        {
            ThrowNullArguments(() => func);

            try
            {
                Result = func();
                Succeeded = true;
            }
            catch (Exception e)
            {
                Error = e;
                Succeeded = false;
            }
        }

        #region Properties
        internal Exception Error { get; set; }
        private string _message;

        public virtual R Result { get; internal set; }
        public bool Succeeded { get; set; }
        public string Message
        {
            get { return Error?.GetRoot(e => e.InnerException)?.Message ?? _message; }
            set { _message = value; }
        }
        #endregion

        #region Methods 
        public R Resolve() => Result.ThrowIf(r => Error != null, new Exception("See inner exception", Error));
        public Exception GetException() => Error;
        #endregion
    }

    public class Operation : Operation<@void>
    {
        #region init
        internal Operation(Action action)
        : base(() => Void(action))
        { }
        #endregion

        #region Statics

        #region Operation
        public static Operation Run(Action action) => new Operation(action);
        public static Operation Try(Action action) => Run(action);

        public static Operation<Result> Run<Result>(Func<Result> func) => new Operation<Result>(func);
        public static Operation<Result> Try<Result>(Func<Result> func) => Run(func);

        public static Operation Run(Func<Operation> func) => Eval(() => func(), ex => Fail(ex));
        public static Operation Try(Func<Operation> func) => Run(func);

        public static Operation<Result> Run<Result>(Func<Operation<Result>> func) => Eval(func, ex => Fail<Result>(ex));
        public static Operation<Result> Try<Result>(Func<Operation<Result>> func) => Run(func);

        public static Operation<R> NoOp<R>() => Run(() => { return default(R); });
        public static Operation NoOp() =>  Run(() => { });

        public static Operation<Value> FromValue<Value>(Value v) => Run(() => { return v; });

        public static Operation<R> Fail<R>(Exception ex = null)
            => Run(() => (ex ?? new Exception("Operation Failed")).Throw<R>());
        public static Operation<R> Fail<R>(string message = null)
            => Run(() => new Exception(message ?? "Operation Failed").Throw<R>());

        public static Operation Fail(string message = null)
            => Run(() => new Exception(message ?? "Operation Failed").Throw());
        public static Operation Fail(Exception ex = null)
            => Run(() => (ex ?? new Exception("Operation Failed")).Throw());

        #endregion

        #region async operation

        public static AsyncOperation RunAsync(Action action) => new AsyncOperation(action);
        public static AsyncOperation TryAsync(Action action) => RunAsync(action);
        public static AsyncOperation RunAsync(ContinuationInfo info, Action action) => new AsyncOperation(action, info);
        public static AsyncOperation TryAsync(ContinuationInfo info, Action action) => RunAsync(info, action);

        public static AsyncOperation<Result> RunAsync<Result>(Func<Result> func) => new AsyncOperation<Result>(func);
        public static AsyncOperation<Result> TryASync<Result>(Func<Result> func) => RunAsync(func);
        public static AsyncOperation<Result> RunAsync<Result>(ContinuationInfo info, Func<Result> func) => new AsyncOperation<Result>(func, info);
        public static AsyncOperation<Result> TryASync<Result>(ContinuationInfo info, Func<Result> func) => RunAsync(info, func);

        public static AsyncOperation RunAsync(Func<AsyncOperation> func) => Eval(func, ex => FailAsync(ex));
        public static AsyncOperation TryAsync(Func<AsyncOperation> func) => RunAsync(func);

        public static AsyncOperation<Result> RunAsync<Result>(Func<AsyncOperation<Result>> func) => Eval(func, ex => FailAsync<Result>(ex));
        public static AsyncOperation<Result> TryAsync<Result>(Func<AsyncOperation<Result>> func) => RunAsync(func);

        public static AsyncOperation<R> NoOpAsync<R>() => new AsyncOperation<R>(default(R));
        public static AsyncOperation NoOpAsync() => new AsyncOperation();

        public static AsyncOperation<Value> FromValueAsync<Value>(Value v) => new AsyncOperation<Value>(v);

        public static AsyncOperation<R> FailAsync<R>(Exception ex = null)
            => RunAsync(() => (ex ?? new Exception("Operation Failed")).Throw<R>());
        public static AsyncOperation<R> FailAsync<R>(string message = null)
            => RunAsync(() => new Exception(message ?? "Operation Failed").Throw<R>());

        public static AsyncOperation FailAsync(string message = null)
            => RunAsync(() => new Exception(message ?? "Operation Failed").Throw());
        public static AsyncOperation FailAsync(Exception ex = null)
            => RunAsync(() => (ex ?? new Exception("Operation Failed")).Throw());

        #endregion

        #endregion
    }



    public class AsyncOperation<R>
    {
        internal AsyncOperation(Func<R> func, ContinuationInfo info = null)
        {
            ThrowNullArguments(() => func);

            this._task = new Task<R>(func, Eval(() => info.CancellationToken), Eval(() => info.TaskCreationOptions));
        }
        internal AsyncOperation(R value)
        {
            this._task = Task.FromResult(value);
        }

        #region Properties

        private string _message;
        internal Task<R> _task;


        public string Message
        {
            get { return Error?.GetRoot(e => e.InnerException)?.Message ?? _message; }
            set { _message = value; }
        }

        internal Exception Error { get; set; }

        public virtual R Result { get; internal set; }

        public bool Succeeded { get; set; }

        #endregion

        #region Methods 
        public R Resolve() => Result.ThrowIf(r => Error != null, new Exception("See inner exception", Error));
        public Exception GetException() => Error;
        #endregion

        public TaskAwaiter<R> GetAwaiter() => _task.GetAwaiter();
    }

    public class AsyncOperation: AsyncOperation<@void>
    {
        internal AsyncOperation(Action action, ContinuationInfo info = null)
        : base(() => Void(action), info)
        { }

        internal AsyncOperation()
        : base(Void())
        { }
    }


    public class ContinuationInfo
    {
        public CancellationToken CancellationToken { get; set; }
        public TaskCreationOptions TaskCreationOptions { get; set; }
    }
}
