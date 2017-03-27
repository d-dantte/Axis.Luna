using Axis.Luna.MetaTypes;
using System;
using System.Collections.Concurrent;
namespace Axis.Luna.Extensions
{
    using Axis.Luna.MetaTypes;
    using System.Diagnostics;

    public static class OperationExtensions
    {
        internal static Operation Promote(this Operation<@void> generic) => new Operation
        {
            Error = generic.Error,
            Message = generic.Message,
            Succeeded = generic.Succeeded
        };

        #region Synchronious Operation

        [DebuggerHidden] public static Operation Then(this Operation<@void> op, Action<Operation<@void>> action)
        {
            if (op.Succeeded) return Operation.Run(() => action(op));
            else return Operation.Fail(op.GetException());
        }
        [DebuggerHidden] public static Operation Then<In>(this Operation<In> op, Action<Operation<In>> action)
        {
            if (op.Succeeded) return Operation.Run(() => action(op));
            else return Operation.Fail(op.GetException());
        }

        [DebuggerHidden] public static Operation<Out> Then<In, Out>(this Operation<In> op, Func<Operation<In>, Out> func)
        {
            if (op.Succeeded) return Operation.Run(() => func(op));
            else return Operation.Fail<Out>(op.GetException());
        }
        [DebuggerHidden] public static Operation<Out> Then<In, Out>(this Operation<In> op, Func<Operation<In>, Operation<Out>> func)
        {
            if (op.Succeeded) return func(op);
            else return Operation.Fail<Out>(op.GetException());
        }

        [DebuggerHidden] public static Operation Then<In>(this Operation<In> op, Func<Operation<In>, @void> func)
        {
            if (op.Succeeded) return Operation.Run(() => { func(op); });
            else return Operation.Fail(op.GetException());
        }
        [DebuggerHidden] public static Operation Then<In>(this Operation<In> op, Func<Operation<In>, Operation<@void>> func)
        {
            if (op.Succeeded) return func(op).Promote();
            else return Operation.Fail(op.GetException());
        }
        [DebuggerHidden] public static Operation Then<In>(this Operation<In> op, Func<Operation<In>, Operation> func)
        {
            if (op.Succeeded) return func(op);
            else return Operation.Fail(op.GetException());
        }


        [DebuggerHidden] public static Operation<Out> Error<Out>(this Operation<Out> op, Action action)
        {
            if (!op.Succeeded)
            {
                try
                {
                    action();
                }
                catch
                { }
            }

            return op;
        }
        [DebuggerHidden] public static Operation Error(this Operation<@void> op, Action action)
        {
            if (!op.Succeeded)
            {
                try
                {
                    action();
                }
                catch
                { }
            }

            return op.Promote();
        }

        [DebuggerHidden] public static Operation<Out> Error<Out>(this Operation<Out> op, Action<Operation<Out>> func)
        {
            if (!op.Succeeded)
            {
                try
                {
                    func(op);
                }
                catch
                { }
            }

            return op;
        }
        [DebuggerHidden] public static Operation Error(this Operation<@void> op, Action<Operation<@void>> func)
        {
            if (!op.Succeeded)
            {
                try
                {
                    func(op);
                }
                catch
                { }
            }

            return op.Promote();
        }

        /// <summary>
        /// Executes and transforms to a successfull operation instead of propagating a failed operation
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <param name="op"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [DebuggerHidden] public static Operation<Out> Instead<Out>(this Operation<Out> op, Func<Operation<Out>, Out> func)
        {
            if (!op.Succeeded) return Operation.Run(() => func(op));
            return op;
        }

        /// <summary>
        /// Executes and transforms to a successfull operation instead of propagating a failed operation
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <param name="op"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [DebuggerHidden] public static Operation<Out> Instead<Out>(this Operation<Out> op, Func<Operation<Out>, Operation<Out>> func)
        {
            if (!op.Succeeded) return Operation.Run(() => func(op));
            return op;
        }

        [DebuggerHidden] public static Operation Instead(this Operation<@void> op, Func<Operation<@void>, Operation> func)
        {
            if (!op.Succeeded) return Operation.Try(() => func(op));
            else return Operation.Fail(op.GetException());
        }

        [DebuggerHidden] public static Operation Instead(this Operation<@void> op, Action<Operation<@void>> action)
        {
            if (!op.Succeeded) return Operation.Try(() => action(op));
            else return Operation.Fail(op.GetException());
        }

