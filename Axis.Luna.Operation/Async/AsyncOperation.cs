using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Async
{
    public class AsyncOperation<Result> : IOperation<Result>
    {
        private Exception _exception;
        private Task<Result> _task;
        private AsyncAwaiter<Result> _taskAwaiter;


        internal AsyncOperation(Func<Task<Result>> task)
        {
            try
            {
                _task = task?.Invoke() ?? throw new NullReferenceException("Invalid delegate supplied");
                if (_task.Status == TaskStatus.Created) _task.Start();

                _taskAwaiter = new AsyncAwaiter<Result>(_task.GetAwaiter());
            }
            catch(Exception e)
            {
                _exception = e;
                _task = Task.FromException<Result>(e);
                _taskAwaiter = new AsyncAwaiter<Result>(_task.GetAwaiter());
            }
        }

        internal AsyncOperation(Task<Result> task)
        {
            _task = task ?? throw new NullReferenceException("Invalid task upplied");
            if (_task.Status == TaskStatus.Created) _task.Start();

            _taskAwaiter = new AsyncAwaiter<Result>(task.GetAwaiter());
        }


        public bool? Succeeded
        {
            get
            {
                switch(_task.Status)
                {
                    case TaskStatus.RanToCompletion: return true;

                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled: return false;

                    default: return null;
                }
            }
        }

        public IAwaiter<Result> GetAwaiter() => _taskAwaiter;

        public Exception GetException() => _exception;

        internal Task<Result> GetTask() => _task;

        public Result Resolve()
        {
            if (_exception != null)
                ExceptionDispatchInfo.Capture(_exception).Throw();

            try
            {
                return _task.Result;
            }
            catch(Exception e)
            {
                _exception = e;
                throw;
            }
        }
    }


    public class AsyncOperation: IOperation
    {
        private Exception _exception;
        private Task _task;
        private AsyncAwaiter _taskAwaiter;

        internal AsyncOperation(Func<Task> task)
        {
            _task = task?.Invoke() ?? throw new NullReferenceException("Invalid delegate supplied");
            if (_task.Status == TaskStatus.Created) _task.Start();

            _taskAwaiter = new AsyncAwaiter(_task.GetAwaiter());
        }

        internal AsyncOperation(Task task)
        {
            try
            {
                _task = task ?? throw new NullReferenceException("Invalid task upplied");
                _taskAwaiter = new AsyncAwaiter(task.GetAwaiter());
            }
            catch (Exception e)
            {
                _exception = e;
                _task = Task.FromException(e);
                _taskAwaiter = new AsyncAwaiter(_task.GetAwaiter());
            }
        }


        public bool? Succeeded
        {
            get
            {
                switch (_task.Status)
                {
                    case TaskStatus.RanToCompletion: return true;

                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled: return false;

                    default: return null;
                }
            }
        }

        public IAwaiter GetAwaiter() => _taskAwaiter;

        public Exception GetException() => _exception;

        internal Task GetTask() => _task;

        public void Resolve()
        {
            if (_exception != null)
                ExceptionDispatchInfo.Capture(_exception).Throw();

            try
            {
                _task.GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _exception = e;
                throw;
            }
        }
    }
}
