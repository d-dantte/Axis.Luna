using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Lazy
{
    //[DebuggerStepThrough]
    public class LazyOperation<R> : Operation<R>
    {
        private OperationError _error;
        private LazyAwaiter<R> _awaiter;

        internal LazyOperation(Func<R> func)
        {
            if (func == null) throw new NullReferenceException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter<R>(new Lazy<R>(func, true));
        }

        internal LazyOperation(Lazy<R> lazy, Func<Task> rollBack = null)
        {
            _awaiter = new LazyAwaiter<R>(lazy ?? throw new NullReferenceException("Invalid Lazy factory supplied"));
        }

        public override bool? Succeeded => !_awaiter.IsCompleted ? null : (bool?)(_error == null);


        public override IAwaiter<R> GetAwaiter() => _awaiter;

        public override OperationError Error => _error;

        public override R Resolve()
        {
            if (_error?.GetException() != null)
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

            try
            {
                return _awaiter.GetResult();
            }
            catch (OperationException oe)
            {
                _error = oe.Error;
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

                //never reached
                throw oe;
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    message: e.Message,
                    code: "GeneralError",
                    exception: e);

                throw;
            }
        }


        public static implicit operator LazyOperation<R>(Func<R> func) => new LazyOperation<R>(func);

        public static implicit operator LazyOperation<R>(Lazy<R> lazy) => new LazyOperation<R>(lazy);
    }


    public class LazyOperation : Operation
    {
        private OperationError _error;
        private LazyAwaiter _awaiter;

        internal LazyOperation(Action action)
        {
            if (action == null) throw new NullReferenceException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter(
                new Lazy<object>(
                    isThreadSafe: true,
                    valueFactory: () =>
                    {
                        action.Invoke();
                        return true;
                    }));
        }

        public override bool? Succeeded => !_awaiter.IsCompleted ? null : (bool?)(_error == null);

        public override OperationError Error => _error;

        public override IAwaiter GetAwaiter() => _awaiter;

        public override void Resolve()
        {
            if (_error?.GetException() != null)
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

            try
            {
                _awaiter.GetResult();
            }
            catch (OperationException oe)
            {
                _error = oe.Error;
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

                //never reached
                throw oe;
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    message: e.Message,
                    code: "GeneralError",
                    exception: e);

                throw;
            }
        }

        
        public static implicit operator LazyOperation(Action action) => new LazyOperation(action);
    }
}
