namespace Axis.Luna
{
    using System;
    using System.Threading.Tasks;
    using MetaTypes;

    using static Extensions.ObjectExtensions;
    using static Extensions.ExceptionExtensions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Diagnostics;

    #region Synchronious Operation
    public class Operation<R>
    {
        [DebuggerHidden]
        protected Operation() { }

        [DebuggerHidden]
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
        public bool Succeeded { get; internal set; }
        public string Message
        {
            get { return Error?.GetRoot(e => e.InnerException)?.Message ?? _message; }
            set { _message = value; }
        }
        #endregion

        #region Methods 
        [DebuggerHidden]
        public R Resolve() => Result.ThrowIf(r => Error != null, new Exception("See inner exception", Error));
        [DebuggerHidden]
        public Exception GetException() => Error;
        #endregion
    }

    public class Operation : Operation<@void>
    {
        #region init
        [DebuggerHidden]
        internal Operation(Action action)
        : base(() => Void(action))
        { }

        [DebuggerHidden]
        internal Operation() : base()
        { }
        #endregion

        #region Statics

        #region Operation
        [DebuggerHidden]
        public static Operation Run(Action action) => new Operation(action);
        [DebuggerHidden]
        public static Operation Try(Action action) => Run(action);

        [DebuggerHidden]
        public static Operation<Result> Run<Result>(Func<Result> func) => new Operation<Result>(func);
        [DebuggerHidden]
        public static Operation<Result> Try<Result>(Func<Result> func) => Run(func);

        [DebuggerHidden]
        public static Operation Run(Func<Operation> func) => Eval(() => func(), ex => Fail(ex));
        [DebuggerHidden]
        public static Operation Try(Func<Operation> func) => Run(func);

        [DebuggerHidden]
        public static Operation<Result> Run<Result>(Func<Operation<Result>> func) => Eval(func, ex => Fail<Result>(ex));
        [DebuggerHidden]
        public static Operation<Result> Try<Result>(Func<Operation<Result>> func) => Run(func);

        [DebuggerHidden]
        public static Operation<R> NoOp<R>() => Run(() => { return default(R); });
        [DebuggerHidden]
        public static Operation NoOp() => Run(() => { });

        [DebuggerHidden]
        public static Operation<Value> FromValue<Value>(Value v) => Run(() => { return v; });

        [DebuggerHidden]
        public static Operation<R> Fail<R>(Exception ex = null)
            => Run(() => (ex ?? new Exception("Operation Failed")).Throw<R>());
        [DebuggerHidden]
        public static Operation<R> Fail<R>(string message = null)
            => Run(() => new Exception(message ?? "Operation Failed").Throw<R>());

        [DebuggerHidden]
        public static Operation Fail(string message = null)
            => Run(() => new Exception(message ?? "Operation Failed").Throw());
        [DebuggerHidden]
        public static Operation Fail(Exception ex = null)
            => Run(() => (ex ?? new Exception("Operation Failed")).Throw());

        #endregion

        #region Operation
        [DebuggerHidden]
        public static LazyOperation<@void> TryLazily(Action action) => new LazyOperation<@void>(Foid(action));

        [DebuggerHidden]
        public static LazyOperation<Result> TryLazily<Result>(Func<Result> func) => new LazyOperation<Result>(func);

        [DebuggerHidden]
        public static LazyOperation<@void> TryLazily(Func<LazyOperation<@void>> func) => new LazyOperation<@void>(func);

        [DebuggerHidden]
        public static LazyOperation<Result> TryLazily<Result>(Func<LazyOperation<Result>> func) => new LazyOperation<Result>(func);

        [DebuggerHidden]
        public static LazyOperation<R> NoOpLazily<R>() => new LazyOperation<R>(() => default(R));
        [DebuggerHidden]
        public static LazyOperation<@void> NoOpLazily() => new LazyOperation<@void>(() => @void.Instance);


