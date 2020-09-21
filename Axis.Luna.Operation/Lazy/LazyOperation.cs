using System;
using System.Runtime.ExceptionServices;

namespace Axis.Luna.Operation.Lazy
{
    //[DebuggerStepThrough]
    public class LazyOperation<R> : Operation<R>, IResolvable<R>
    {
        private OperationError _error;
        private readonly LazyAwaiter<R> _awaiter;

        internal LazyOperation(Func<R> func)
        {
            if (func == null) throw new NullReferenceException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter<R>(
                new Lazy<R>(func, true),
                SetError);
        }

        internal LazyOperation(Lazy<R> lazy)
        {
            _awaiter = new LazyAwaiter<R>(
                lazy ?? throw new NullReferenceException("Invalid Lazy factory supplied"),
                SetError);
        }

        public override bool? Succeeded => _awaiter.IsSuccessful;


        public override IAwaiter<R> GetAwaiter() => _awaiter;

        public override OperationError Error => _error;

		#region Resolvable
		public R Resolve() => _awaiter.GetResult();

        public R ResolveSafely()
        {
            if (Succeeded == true)
                return _awaiter.GetResult();

            else if (Succeeded == null)
            {
                try
                {
                    return Resolve();
                }
                catch { }
            }

            return default;
        }

        public bool TryResolve(out R result, out OperationError error)
        {
            result = ResolveSafely();

            error = _error;

            return Succeeded.Value;
        }
		#endregion

        private void SetError(Exception e)
        {
            switch(e)
            {
                case OperationException oe:
                    _error = oe.Error;
                    break;

                default:
                    _error = new OperationError(
                        message: e.Message,
                        code: "GeneralError",
                        exception: e);
                    break;
            }
        }

		public static implicit operator LazyOperation<R>(Func<R> func) => new LazyOperation<R>(func);

        public static implicit operator LazyOperation<R>(Lazy<R> lazy) => new LazyOperation<R>(lazy);
    }


    public class LazyOperation : Operation, IResolvable
    {
        private OperationError _error;
        private readonly LazyAwaiter _awaiter;

        internal LazyOperation(Action action)
        {
            if (action == null) throw new NullReferenceException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter(
                errorSetter: SetError,
                lazy: new Lazy<object>(
                    isThreadSafe: true,
                    valueFactory: () =>
                    {
                        action.Invoke();
                        return true;
                    }));
        }

        public override bool? Succeeded => _awaiter.IsSuccessful;

        public override OperationError Error => _error;

        public override IAwaiter GetAwaiter() => _awaiter;

        #region Resolvable
        public void Resolve()
        {
            if (Succeeded == true)
                return;

            else
            {
                _awaiter.GetResult();
            }
        }

        public void ResolveSafely()
        {
            if (Succeeded == null)
            {
                try
                {
                    Resolve();
                }
                catch { }
            }
        }

        public bool TryResolve(out OperationError error)
        {
            ResolveSafely();

            error = _error;

            return Succeeded.Value;
        }
        #endregion

        private void SetError(Exception e)
        {
            switch (e)
            {
                case OperationException oe:
                    _error = oe.Error;
                    break;

                default:
                    _error = new OperationError(
                        message: e.Message,
                        code: "GeneralError",
                        exception: e);
                    break;
            }
        }

        public static implicit operator LazyOperation(Action action) => new LazyOperation(action);
    }
}
