using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Async
{

    public class AsyncOperation<R> : Operation<R>
    {
        private OperationError _error;
        private Task<R> _task;
        private AsyncAwaiter<R> _taskAwaiter;


        internal AsyncOperation(Func<Task<R>> taskProducer, Func<Task> rollBack = null)
        {
            if (taskProducer == null)
                throw new NullReferenceException("Invalid Task Producer Supplied");

            var cxt = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);

                _task = taskProducer?.Invoke();
                if (_task.Status == TaskStatus.Created) _task.Start();
                _taskAwaiter = new AsyncAwaiter<R>(_task.GetAwaiter());

                if (rollBack != null) _task.ContinueWith(_t =>
                {
                    if(_t.Status != TaskStatus.RanToCompletion)
                    {
                        var rollbackTask = rollBack.Invoke();
                        if (rollbackTask.Status == TaskStatus.Created) rollbackTask.Start(); // or .RunSynchroniously() ?
                        rollbackTask.Wait(); //wait till it finishes
                    }
                });
            }

            #region Exceptions thrown from the producer
            catch (OperationException oe)
            {
                _error = oe.Error;
                _task = Task.FromException<R>(_error.GetException());
                _taskAwaiter = new AsyncAwaiter<R>(_task.GetAwaiter());
            }
            catch (Exception e)
            {
                _error = new OperationError(e)
                {
                    Code = "GeneralError",
                    Message = e.Message
                };
                _task = Task.FromException<R>(e);
                _taskAwaiter = new AsyncAwaiter<R>(_task.GetAwaiter());
            }
            #endregion

            finally
            {
                SynchronizationContext.SetSynchronizationContext(cxt);
            }
        }

        internal AsyncOperation(Task<R> task, Func<Task> rollBack = null)
        {
            var cxt = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                _task = task ?? throw new NullReferenceException("Invalid task supplied");
                if (_task.Status == TaskStatus.Created) _task.Start();

                _taskAwaiter = new AsyncAwaiter<R>(task.GetAwaiter());

                if (rollBack != null) _task.ContinueWith(_t =>
                {
                    if (_t.Status != TaskStatus.RanToCompletion)
                    {
                        var rollbackTask = rollBack.Invoke();
                        if (rollbackTask.Status == TaskStatus.Created) rollbackTask.Start(); // or .RunSynchroniously() ?
                        rollbackTask.Wait(); //wait till it finishes
                    }
                });
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(cxt);
            }
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

        public override R Resolve()
        {
            if (_error?.GetException() != null)
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

            try
            {
                return _taskAwaiter.GetResult();
            }
            catch(OperationException oe)
            {
                _error = oe.Error;
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

                //never reached
                throw oe;
            }
            catch (Exception e)
            {
                _error = new OperationError(e)
                {
                    Message = e.Message,
                    Code = "GeneralError"
                };
                throw;
            }
        }
    }


    public class AsyncOperation : Operation
    {
        private OperationError _error;
        private Task _task;
        private AsyncAwaiter _taskAwaiter;

        internal AsyncOperation(Func<Task> taskProducer, Func<Task> rollBack = null)
        {
            if (taskProducer == null)
                throw new NullReferenceException("Invalid Task Producer Supplied");

            var cxt = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);

                _task = taskProducer?.Invoke() ?? throw new NullReferenceException("Invalid delegate supplied");
                if (_task.Status == TaskStatus.Created) _task.Start();
                _taskAwaiter = new AsyncAwaiter(_task.GetAwaiter());

                if (rollBack != null) _task.ContinueWith(_t =>
                {
                    if (_t.Status != TaskStatus.RanToCompletion)
                    {
                        var rollbackTask = rollBack.Invoke();
                        if (rollbackTask.Status == TaskStatus.Created) rollbackTask.Start(); // or .RunSynchroniously() ?
                        rollbackTask.Wait(); //wait till it finishes
                    }
                });
            }

            #region Exceptions thrown from the producer
            catch (OperationException oe)
            {
                _error = oe.Error;
                _task = Task.FromException(_error.GetException());
                _taskAwaiter = new AsyncAwaiter(_task.GetAwaiter());
            }
            catch (Exception e)
            {
                _error = new OperationError(e)
                {
                    Code = "GeneralError",
                    Message = e.Message
                };
                _task = Task.FromException(e);
                _taskAwaiter = new AsyncAwaiter(_task.GetAwaiter());
            }
            #endregion

            finally
            {
                SynchronizationContext.SetSynchronizationContext(cxt);
            }
        }

        internal AsyncOperation(Task task, Func<Task> rollBack = null)
        {
            var cxt = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                _task = task ?? throw new NullReferenceException("Invalid task upplied");
                _taskAwaiter = new AsyncAwaiter(task.GetAwaiter());

                if (rollBack != null) _task.ContinueWith(_t =>
                {
                    if (_t.Status != TaskStatus.RanToCompletion)
                    {
                        var rollbackTask = rollBack.Invoke();
                        if (rollbackTask.Status == TaskStatus.Created) rollbackTask.Start(); // or .RunSynchroniously() ?
                        rollbackTask.Wait(); //wait till it finishes
                    }
                });
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(cxt);
            }
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

        internal Task GetTask() => _task;

        public override void Resolve()
        {
            if (_error?.GetException() != null)
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
                throw oe;
            }
            catch (Exception e)
            {
                _error = new OperationError(e)
                {
                    Message = e.Message,
                    Code = "GeneralError"
                };
                throw;
            }
        }
    }
}
