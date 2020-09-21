using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Async
{
    public class AsyncOperation<R> : Operation<R>, IResolvable<Task<R>>
    {
        private OperationError _error;
        private readonly Task<R> _task;
        private readonly AsyncAwaiter<R> _taskAwaiter;

        /// <summary>
        /// Creates a new AsyncOperation.
        /// 
        /// <para>
        /// NOTE: if the task producer returns a task that was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="taskProducer">A delegate that returns the task that this operation encapsulates</param>
        internal AsyncOperation(Func<Task<R>> taskProducer)
        {
            if (taskProducer == null)
                throw new NullReferenceException("Invalid Task Producer Supplied");

            try
            {
                _task = taskProducer.Invoke();

                if (_task.Status == TaskStatus.Created)
                    _task.Start();

                _taskAwaiter = new AsyncAwaiter<R>(_task, SetError);
            }

            #region Exceptions thrown from the producer
            catch (OperationException oe)
            {
                _error = oe.Error;
                _task = Task.FromException<R>(_error.GetException());
                _taskAwaiter = new AsyncAwaiter<R>(_task, SetError);
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    code: "GeneralError",
                    message: e.Message,
                    exception: e);

                _task = Task.FromException<R>(e);
                _taskAwaiter = new AsyncAwaiter<R>(_task, SetError);
            }
            #endregion
        }

        /// <summary>
        /// Creates a new AsyncOperation.
        /// 
        /// <para>
        /// NOTE: if the task supplied was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="task">The task that this operation encapsulates</param>
        internal AsyncOperation(Task<R> task)
        {
            _task = task ?? throw new NullReferenceException("Invalid task supplied");

            if (_task.Status == TaskStatus.Created)
                _task.Start();

            _taskAwaiter = new AsyncAwaiter<R>(_task, SetError);
        }
        
        public override bool? Succeeded
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

        public override OperationError Error => _error;
        
        public override IAwaiter<R> GetAwaiter() => _taskAwaiter;

        internal Task<R> GetTask() => _task;

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

        #region Resolvable
        public Task<R> ResolveSafely() => _task;

        public Task<R> Resolve() => _task;

        public bool TryResolve(out Task<R> result, out OperationError error)
        {
            error = null;
            result = _task;
            return true;
        }
        #endregion

        public static implicit operator AsyncOperation<R>(Func<Task<R>> func) => new AsyncOperation<R>(func);

        public static implicit operator AsyncOperation<R>(Task<R> task) => new AsyncOperation<R>(task);
    }
    
    public class AsyncOperation : Operation, IResolvable<Task>
    {
        private OperationError _error;
        private readonly Task _task;
        private readonly AsyncAwaiter _taskAwaiter;

        /// <summary>
        /// Creates a new AsyncOperation.
        /// 
        /// <para>
        /// NOTE: if the task producer returns a task that was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="taskProducer">A delegate that returns the task that this operation encapsulates</param>
        internal AsyncOperation(Func<Task> taskProducer)
        {
            if (taskProducer == null)
                throw new NullReferenceException("Invalid Task Producer Supplied");

            try
            {
                _task = taskProducer?.Invoke();

                if (_task.Status == TaskStatus.Created)
                    _task.Start();

                _taskAwaiter = new AsyncAwaiter(_task, SetError);
            }

            #region Exceptions thrown from the producer
            catch (OperationException oe)
            {
                _error = oe.Error;
                _task = Task.FromException(_error.GetException());
                _taskAwaiter = new AsyncAwaiter(_task, SetError);
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    code: "GeneralError",
                    message: e.Message,
                    exception: e);

                _task = Task.FromException(e);
                _taskAwaiter = new AsyncAwaiter(_task, SetError);
            }
            #endregion
        }
        
        /// <summary>
        /// Creates a new AsyncOperation.
        /// 
        /// <para>
        /// NOTE: if the task supplied was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <param name="task">The task that this operation encapsulates</param>
        internal AsyncOperation(Task task)
        {
            _task = task ?? throw new NullReferenceException("Invalid task supplied");

            if (_task.Status == TaskStatus.Created)
                _task.Start();

            _taskAwaiter = new AsyncAwaiter(_task, SetError);
        }


        public override bool? Succeeded
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

        public override IAwaiter GetAwaiter() => _taskAwaiter;

        public override OperationError Error => _error;

        public Task GetTask() => _task;

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


        #region Resolvable
        public Task ResolveSafely() => _task;

        public Task Resolve() => _task;

        public bool TryResolve(out Task result, out OperationError error)
        {
            error = null;
            result = _task;
            return true;
        }
        #endregion


        public static implicit operator AsyncOperation(Func<Task> func) => new AsyncOperation(func);

        public static implicit operator AsyncOperation(Task task) => new AsyncOperation(task);
    }
}
