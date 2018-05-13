using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Utils
{
    public static class Operation
    {
        #region Try
        public static IOperation<Result> Try<Result>(Func<Result> func) => new Lazy.LazyOperation<Result>(func);
        public static IOperation Try(Action action) => new Lazy.LazyOperation(action);


        public static IOperation<Result> Try<Result>(Func<Task<Result>> func) => new Async.AsyncOperation<Result>(func);
        public static IOperation Try(Func<Task> action) => new Async.AsyncOperation(action);

        public static IOperation<Result> Try<Result>(Task<Result> task) => new Async.AsyncOperation<Result>(task);
        public static IOperation Try(Task task) => new Async.AsyncOperation(task);
        

        public static IOperation<Result> Try<Result>(Func<IOperation<Result>> op) => op.Invoke();
        public static IOperation Try(Func<IOperation> op) => op.Invoke();
        #endregion


        #region Fail
        public static IOperation Fail(Exception exception = null) 
        => new Lazy.LazyOperation(() =>
        {
            ExceptionDispatchInfo
                .Capture(exception ?? new InvalidOperationException())
                .Throw();
        });

        public static IOperation<Result> Fail<Result>(Exception exception = null) 
        => new Lazy.LazyOperation<Result>(() =>
        {
            ExceptionDispatchInfo
                .Capture(exception ?? new InvalidOperationException())
                .Throw();

            //NOTE! this statement is never reached
            return default(Result);
        });
        #endregion


        #region Then
        public static IOperation Then(this IOperation prev, Action next)
        {
            if (prev.Succeeded == false) return prev;
            else return new Lazy.LazyOperation(() =>
            {
                prev.Resolve();
                next.Invoke();
            });
        }
        public static IOperation Then<In>(this IOperation<In> prev, Action next)
        {
            if (prev.Succeeded == false) return Fail(prev.GetException());
            else return new Lazy.LazyOperation(() =>
            {
                prev.Resolve();
                next.Invoke();
            });
        }
        public static IOperation Then<In>(this IOperation<In> prev, Action<In> next)
        {
            if (prev.Succeeded == false) return Fail(prev.GetException());
            else return new Lazy.LazyOperation(() =>
            {
                var _in = prev.Resolve();
                next.Invoke(_in);
            });
        }


        public static IOperation<Out> Then<Out>(this IOperation prev, Func<Out> next)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return new Lazy.LazyOperation<Out>(() =>
            {
                prev.Resolve();
                return next.Invoke();
            });
        }
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<Out> next)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return new Lazy.LazyOperation<Out>(() =>
            {
                prev.Resolve();
                return next.Invoke();
            });
        }
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<In, Out> next)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return new Lazy.LazyOperation<Out>(() =>
            {
                var _in = prev.Resolve();
                return next.Invoke(_in);
            });
        }        

        
        public static IOperation Then(this IOperation prev, Func<Task> taskProducer)
        {
            if (prev.Succeeded == false) return Fail(prev.GetException());
            else return new Async.AsyncOperation(Task.Run(() =>
            {
                prev.Resolve();
                taskProducer().GetAwaiter().GetResult();
            }));
        }
        public static IOperation Then<In>(this IOperation<In> prev, Func<Task> taskProducer)
        {
            if (prev.Succeeded == false) return Fail(prev.GetException());
            else return new Async.AsyncOperation(Task.Run(() =>
            {
                var _in = prev.Resolve();
                taskProducer().GetAwaiter().GetResult();
            }));
        }
        public static IOperation Then<In>(this IOperation<In> prev, Func<In, Task> taskProducer)
        {
            if (prev.Succeeded == false) return Fail(prev.GetException());
            else return new Async.AsyncOperation(Task.Run(() =>
            {
                var _in = prev.Resolve();
                taskProducer(_in).GetAwaiter().GetResult();
            }));
        }

        
        public static IOperation<Out> Then<Out>(this IOperation prev, Func<Task<Out>> taskProducer)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return new Async.AsyncOperation<Out>(Task.Run(() =>
            {
                //using a task here instead of awaiting the previous operation because awaiting a lazy operation causes it to resolve,
                //meaning this method will block till all of the previous operations have resolved - something we do not want happening here.
                prev.Resolve();
                return taskProducer().GetAwaiter().GetResult();
            }));
        }
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<Task<Out>> taskProducer)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return new Async.AsyncOperation<Out>(Task.Run(() =>
            {
                //using a task here instead of awaiting the previous operation because awaiting a lazy operation causes it to resolve,
                //meaning this method will block till all of the previous operations have resolved - something we do not want happening here.
                var _in = prev.Resolve();
                return taskProducer().GetAwaiter().GetResult();
            }));
        }
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Func<In, Task<Out>> taskProducer)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return new Async.AsyncOperation<Out>(Task.Run(() =>
            {
                //using a task here instead of awaiting the previous operation because awaiting a lazy operation causes it to resolve,
                //meaning this method will block till all of the previous operations have resolved - something we do not want happening here.
                var _in = prev.Resolve();
                return taskProducer(_in).GetAwaiter().GetResult();
            }));
        }

        
        public static IOperation Then(this IOperation prev, IOperation next)
        {
            if (prev.Succeeded == false) return prev;
            else return prev.Then(async () => await next);
        }
        public static IOperation Then<In>(this IOperation<In> prev, IOperation next)
        {
            if (prev.Succeeded == false) return Fail(prev.GetException());
            else return prev.Then(async () => await next);
        }


        public static IOperation<Out> Then<Out>(this IOperation prev, IOperation<Out> next)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return prev.Then(async () => await next);
        }
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, IOperation<Out> next)
        {
            if (prev.Succeeded == false) return Operation.Fail<Out>(prev.GetException());
            else return prev.Then(async () => await next);
        }

                
        public static IOperation Then(this IOperation prev, Task task)
        {
            if (prev.Succeeded == false) return Fail(prev.GetException());
            else return new Async.AsyncOperation(Task.Run(() =>
            {
                prev.Resolve();
                task.GetAwaiter().GetResult();
            }));
        }
        public static IOperation Then<In>(this IOperation<In> prev, Task task)
        {
            if (prev.Succeeded == false) return Fail(prev.GetException());
            else return new Async.AsyncOperation(Task.Run(() =>
            {
                prev.Resolve();
                task.GetAwaiter().GetResult();
            }));
        }
        

        public static IOperation<Out> Then<Out>(this IOperation prev, Task<Out> task)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return new Async.AsyncOperation<Out>(Task.Run(() =>
            {
                prev.Resolve();
                return task.GetAwaiter().GetResult();
            }));
        }
        public static IOperation<Out> Then<In, Out>(this IOperation<In> prev, Task<Out> task)
        {
            if (prev.Succeeded == false) return Fail<Out>(prev.GetException());
            else return new Async.AsyncOperation<Out>(Task.Run(() =>
            {
                prev.Resolve();
                return task.GetAwaiter().GetResult();
            }));
        }
        #endregion
    }
}