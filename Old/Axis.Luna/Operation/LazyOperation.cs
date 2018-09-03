using Axis.Luna.Extensions;
using System;
using System.Diagnostics;

namespace Axis.Luna.Operation
{
    [DebuggerStepThrough]
    public class LazyOperation : IOperation
    {
        private Exception _exception;
        private Action _operation;

        public bool? Succeeded { get; set; }
        
        public LazyOperation(Action operation)
        {
            if (operation == null) throw new Exception("null argument");
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

                    throw;
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
            }
            catch(Exception e)
            {
                error?.Invoke(e);
                throw;
            }
            continuation?.Invoke();
        });

        public IOperation<R> Then<R>(Func<R> continuation, Action<Exception> error = null)
        => new LazyOperation<R>(() =>
        {
            try
            {
                Resolve();
            }
            catch(Exception e)
            {
                error?.Invoke(e);
                throw;
            }
            return continuation == null ? default(R) : continuation.Invoke();
        });


        public IOperation Then(Func<IOperation> continuation, Action<Exception> error = null)
        => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }

            continuation
                ?.Invoke()
                ?.Resolve();
        });

        public IOperation<S> Then<S>(Func<IOperation<S>> continuation, Action<Exception> error = null)
        => new LazyOperation<S>(() =>
        {
            try
            {
                Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }

            var innerOp = continuation?.Invoke();
            return innerOp == null ? default(S) : innerOp.Resolve();
        });


        public IOperation ContinueWith(Action<IOperation> continuation) => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            continuation?.Invoke(this);
        });

        public IOperation<R> ContinueWith<R>(Func<IOperation, R> continuation) => new LazyOperation<R>(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            return continuation == null ? default(R) : continuation(this);
        });

        public IOperation ContinueWith(Func<IOperation, IOperation> continuation) => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            continuation
                ?.Invoke(this)
                ?.Resolve();
        });

        public IOperation<S> ContinueWith<S>(Func<IOperation, IOperation<S>> continuation) => new LazyOperation<S>(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            var innerOp = continuation?.Invoke(this);
            return innerOp == null ? default(S) : innerOp.Resolve();
        });


        public IOperation Finally(Action @finally)
        => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            finally
            {
                @finally?.Invoke();
            }
        });
        #endregion
    }

    [DebuggerStepThrough]
    public class LazyOperation<R> : IOperation<R>
    {
        private Exception _exception;
        private Func<R> _operation;
        private R _result;

        public bool? Succeeded { get; set; }

        public R Result => Succeeded == true ? Resolve() : default(R);


        public LazyOperation(Func<R> operation)
        {
            if (operation == null) throw new Exception("null argument");

            _operation = operation;
        }

        public LazyOperation(Lazy<R> lazy)
        : this(() => lazy.Value)
        {
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

                    throw;
                }
            }

            return _result;
        }


        #region Continuations
        public IOperation Then(Action<R> continuation, Action<Exception> error = null)
        => new LazyOperation(() =>
        {
            R _r;
            try
            {
                _r = Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }
            continuation?.Invoke(_r);
        });

        public IOperation<S> Then<S>(Func<R, S> continuation, Action<Exception> error = null)
        => new LazyOperation<S>(() =>
        {
            R _r;
            try
            {
                _r = Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }

            return continuation == null ? default(S) : continuation.Invoke(_r);
        });

        public IOperation Then(Func<R, IOperation> continuation, Action<Exception> error = null)
        => new LazyOperation(() =>
        {
            R _r;
            try
            {
                _r = Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }

            continuation
                ?.Invoke(_r)
                ?.Resolve();
        });

        public IOperation<S> Then<S>(Func<R, IOperation<S>> continuation, Action<Exception> error = null)
        => new LazyOperation<S>(() =>
        {
            R _r;
            try
            {
                _r = Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw;
            }

            var innerOp = continuation?.Invoke(_r);
            return innerOp == null ? default(S) : innerOp.Resolve();
        });


        public IOperation ContinueWith(Action<IOperation<R>> continuation) => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            continuation?.Invoke(this);
        });

        public IOperation<S> ContinueWith<S>(Func<IOperation<R>, S> continuation) => new LazyOperation<S>(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            return continuation == null ? default(S) : continuation.Invoke(this);
        });

        public IOperation ContinueWith(Func<IOperation<R>, IOperation> continuation) => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            continuation
                ?.Invoke(this)
                ?.Resolve();
        });

        public IOperation<S> ContinueWith<S>(Func<IOperation<R>, IOperation<S>> continuation) => new LazyOperation<S>(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            var innerOp = continuation?.Invoke(this);
            return innerOp == null ? default(S) : innerOp.Resolve();
        });


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
    [DebuggerStepThrough]
    public static class LazyOp
    {
        public static LazyOperation Try(Action operation) => new LazyOperation(operation);
        public static LazyOperation Try(Func<IOperation> operation) => new LazyOperation(() => operation().Resolve());

        public static LazyOperation<R> Try<R>(Func<R> operation) => new LazyOperation<R>(operation);
        public static LazyOperation<R> Try<R>(Func<IOperation<R>> operation) => new LazyOperation<R>(() => operation().Resolve());
        public static LazyOperation<R> Try<R>(Func<Lazy<R>> operation) => new LazyOperation<R>(operation());
        public static LazyOperation<R> Try<R>(Lazy<R> operation) => new LazyOperation<R>(operation);

        public static LazyOperation Fail(Exception ex) => new LazyOperation(() => { throw new Exception("See Inner Exception", ex); });
        public static LazyOperation<R> Fail<R>(Exception ex) => new LazyOperation<R>(() => { throw new Exception("See Inner Exception", ex); });


        #region Lazy<R> helpers
        public static LazyOperation Then<R>(this Lazy<R> lazy, Action<R> continuation, Action<Exception> error = null)
        => new LazyOperation<R>(lazy).Then(continuation, error).Cast<LazyOperation>();

        public static LazyOperation<S> Then<R, S>(this Lazy<R> lazy, Func<R, S> continuation, Action<Exception> error = null)
        => new LazyOperation<R>(lazy).Then(continuation, error).Cast<LazyOperation<S>>();

        public static LazyOperation Then<R>(this Lazy<R> lazy, Func<R, IOperation> continuation, Action<Exception> error = null)
        => new LazyOperation<R>(lazy).Then(continuation, error).Cast<LazyOperation>();

        public static LazyOperation<S> Then<R, S>(this Lazy<R> lazy, Func<R, IOperation<S>> continuation, Action<Exception> error = null)
        => new LazyOperation<R>(lazy).Then(continuation, error).Cast<LazyOperation<S>>();


        public static LazyOperation ContinueWith<R>(this Lazy<R> lazy, Action<IOperation<R>> continuation)
        => new LazyOperation<R>(lazy).ContinueWith(continuation).Cast<LazyOperation>();

        public static LazyOperation<S> ContinueWith<R, S>(this Lazy<R> lazy, Func<IOperation<R>, S> continuation)
        => new LazyOperation<R>(lazy).ContinueWith(continuation).Cast<LazyOperation<S>>();

        public static LazyOperation ContinueWith<R>(this Lazy<R> lazy, Func<IOperation<R>, IOperation> continuation)
        => new LazyOperation<R>(lazy).ContinueWith(continuation).Cast<LazyOperation>();

        public static LazyOperation<S> ContinueWith<R, S>(this Lazy<R> lazy, Func<IOperation<R>, IOperation<S>> continuation)
        => new LazyOperation<R>(lazy).ContinueWith(continuation).Cast<LazyOperation<S>>();


        public static LazyOperation<R> Finally<R>(this Lazy<R> lazy, Action @finally)
        => new LazyOperation<R>(lazy).Finally(@finally).Cast<LazyOperation<R>>();
        #endregion
    }
    #endregion
}
