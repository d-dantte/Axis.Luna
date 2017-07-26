using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation
{
    [DebuggerStepThrough]
    public class AsyncOperation : IOperation
    {
        private Task _task;

        public bool? Succeeded => _task.Status == TaskStatus.RanToCompletion ? true :
                                  _task.Status == TaskStatus.Canceled ? false :
                                  _task.Status == TaskStatus.Faulted ? false :
                                  (bool?)null;

        public AsyncOperation(Action operation)
        : this(Task.Run(operation))
        { 
        }

        public AsyncOperation(Task task)
        {
            if (task == null) throw new Exception("null argument");

            this._task = task;
        }

        public Exception GetException() => _task.GetInnerException();

        public void Resolve() => _task.GetAwaiter().GetResult();

        public TaskAwaiter GetAwaiter() => _task.GetAwaiter();


        #region Continuations
        public IOperation Then(Action continuation, Action<Exception> error = null)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            if(_t.Status != TaskStatus.RanToCompletion)
            {
                var e = _t.GetInnerException();
                error?.Invoke(e);
                throw e;
            }

            continuation?.Invoke();
        }));

        public IOperation<R> Then<R>(Func<R> continuation, Action<Exception> error = null)
        => new AsyncOperation<R>(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
            {
                var e = _t.GetInnerException();
                error?.Invoke(e);
                throw e;
            }

            return continuation == null ? default(R) : continuation.Invoke();
        }));


        public IOperation Then(Func<IOperation> continuation, Action<Exception> error = null)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
            {
                var e = _t.GetInnerException();
                error?.Invoke(e);
                throw e;
            }

            continuation
                ?.Invoke()
                ?.Resolve();
        }));

        public IOperation<S> Then<S>(Func<IOperation<S>> continuation, Action<Exception> error = null)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
            {
                var e = _t.GetInnerException();
                error?.Invoke(e);
                throw e;
            }

            var innerOp = continuation?.Invoke();
            return innerOp != null ? innerOp.Resolve() : default(S);
        }));



        public IOperation ContinueWith(Action<IOperation> continuation) => new AsyncOperation(_task.ContinueWith(_t => continuation?.Invoke(this)));

        public IOperation<S> ContinueWith<S>(Func<IOperation, S> continuation)
        => new AsyncOperation<S>(_task.ContinueWith(_t => continuation != null ? continuation.Invoke(this) : default(S)));

        public IOperation ContinueWith(Func<IOperation, IOperation> continuation) => new AsyncOperation(_task.ContinueWith(_t => continuation?.Invoke(this)?.Resolve()));

        public IOperation<S> ContinueWith<S>(Func<IOperation, IOperation<S>> continuation) 
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            var inner = continuation?.Invoke(this);
            return inner == null ? default(S) : inner.Resolve();
        }));


        public IOperation Finally(Action @finally)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            @finally?.Invoke();

            Resolve();
        }));
        #endregion
    }

    [DebuggerStepThrough]
    public class AsyncOperation<R> : IOperation<R>
    { 
        private Task<R> _task;

        public bool? Succeeded => _task.Status == TaskStatus.RanToCompletion ? true :
                                  _task.Status == TaskStatus.Canceled ? false :
                                  _task.Status == TaskStatus.Faulted ? false :
                                  (bool?)null;

        public R Result => Succeeded == true ? Resolve() : default(R);


        public AsyncOperation(Func<R> operation)
        : this(Task.Run(operation))
        { 
        }

        public AsyncOperation(Task<R> task)
        {
            if (task == null) throw new Exception("null argument");

            _task = task;
        }

        public Exception GetException() => _task.GetInnerException();

        public R Resolve() => _task.GetAwaiter().GetResult();


        #region Continuations
        public IOperation Then(Action<R> continuation, Action<Exception> error = null)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
            {
                var e = _t.GetInnerException();
                error?.Invoke(e);
                throw e;
            }

            continuation?.Invoke(_t.Result);
        }));

        public IOperation<S> Then<S>(Func<R, S> continuation, Action<Exception> error = null)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
            {
                var e = _t.GetInnerException();
                error?.Invoke(e);
                throw e;
            }

            return continuation == null? default(S): continuation.Invoke(_t.Result);
        }));


        public IOperation Then(Func<R, IOperation> continuation, Action<Exception> error = null)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
            {
                var e = _t.GetInnerException();
                error?.Invoke(e);
                throw e;
            }

            continuation
                ?.Invoke(_t.Result)
                ?.Resolve();
        }));

        public IOperation<S> Then<S>(Func<R, IOperation<S>> continuation, Action<Exception> error = null)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
            {
                var e = _t.GetInnerException();
                error?.Invoke(e);
                throw e;
            }

            var innerOp = continuation?.Invoke(_t.Result);
            return innerOp.Resolve();
        }));



        public IOperation ContinueWith(Action<IOperation<R>> continuation) => new AsyncOperation(_task.ContinueWith(_t => continuation?.Invoke(this)));

        public IOperation<S> ContinueWith<S>(Func<IOperation<R>, S> continuation) 
        => new AsyncOperation<S>(_task.ContinueWith(_t => continuation == null? default(S): continuation.Invoke(this)));

        public IOperation ContinueWith(Func<IOperation<R>, IOperation> continuation) => new AsyncOperation(_task.ContinueWith(_t => continuation?.Invoke(this)?.Resolve()));

        public IOperation<S> ContinueWith<S>(Func<IOperation<R>, IOperation<S>> continuation) 
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            var innerOp = continuation?.Invoke(this);
            return innerOp == null ? default(S) : innerOp.Resolve();
        }));


        public IOperation<R> Finally(Action @finally)
        => new AsyncOperation<R>(_task.ContinueWith(_t =>
        {
            @finally.Invoke();

            return Resolve();
        }));
        #endregion
    }


    #region Helper
    [DebuggerStepThrough]
    public static class AsyncOp
    {
        public static AsyncOperation Try(Action operation) => new AsyncOperation(operation);
        public static AsyncOperation Try(Func<IOperation> operation) => new AsyncOperation(() => operation().Resolve());

        public static AsyncOperation<R> Try<R>(Func<R> operation) => new AsyncOperation<R>(operation);
        public static AsyncOperation<R> Try<R>(Func<IOperation<R>> operation) => new AsyncOperation<R>(() => operation().Resolve());

        public static AsyncOperation Fail(Exception ex) => new AsyncOperation(() => { throw ex; });
        public static AsyncOperation<R> Fail<R>(Exception ex) => new AsyncOperation<R>(() => { throw ex; });


        public static Exception GetInnerException(this Task t)
        {
            Exception e = null;
            try
            {
                if (t.Exception != null)
                    t.GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                e = ex;
            }

            return e;
        }
    }
    #endregion
}