        ///...

        #endregion

        #region Asynchronious AsyncOperation

        [DebuggerHidden] public static AsyncOperation Then(this AsyncOperation<@void> op, Action<AsyncOperation<@void>> action) => new AsyncOperation(() => action(op), op.Task());
        [DebuggerHidden] public static AsyncOperation Then(this AsyncOperation<@void> op, AsyncInfo info, Action<AsyncOperation<@void>> action) => new AsyncOperation(() => action(op), op.Task(), info);

        [DebuggerHidden] public static AsyncOperation<Out> Then<In, Out>(this AsyncOperation<In> op, Func<AsyncOperation<In>, Out> func) => new AsyncOperation<Out>(() => func(op), op.Task());
        [DebuggerHidden] public static AsyncOperation<Out> Then<In, Out>(this AsyncOperation<In> op, AsyncInfo info, Func<AsyncOperation<In>, Out> func) => new AsyncOperation<Out>(() => func(op), op.Task());

        [DebuggerHidden] public static AsyncOperation<Out> Then<In, Out>(this AsyncOperation<In> op, Func<AsyncOperation<In>, AsyncOperation<Out>> func) => func(op);

        [DebuggerHidden] public static AsyncOperation<Out> Then<Out>(this AsyncOperation<@void> op, Func<AsyncOperation<@void>, Out> func) => new AsyncOperation<Out>(() => func(op), op.Task());
        [DebuggerHidden] public static AsyncOperation<Out> Then<Out>(this AsyncOperation<@void> op, AsyncInfo info, Func<AsyncOperation<@void>, Out> func) => new AsyncOperation<Out>(() => func(op), op.Task(), info);

        [DebuggerHidden] public static AsyncOperation<Out> Then<Out>(this AsyncOperation<@void> op, Func<AsyncOperation<@void>, AsyncOperation<Out>> func) => func(op);

        [DebuggerHidden] public static AsyncOperation Then<In>(this AsyncOperation<In> op, AsyncInfo info, Action<AsyncOperation<In>> action) => new AsyncOperation(() => action(op), op.Task(), info);
        [DebuggerHidden] public static AsyncOperation Then<In>(this AsyncOperation<In> op, Action<AsyncOperation<In>> action) => new AsyncOperation(() => action(op), op.Task());


        [DebuggerHidden] public static AsyncOperation<Out> Error<Out>(this AsyncOperation<Out> op, Action action)
        {
            if (op.Succeeded ?? false)
            {
                try
                {
                    action();
                }
                catch
                { }
            }

            return op;
        }

        /// <summary>
        /// Executes and transforms to a successfull AsyncOperation instead of propagating a FailAsynced AsyncOperation
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <param name="op"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [DebuggerHidden] public static AsyncOperation<Out> Instead<Out>(this AsyncOperation<Out> op, Func<AsyncOperation<Out>, Out> func)
        {
            if (!op.Succeeded == true) return new AsyncOperation<Out>(() => func(op));
            return op;
        }
        [DebuggerHidden] public static AsyncOperation<Out> Instead<Out>(this AsyncOperation<Out> op, AsyncInfo info, Func<AsyncOperation<Out>, Out> func)
        {
            if (!op.Succeeded == true) return new AsyncOperation<Out>(() => func(op), info);
            return op;
        }

        /// <summary>
        /// Executes and transforms to a successfull AsyncOperation instead of propagating a FailAsynced AsyncOperation
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <param name="op"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [DebuggerHidden] public static AsyncOperation<Out> Instead<Out>(this AsyncOperation<Out> op, Func<AsyncOperation<Out>, AsyncOperation<Out>> func)
        {
            if (!op.Succeeded == true) func(op);
            return op;
        }

        [DebuggerHidden] public static AsyncOperation Instead(this AsyncOperation<@void> op, Action<AsyncOperation<@void>> action)
        {
            if (!op.Succeeded == true) return new AsyncOperation(() => action(op));
            else return Operation.FailAsync(op.GetException());
        }
        [DebuggerHidden] public static AsyncOperation Instead(this AsyncOperation<@void> op, AsyncInfo info, Action<AsyncOperation<@void>> action)
        {
            if (!op.Succeeded == true) return new AsyncOperation(() => action(op), info);
            else return Operation.FailAsync(op.GetException());
        }

        ///...

        #endregion
    }
}