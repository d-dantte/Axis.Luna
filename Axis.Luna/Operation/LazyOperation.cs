using System;


namespace Axis.Luna.Operation
{
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
            }
            catch(Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
            continuation.Invoke();
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
                throw e;
            }
            return continuation.Invoke();
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
                throw e;
            }

            var innerOp = continuation.Invoke();
            innerOp.Resolve();
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
                throw e;
            }

            var innerOp = continuation.Invoke();
            return innerOp.Resolve();
        });


        public IOperation ContinueWith(Action<IOperation> continuation) => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            continuation(this);
        });

        public IOperation<R> ContinueWith<R>(Func<IOperation, R> continuation) => new LazyOperation<R>(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            return continuation(this);
        });

        public IOperation ContinueWith(Func<IOperation, IOperation> continuation) => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            continuation(this).Resolve();
        });

        public IOperation<S> ContinueWith<S>(Func<IOperation, IOperation<S>> continuation) => new LazyOperation<S>(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            return continuation(this).Resolve();
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
            if (operation == null) throw new Exception("null argument");

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
            R _r;
            try
            {
                _r = Resolve();
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw e;
            }
            continuation.Invoke(_r);
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
                throw e;
            }

            return continuation.Invoke(_r);
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
                throw e;
            }

            var innerOp = continuation.Invoke(_r);
            innerOp.Resolve();
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
                throw e;
            }

            var innerOp = continuation.Invoke(_r);
            return innerOp.Resolve();
        });



        public IOperation ContinueWith(Action<IOperation<R>> continuation) => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            continuation(this);
        });

        public IOperation<S> ContinueWith<S>(Func<IOperation<R>, S> continuation) => new LazyOperation<S>(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            return continuation(this);
        });

        public IOperation ContinueWith(Func<IOperation<R>, IOperation> continuation) => new LazyOperation(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            continuation(this).Resolve();
        });

        public IOperation<S> ContinueWith<S>(Func<IOperation<R>, IOperation<S>> continuation) => new LazyOperation<S>(() =>
        {
            try
            {
                Resolve();
            }
            catch { }
            return continuation(this).Resolve();
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
