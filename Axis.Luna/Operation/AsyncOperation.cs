using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Axis.Luna.Extensions.ExceptionExtensions;

namespace Axis.Luna.Operation
{
    public class AsyncOperation : IOperation
    {
        private Exception _exception;
        private Task _task;

        public bool? Succeeded { get; set; }

        public AsyncOperation(Action operation)
        : this(Task.Run(operation))
        { 
        }

        public AsyncOperation(Task task)
        {
            ThrowNullArguments(() => task);

            this._task = task;
        }

        public Exception GetException() => _exception;

        public void Resolve() => _task.GetAwaiter().GetResult();

        public TaskAwaiter GetAwaiter() => _task.GetAwaiter();


        #region Continuations
        public IOperation Then(Action continuation, Action<Exception> error = null)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            try
            {
                continuation.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
        }));

        public IOperation<R> Then<R>(Func<R> continuation, Action<Exception> error = null)
        => new AsyncOperation<R>(_task.ContinueWith(_t =>
        {
            try
            {
                return continuation.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
        }));


        public IOperation Then(Func<IOperation> continuation, Action<Exception> error = null)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            var innerOp = continuation.Invoke();
            innerOp.Resolve();
        }));

        public IOperation<S> Then<S>(Func<IOperation<S>> continuation, Action<Exception> error = null)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            var innerOp = continuation.Invoke();
            return innerOp.Resolve();
        }));
        #endregion

        #region Error
        public IOperation Otherwise(Action<Exception> errorContinuation)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            if(_t.Status != TaskStatus.RanToCompletion) errorContinuation(_t.GetInnerException());
        }));

        public IOperation<R> Otherwise<R>(Func<Exception, R> errorContinuation, Func<R> successContinuation)
        => new AsyncOperation<R>(_task.ContinueWith(_t =>
        {
            if (_t.Status == TaskStatus.RanToCompletion) return successContinuation();
            else return errorContinuation(_t.GetInnerException());
        }));

        public IOperation Otherwise(Func<Exception, IOperation> errorContinuation)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
                errorContinuation.Invoke(_t.GetInnerException()).Resolve();
        }));

        public IOperation<S> Otherwise<S>(Func<Exception, IOperation<S>> errorContinuation, Func<S> successContinuation)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            if (_t.Status == TaskStatus.RanToCompletion)
                return successContinuation.Invoke();

            else return errorContinuation.Invoke(_t.GetInnerException()).Resolve();
        }));
        #endregion

        #region Finally
        public IOperation Finally(Action @finally)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            @finally.Invoke();
        }));
        #endregion
    }

    public class AsyncOperation<R> : IOperation<R>
    {
        private Exception _exception;
        private Task<R> _task;

        public bool? Succeeded { get; set; }

        public R Result => Succeeded == true ? Resolve() : default(R);


        public AsyncOperation(Func<R> operation)
        : this(Task.Run(operation))
        { 
        }

        public AsyncOperation(Task<R> task)
        {
            ThrowNullArguments(() => task);

            _task = task;
        }

        public Exception GetException() => _exception;

        public R Resolve() => _task.GetAwaiter().GetResult();


        #region Continuations
        public IOperation Then(Action<R> continuation, Action<Exception> error = null)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            try
            {
                continuation.Invoke(Resolve());
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
        }));

        public IOperation<S> Then<S>(Func<R, S> continuation, Action<Exception> error = null)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            try
            {
                return continuation.Invoke(Resolve());
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
        }));

        public IOperation Then(Func<R, IOperation> continuation, Action<Exception> error = null)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            var innerOp = continuation.Invoke(Resolve());
            innerOp.Resolve();
        }));

        public IOperation<S> Then<S>(Func<R, IOperation<S>> continuation, Action<Exception> error = null)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            var innerOp = continuation.Invoke(Resolve());
            return innerOp.Resolve();
        }));
        #endregion

        #region Error
        public IOperation Otherwise(Action<Exception> errorContinuation)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion) errorContinuation(_t.GetInnerException());
        }));

        public IOperation<S> Otherwise<S>(Func<Exception, S> errorContinuation, Func<R, S> successContinuation)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            if (_t.Status == TaskStatus.RanToCompletion)
                return successContinuation(Resolve());

            else return errorContinuation(_t.GetInnerException());
        }));

        public IOperation Otherwise(Func<Exception, IOperation> errorContinuation)
        => new AsyncOperation(_task.ContinueWith(_t =>
        {
            if (_t.Status != TaskStatus.RanToCompletion)
                errorContinuation.Invoke(_t.GetInnerException()).Resolve();
        }));

        public IOperation<S> Otherwise<S>(Func<Exception, IOperation<S>> errorContinuation, Func<R, S> successContinuation)
        => new AsyncOperation<S>(_task.ContinueWith(_t =>
        {
            if (_t.Status == TaskStatus.RanToCompletion)
                return successContinuation.Invoke(Resolve());

            else return errorContinuation.Invoke(_t.GetInnerException()).Resolve();
        }));
        #endregion

        #region Finally
        public IOperation<R> Finally(Action @finally)
        => new AsyncOperation<R>(_task.ContinueWith(_t =>
        {
            @finally.Invoke();

            return Resolve();
        }));
        #endregion
    }


    #region Helper
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