        [DebuggerHidden]
        public static LazyOperation<R> FailLazily<R>(Exception ex = null)
            => TryLazily(() => (ex ?? new Exception("Operation Failed")).Throw<R>());
        [DebuggerHidden]
        public static LazyOperation<R> FailLazily<R>(string message = null)
            => TryLazily(() => new Exception(message ?? "Operation Failed").Throw<R>());

        [DebuggerHidden]
        public static LazyOperation<@void> FailLazily(string message = null)
            => TryLazily(() => new Exception(message ?? "Operation Failed").Throw());
        [DebuggerHidden]
        public static LazyOperation<@void> FailLazily(Exception ex = null)
            => TryLazily(() => (ex ?? new Exception("Operation Failed")).Throw());

        #endregion

        #region async operation

        [DebuggerHidden] public static AsyncOperation RunAsync(Action action) => new AsyncOperation(action);
        [DebuggerHidden] public static AsyncOperation TryAsync(Action action) => RunAsync(action);
        [DebuggerHidden] public static AsyncOperation RunAsync(AsyncInfo info, Action action) => new AsyncOperation(action, info);
        [DebuggerHidden] public static AsyncOperation TryAsync(AsyncInfo info, Action action) => RunAsync(info, action);

        [DebuggerHidden] public static AsyncOperation<Result> RunAsync<Result>(Func<Result> func) => new AsyncOperation<Result>(func);
        [DebuggerHidden] public static AsyncOperation<Result> TryAsync<Result>(Func<Result> func) => RunAsync(func);
        [DebuggerHidden] public static AsyncOperation<Result> RunAsync<Result>(AsyncInfo info, Func<Result> func) => new AsyncOperation<Result>(func, info);
        [DebuggerHidden] public static AsyncOperation<Result> TryAsync<Result>(AsyncInfo info, Func<Result> func) => RunAsync(info, func);

        [DebuggerHidden] public static AsyncOperation RunAsync(Func<AsyncOperation> func) => Eval(func, ex => FailAsync(ex));
        [DebuggerHidden] public static AsyncOperation TryAsync(Func<AsyncOperation> func) => RunAsync(func);

        [DebuggerHidden] public static AsyncOperation<Result> RunAsync<Result>(Func<AsyncOperation<Result>> func) => Eval(func, ex => FailAsync<Result>(ex));
        [DebuggerHidden] public static AsyncOperation<Result> TryAsync<Result>(Func<AsyncOperation<Result>> func) => RunAsync(func);

        [DebuggerHidden] public static AsyncOperation<R> NoOpAsync<R>() => new AsyncOperation<R>(default(R));
        [DebuggerHidden] public static AsyncOperation NoOpAsync() => new AsyncOperation(()=> { });

        [DebuggerHidden] public static AsyncOperation<Value> FromValueAsync<Value>(Value v) => new AsyncOperation<Value>(v);

        [DebuggerHidden] public static AsyncOperation<R> FailAsync<R>(Exception ex = null)
            => RunAsync(() => (ex ?? new Exception("Operation Failed")).Throw<R>());
        [DebuggerHidden] public static AsyncOperation<R> FailAsync<R>(string message = null)
            => RunAsync(() => new Exception(message ?? "Operation Failed").Throw<R>());

        [DebuggerHidden] public static AsyncOperation FailAsync(string message = null)
            => RunAsync(() => new Exception(message ?? "Operation Failed").Throw());
        [DebuggerHidden] public static AsyncOperation FailAsync(Exception ex = null)
            => RunAsync(() => (ex ?? new Exception("Operation Failed")).Throw());

        #endregion

        #endregion
    }
    #endregion

