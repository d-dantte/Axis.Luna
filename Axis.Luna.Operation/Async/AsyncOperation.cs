using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Async
{
    public class AsyncOperation<R> : Operation<R>
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
                _task = Task.Run(() =>
                {
                    var task = taskProducer?.Invoke();
                    if (task.Status == TaskStatus.Created)
                        task.Start();

                    return task;
                });

                _taskAwaiter = new AsyncAwaiter<R>(_task);
            }

            #region Exceptions thrown from the producer
            catch (OperationException oe)
            {
                _error = oe.Error;
                _task = Task.FromException<R>(_error.GetException());
                _taskAwaiter = new AsyncAwaiter<R>(_task);
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    code: "GeneralError",
                    message: e.Message,
                    exception: e);

                _task = Task.FromException<R>(e);
                _taskAwaiter = new AsyncAwaiter<R>(_task);
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

            _taskAwaiter = new AsyncAwaiter<R>(_task);
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

        /// <summary>
        /// Resolves this task synchroniously.
        /// 
        /// <para>
        /// NOTE: if the encapsulated task was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public override R Resolve()
        {
            if (_error != null)
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

            try
            {
                return _taskAwaiter.GetResult();
            }
            catch (OperationException oe)
            {
                _error = oe.Error;
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

                //never reached
                throw;
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

        public static implicit operator AsyncOperation<R>(Func<Task<R>> func) => new AsyncOperation<R>(func);

        public static implicit operator AsyncOperation<R>(Task<R> task) => new AsyncOperation<R>(task);
    }
    
    public class AsyncOperation : Operation
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
                _task = Task.Run(() =>
                {
                    var task = taskProducer?.Invoke();
                    if (task.Status == TaskStatus.Created)
                        task.Start();

                    return task;
                });

                _taskAwaiter = new AsyncAwaiter(_task);
            }

            #region Exceptions thrown from the producer
            catch (OperationException oe)
            {
                _error = oe.Error;
                _task = Task.FromException(_error.GetException());
                _taskAwaiter = new AsyncAwaiter(_task);
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    code: "GeneralError",
                    message: e.Message,
                    exception: e);

                _task = Task.FromException(e);
                _taskAwaiter = new AsyncAwaiter(_task);
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

            _taskAwaiter = new AsyncAwaiter(_task);
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

        /// <summary>
        /// Resolves this task synchroniously.
        /// 
        /// <para>
        /// NOTE: if the encapsulated task was created with a synchronization context in effect,
        /// Calling "Resolve" will result in a deadlock.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public override void Resolve()
        {
            if (_error != null)
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

            try
            {
                _taskAwaiter.GetResult();
            }
            catch (OperationException oe)
            {
                _error = oe.Error;
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

                //never reached
                throw;
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    code: "GeneralError",
                    message: e.Message,
                    exception: e);

                throw;
            }
        }


        public static implicit operator AsyncOperation(Func<Task> func) => new AsyncOperation(func);

        public static implicit operator AsyncOperation(Task task) => new AsyncOperation(task);
    }
}
