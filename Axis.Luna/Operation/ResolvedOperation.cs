using System;

using static Axis.Luna.Extensions.ExceptionExtensions;

namespace Axis.Luna.Operation
{
    public class ResolvedOperation : IOperation
    {
        private Exception _exception;

        public bool? Succeeded { get; set; }

        public ResolvedOperation(Action operation)
        {
            ThrowNullArguments(() => operation);

            try
            {
                operation.Invoke();
                Succeeded = true;
            }
            catch(Exception e)
            {
                this._exception = e;
                Succeeded = false;
            }
        }

        public Exception GetException() => _exception;

        public void Resolve()
        {
            if (_exception != null) throw _exception;
        }


        #region Continuations
        public IOperation Then(Action continuation, Action<Exception> error = null)
        => new ResolvedOperation(() =>
        {
            try
            {
                Resolve();
                continuation.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
        });

        public IOperation<R> Then<R>(Func<R> continuation, Action<Exception> error = null)
        => new ResolvedOperation<R>(() =>
        {
            try
            {
                Resolve();
                return continuation.Invoke();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
        });


        public IOperation Then(Func<IOperation> continuation, Action<Exception> error = null)
        => new ResolvedOperation(() =>
        {
            Resolve();
            var innerOp = continuation.Invoke();
            innerOp.Resolve();
        });

        public IOperation<S> Then<S>(Func<IOperation<S>> continuation, Action<Exception> error = null)
        => new ResolvedOperation<S>(() =>
        {
            Resolve();
            var innerOp = continuation.Invoke();
            return innerOp.Resolve();
        });
        #endregion

        #region Error
        public IOperation Otherwise(Action<Exception> errorContinuation)
        => new ResolvedOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch (Exception e)
            {
                errorContinuation(e);
                //not re-throwing e makes this a successfull operation upon resolution
            }
        });

        public IOperation<R> Otherwise<R>(Func<Exception, R> errorContinuation, Func<R> successContinuation)
        => new ResolvedOperation<R>(() =>
        {
            try
            {
                Resolve();
                return successContinuation();
            }
            catch (Exception e)
            {
                return errorContinuation(e);
                //not re-throwing e makes this a successfull operation upon resolution
            }
        });

        public IOperation Otherwise(Func<Exception, IOperation> errorContinuation)
        => new ResolvedOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch (Exception e)
            {
                var innerOp = errorContinuation(e);
                innerOp.Resolve();
                //not re-throwing e makes this a successfull operation upon resolution
            }
        });

        public IOperation<S> Otherwise<S>(Func<Exception, IOperation<S>> errorContinuation, Func<S> successContinuation)
        => new ResolvedOperation<S>(() =>
        {
            try
            {
                Resolve();
                return successContinuation();
            }
            catch (Exception e)
            {
                var innerOp = errorContinuation(e);
                return innerOp.Resolve();
                //not re-throwing e makes this a successfull operation upon resolution
            }
        });
        #endregion

        #region Finally
        public IOperation Finally(Action @finally)
        => new ResolvedOperation(() =>
        {
            try
            {
                Resolve();
            }
            finally
            {
                @finally.Invoke();
            }
        });
        #endregion
    }

    public class ResolvedOperation<R> : IOperation<R>
    {
        private Exception _exception;
        private R _result;

        public bool? Succeeded { get; set; }

        public R Result => Succeeded == true ? Resolve() : default(R);


        public ResolvedOperation(Func<R> operation)
        {
            ThrowNullArguments(() => operation);

            try
            {
                _result = operation.Invoke();
                Succeeded = true;
            }
            catch (Exception e)
            {
                this._exception = e;
                Succeeded = false;
            }
        }

        public Exception GetException() => _exception;

        public R Resolve()
        {
            if (_exception != null) throw _exception;
            else return _result;
        }


        #region Continuations
        public IOperation Then(Action<R> continuation, Action<Exception> error = null)
        => new ResolvedOperation(() =>
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
        });
        public IOperation<S> Then<S>(Func<R, S> continuation, Action<Exception> error = null)
        => new ResolvedOperation<S>(() =>
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
        });

        public IOperation Then(Func<R, IOperation> continuation, Action<Exception> error = null)
        => new ResolvedOperation(() =>
        {
            var innerOp = continuation.Invoke(Resolve());
            innerOp.Resolve();
        });
        public IOperation<S> Then<S>(Func<R, IOperation<S>> continuation, Action<Exception> error = null)
        => new ResolvedOperation<S>(() =>
        {
            var innerOp = continuation.Invoke(Resolve());
            return innerOp.Resolve();
        });
        #endregion

        #region Error
        public IOperation Otherwise(Action<Exception> errorContinuation)
        => new ResolvedOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch (Exception e)
            {
                errorContinuation(e);
                //not re-throwing e makes this a successfull operation upon resolution
            }
        });

        public IOperation<S> Otherwise<S>(Func<Exception, S> errorContinuation, Func<R, S> successContinuation)
        => new ResolvedOperation<S>(() =>
        {
            try
            {                
                return successContinuation(Resolve());
            }
            catch (Exception e)
            {
                return errorContinuation(e);
                //not re-throwing e makes this a successfull operation upon resolution
            }
        });

        public IOperation Otherwise(Func<Exception, IOperation> errorContinuation)
        => new ResolvedOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch (Exception e)
            {
                var innerOp = errorContinuation(e);
                innerOp.Resolve();
                //not re-throwing e makes this a successfull operation upon resolution
            }
        });

        public IOperation<S> Otherwise<S>(Func<Exception, IOperation<S>> errorContinuation, Func<R, S> successContinuation)
        => new ResolvedOperation<S>(() =>
        {
            try
            {
                return successContinuation(Resolve());
            }
            catch (Exception e)
            {
                var innerOp = errorContinuation(e);
                return innerOp.Resolve();
                //not re-throwing e makes this a successfull operation upon resolution
            }
        });
        #endregion

        #region Finally
        public IOperation<R> Finally(Action @finally)
        => new ResolvedOperation<R>(() =>
        {
            try
            {
                return Resolve();
            }
            finally
            {
                @finally.Invoke();
            }
        });
        #endregion
    }


    #region Helper
    public static class ResolvedOp
    {
        public static ResolvedOperation Try(Action operation) => new ResolvedOperation(operation);
        public static ResolvedOperation Try(Func<IOperation> operation) => new ResolvedOperation(() => operation().Resolve());

        public static ResolvedOperation<R> Try<R>(Func<R> operation) => new ResolvedOperation<R>(operation);
        public static ResolvedOperation<R> Try<R>(Func<IOperation<R>> operation) => new ResolvedOperation<R>(() => operation().Resolve());

        public static ResolvedOperation Fail(Exception ex) => new ResolvedOperation(() => { throw ex; });
        public static ResolvedOperation<R> Fail<R>(Exception ex) => new ResolvedOperation<R>(() => { throw ex; });

        public static ResolvedOperation<R> FromValue<R>(R value) => Try(() => value);
    }
    #endregion
}
