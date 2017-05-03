using System;

namespace Axis.Luna.Operation
{
    public class ResolvedOperation : IOperation
    {
        private Exception _exception;

        public bool? Succeeded { get; set; }

        public ResolvedOperation(Action operation)
        {
            if (operation == null) throw new Exception("null argument");

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
            }
            catch (Exception e)
            {
                error?.Invoke(e);
                throw e;
            }

            continuation.Invoke();
        });

        public IOperation<R> Then<R>(Func<R> continuation, Action<Exception> error = null)
        => new ResolvedOperation<R>(() =>
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

            return continuation.Invoke();
        });


        public IOperation Then(Func<IOperation> continuation, Action<Exception> error = null)
        => new ResolvedOperation(() =>
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
        => new ResolvedOperation<S>(() =>
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


        public IOperation ContinueWith(Action<IOperation> continuation) => new ResolvedOperation(() => continuation(this));

        public IOperation<R> ContinueWith<R>(Func<IOperation, R> continuation) => new ResolvedOperation<R>(() => continuation(this));

        public IOperation ContinueWith(Func<IOperation, IOperation> continuation) => new ResolvedOperation(() => continuation(this).Resolve());

        public IOperation<S> ContinueWith<S>(Func<IOperation, IOperation<S>> continuation) => new ResolvedOperation<S>(() => continuation(this).Resolve());
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
            if (operation == null) throw new Exception("null argument");

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
        => new ResolvedOperation<S>(() =>
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
        => new ResolvedOperation(() =>
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
        => new ResolvedOperation<S>(() =>
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



        public IOperation ContinueWith(Action<IOperation<R>> continuation) => new ResolvedOperation(() => continuation(this));

        public IOperation<S> ContinueWith<S>(Func<IOperation<R>, S> continuation) => new ResolvedOperation<S>(() => continuation(this));


        public IOperation ContinueWith(Func<IOperation<R>, IOperation> continuation) => new ResolvedOperation(() => continuation(this).Resolve());

        public IOperation<S> ContinueWith<S>(Func<IOperation<R>, IOperation<S>> continuation) => new ResolvedOperation<S>(() => continuation(this).Resolve());
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
