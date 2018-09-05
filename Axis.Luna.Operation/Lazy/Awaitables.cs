using System;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Lazy
{

    public struct LazyAwaiter : IAwaiter
    {
        private readonly Lazy<object> _lazy;
        private readonly Func<Task> _rollBack;

        public LazyAwaiter(Lazy<object> lazy, Func<Task> rollBack)
        {
            _lazy = lazy;
            _rollBack = rollBack;
        }

        public bool IsCompleted => true;

        public void GetResult()
        {
            try
            {
                var unused = _lazy.Value;
            }
            catch(Exception e)
            {
                var cxt = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);

                try
                {
                    _rollBack?.Invoke().Wait();
                }
                catch(Exception inner)
                {
                    throw new AggregateException(e, inner);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(cxt);
                }

                throw;
            }
        }

        public void OnCompleted(Action continuation)
        {
            GetResult();
            continuation.Invoke();
        }
    }


    public struct LazyAwaiter<Result> : IAwaiter<Result>
    {
        private readonly Lazy<Result> _lazy;
        private readonly Func<Task> _rollBack;

        public LazyAwaiter(Lazy<Result> lazy, Func<Task> rollBack)
        {
            _lazy = lazy;
            _rollBack = rollBack;
        }

        public bool IsCompleted => true;

        public Result GetResult()
        {
            try
            {
                return _lazy.Value;
            }
            catch (Exception e)
            {
                var cxt = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);

                try
                {
                    _rollBack?.Invoke().Wait();
                }
                catch (Exception inner)
                {
                    throw new AggregateException(e, inner);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(cxt);
                }

                throw;
            }
        }

        public void OnCompleted(Action continuation)
        {
            //resolve
            GetResult();
            continuation.Invoke();
        }
    }
}