    #region Async Operation
    public class AsyncOperation<R>
    {
        [DebuggerHidden]
        internal AsyncOperation(Func<R> func, Task previousOperationTask) 
        : this(func, previousOperationTask, null)
        { }
        [DebuggerHidden]
        internal AsyncOperation(Func<R> func, AsyncInfo info = null) 
        : this(func, null, info)
        { }
        [DebuggerHidden]
        internal AsyncOperation(Func<R> func, Task previousOperationTask, AsyncInfo info = null)
        {
            ThrowNullArguments(() => func);

            if (previousOperationTask == null)
            {
                _task = info == null ? new Task<R>(func) :
                        info.CancellationToken == default(CancellationToken) ? new Task<R>(func, info.TaskCreationOptions) :
                        new Task<R>(func, info.CancellationToken, info.TaskCreationOptions);
                _task.Start();
            }
            else if (NotAborted(previousOperationTask)) //if the previous task is true or null, meaning it succeeded, or it's not yet completed
                _task = info == null ? previousOperationTask.ContinueWith(tsk => RunFunc(tsk, func)) : previousOperationTask.ContinueWith(tsk => RunFunc(tsk, func), info.TaskContinuationOptions);

            else _task = new TaskCompletionSource<R>().UsingValue(_tcs => _tcs.SetException(previousOperationTask.Exception)).Task; //create a failed task
        }
        [DebuggerHidden]
        internal AsyncOperation(R value)
        {
            this._task = System.Threading.Tasks.Task.FromResult(value);
        }

        #region Properties
        
        private Task<R> _task;

        public string Message => GetException()?.GetRoot(e => e.InnerException)?.Message ?? "";        

        public virtual R Result => Eval(() => Resolve(), ex => default(R));

        public bool? Succeeded => !_task.IsCompleted ? null : (bool?)(_task.Status == TaskStatus.RanToCompletion);

        #endregion

        #region Methods 
        internal Task Task() => _task;

        [DebuggerHidden]
        public R Resolve()
        {
            try
            {
                return _task.Result;
            }
            catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
            //rethrow any other kind of exception
        }

        [DebuggerHidden]
        public Exception GetException() => _task.Exception;

        private R RunFunc(Task previoiusTask, Func<R> func)
        {
            if (previoiusTask.Status == TaskStatus.RanToCompletion) return func();
            else throw previoiusTask.Exception.InnerException;
        }

        private bool NotAborted(Task task)
            => task.Status != TaskStatus.Faulted &&
               task.Status != TaskStatus.Canceled;

        public TaskAwaiter<R> GetAwaiter() => _task.GetAwaiter();

        #endregion
    }

    public class AsyncOperation : AsyncOperation<@void>
    {
        internal AsyncOperation(Action action, Task previousOperationTask)
        : this(action, previousOperationTask, null)
        { }
        internal AsyncOperation(Action action, AsyncInfo info = null)
        : this(action, null, info)
        { }
        internal AsyncOperation(Action action, Task previousOperationTask, AsyncInfo info = null)
        : base(() => Void(action), previousOperationTask, info)
        { }
    }

    public class AsyncInfo
    {
        public CancellationToken CancellationToken { get; set; }
        public TaskCreationOptions TaskCreationOptions { get; set; }
        public TaskContinuationOptions TaskContinuationOptions { get; set; }
    }
    #endregion

    #region Lazy Operation
    public class LazyOperation<R>
    {
        internal LazyOperation(Func<R> func)
        {
            ThrowNullArguments(() => func);

            _func = func;
        }
        internal LazyOperation(Func<LazyOperation<R>> func)
        {
            ThrowNullArguments(() => func);

            _func = () => func().Resolve();
        }

        #region Properties
        internal Exception Error { get; set; }
        private R _result;
        private bool _resolved = false;
        private Func<R> _func;

        public virtual R Result
        {
            get { return Eval(() => Resolve()); }
            set { _result = value; }
        }
        public bool? Succeeded
        {
            get { return !_resolved ? null : Error == null ? (bool?)true : false; }
            set { if (value == true) _resolved = true; }
        }
        public string Message
        {
            get { return Error?.GetRoot(e => e.InnerException)?.Message; }
            set { if (!string.IsNullOrEmpty(value)) Error = new Exception(value); }
        }
        #endregion

        #region Methods 
        public R Resolve()
        {
            if(_resolved)
            {
                if (Error == null) return _result;
                else throw Error;
            }
            else try
            {
                return _result = _func();                
            }
            catch (Exception e)
            {
                throw Error = e;
            }
            finally
            {
                _resolved = true;
            }
        }
        public Exception GetException() => Error;
        #endregion
    }
    #endregion
}
