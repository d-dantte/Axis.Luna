using System;
using System.Runtime.ExceptionServices;

namespace Axis.Luna.Operation.Lazy
{
    public class LazyOperation<Result>: IOperation<Result>
    {
        private Exception _exception;
        private LazyAwaiter<Result> _awaiter;

        internal LazyOperation(Func<Result> func)
        {
            var lazy = new Lazy<Result>(func ?? throw new NullReferenceException("Invalid delegate supplied"), true);

            _awaiter = new LazyAwaiter<Result>(lazy);
        }

        internal LazyOperation(Lazy<Result> lazy)
        {
            _awaiter = new LazyAwaiter<Result>(lazy ?? throw new NullReferenceException("Invalid Lazy factory supplied"));
        }

        public bool? Succeeded => !_awaiter.IsCompleted ? null : (bool?)(_exception != null);

        public IAwaiter<Result> GetAwaiter() => _awaiter;

        public Exception GetException() => _exception;

        public Result Resolve()
        {
            if (_exception != null)
                ExceptionDispatchInfo.Capture(GetException()).Throw();

            try
            {
                return _awaiter.GetResult();
            }
            catch (Exception e)
            {
                _exception = e;
                throw;
            }
        }
    }
    

    public class LazyOperation : IOperation
    {
        private Exception _exception;
        private LazyAwaiter _awaiter;

        internal LazyOperation(Action action)
        {
            if (action == null) throw new NullReferenceException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter(new Lazy<object>(() =>
            {
                action.Invoke();
                return true;
            }, 
            true));
        }

        public bool? Succeeded => !_awaiter.IsCompleted ? null : (bool?)(_exception != null);

        public IAwaiter GetAwaiter() => _awaiter;

        public Exception GetException() => _exception;

        public void Resolve()
        {
            if (_exception != null)
                ExceptionDispatchInfo.Capture(_exception).Throw();

            try
            {
                _awaiter.GetResult();
            }
            catch (Exception e)
            {
                _exception = e;
                throw;
            }
        }
    }
}
