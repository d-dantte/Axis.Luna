﻿using System;

using static Axis.Luna.Extensions.ExceptionExtensions;

namespace Axis.Luna.Operation
{
    public class LazyOperation : IOperation
    {
        private Exception _exception;
        private Action _operation;

        public bool? Succeeded { get; set; }
        
        public LazyOperation(Action operation)
        {
            ThrowNullArguments(() => operation);

            _operation = operation;
        }

        public Exception GetException() => _exception;

        public void Resolve()
        {
            if (_exception != null) throw _exception;
            else if (!Succeeded.HasValue)
            {
                try
                {
                    _operation.Invoke();
                    Succeeded = true;
                }
                catch (Exception e)
                {
                    _exception = e;
                    Succeeded = false;

                    throw e;
                }
            }
        }


        #region Continuations
        public IOperation Then(Action continuation, Action<Exception> error = null)
        => new LazyOperation(() =>
        {
            try
            {
                Resolve();
                continuation.Invoke();
            }
            catch(Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
        });

        public IOperation<R> Then<R>(Func<R> continuation, Action<Exception> error = null)
        => new LazyOperation<R>(() =>
        {
            try
            {
                Resolve();
                return continuation.Invoke();
            }
            catch(Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
        });


        public IOperation Then(Func<IOperation> continuation, Action<Exception> error = null)
        => new LazyOperation(() =>
        {
            Resolve();
            var innerOp = continuation.Invoke();
            innerOp.Resolve();
        });

        public IOperation<S> Then<S>(Func<IOperation<S>> continuation, Action<Exception> error = null)
        => new LazyOperation<S>(() =>
        {
            Resolve();
            var innerOp = continuation.Invoke();
            return innerOp.Resolve();
        });
        #endregion

        #region Error
        public IOperation Otherwise(Action<Exception> errorContinuation)
        => new LazyOperation(() =>
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
        => new LazyOperation<R>(() =>
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
        => new LazyOperation(() =>
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
        => new LazyOperation<S>(() =>
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
        => new LazyOperation(() =>
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

    public class LazyOperation<R> : IOperation<R>
    {
        private Exception _exception;
        private Func<R> _operation;
        private R _result;

        public bool? Succeeded { get; set; }

        public R Result => Succeeded == true ? Resolve() : default(R);


        public LazyOperation(Func<R> operation)
        {
            ThrowNullArguments(() => operation);

            _operation = operation;
        }

        public Exception GetException() => _exception;

        public R Resolve()
        {
            if (_exception != null) throw _exception;
            else if (!Succeeded.HasValue)
            {
                try
                {
                    _result = _operation.Invoke();
                    Succeeded = true;
                }
                catch (Exception e)
                {
                    _exception = e;
                    Succeeded = false;

                    throw e;
                }
            }

            return _result;
        }


        #region Continuations
        public IOperation Then(Action<R> continuation, Action<Exception> error = null)
        => new LazyOperation(() =>
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
        => new LazyOperation<S>(() =>
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
        => new LazyOperation(() =>
        {
            var innerOp = continuation.Invoke(Resolve());
            innerOp.Resolve();
        });

        public IOperation<S> Then<S>(Func<R, IOperation<S>> continuation, Action<Exception> error = null)
        => new LazyOperation<S>(() =>
        {
            var innerOp = continuation.Invoke(Resolve());
            return innerOp.Resolve();
        });
        #endregion

        #region Error
        public IOperation Otherwise(Action<Exception> errorContinuation)
        => new LazyOperation(() =>
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
        => new LazyOperation<S>(() =>
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
        => new LazyOperation(() =>
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
        => new LazyOperation<S>(() =>
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
        => new LazyOperation<R>(() =>
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
    public static class LazyOp
    {
        public static LazyOperation Try(Action operation) => new LazyOperation(operation);
        public static LazyOperation Try(Func<IOperation> operation) => new LazyOperation(() => operation().Resolve());

        public static LazyOperation<R> Try<R>(Func<R> operation) => new LazyOperation<R>(operation);
        public static LazyOperation<R> Try<R>(Func<IOperation<R>> operation) => new LazyOperation<R>(() => operation().Resolve());

        public static LazyOperation Fail(Exception ex) => new LazyOperation(() => { throw ex; });
        public static LazyOperation<R> Fail<R>(Exception ex) => new LazyOperation<R>(() => { throw ex; });
    }
    #endregion
}
