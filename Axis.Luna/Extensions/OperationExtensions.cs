using Axis.Luna.MetaTypes;
using System;
using System.Collections.Concurrent;
namespace Axis.Luna.Extensions
{
    using Axis.Luna.MetaTypes;

    public static class OperationExtensions
    {
        public static Operation<Result> Then<Result>(this Operation<Result> op, Func<Operation<Result>, Result> func)
        {
            if (op.Succeeded) return Operation.Run(() => func(op));
            else return Operation.Fail<Result>(op.Message);
        }
        public static Operation<Result> Then<Result>(this Operation<Operation<Result>> op, Func<Operation<Result>, Result> func)
        {
            if (op.Succeeded && op.Result.Succeeded) return Operation.Run(() => func(op.Result));
            else return Operation.Fail<Result>(op.Message);
        }

        public static Operation<@void> Then(this Operation<@void> op, Action<Operation<@void>> action)
        {
            if (op.Succeeded) return Operation.Run(() => action(op));
            else return Operation.Fail<@void>(op.Message);
        }
        public static Operation<@void> Then(this Operation<Operation<@void>> op, Action<Operation<@void>> action)
        {
            if (op.Succeeded && op.Result.Succeeded) return Operation.Run(() => action(op.Result));
            else return Operation.Fail<@void>(op.Message);
        }


        public static Operation<Out> Then<In, Out>(this Operation<In> op, Func<Operation<In>, Out> func)
        {
            if (op.Succeeded) return Operation.Run(() => func(op));
            else return Operation.Fail<Out>(op.Message);
        }
        public static Operation<Out> Then<In, Out>(this Operation<Operation<In>> op, Func<Operation<In>, Out> func)
        {
            if (op.Succeeded && op.Result.Succeeded) return Operation.Run(() => func(op.Result));
            else return Operation.Fail<Out>(op.Message);
        }

        public static Operation<Out> Then<Out>(this Operation<@void> op, Func<Operation<@void>, Out> func)
        {
            if (op.Succeeded) return Operation.Run(() => func(op));
            else return Operation.Fail<Out>(op.Message);
        }
        public static Operation<Out> Then<Out>(this Operation<Operation<@void>> op, Func<Operation<@void>, Out> func)
        {
            if (op.Succeeded && op.Result.Succeeded) return Operation.Run(() => func(op.Result));
            else return Operation.Fail<Out>(op.Message);
        }

        public static Operation<@void> Then<In>(this Operation<In> op, Action<Operation<In>> action)
        {
            if (op.Succeeded) return Operation.Run(() => action(op));
            else return Operation.Fail<@void>(op.Message);
        }
        public static Operation<@void> Then<In>(this Operation<Operation<In>> op, Action<Operation<In>> action)
        {
            if (op.Succeeded && op.Result.Succeeded) return Operation.Run(() => action(op.Result));
            else return Operation.Fail<@void>(op.Message);
        }


        public static Operation<Out> Error<Out>(this Operation<Out> op, Action action)
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
        public static Operation<Out> Error<Out>(this Operation<Operation<Out>> op, Action action)
        {
            if (!op.Succeeded || !op.Result.Succeeded)
            {
                try
                {
                    action();
                }
                catch
                { }
            }

            return op.Result;
        }

        /// <summary>
        /// Executes and transforms to a successfull operation instead of propagating a failed operation
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <param name="op"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Operation<Out> Instead<Out>(this Operation<Out> op, Func<Operation<Out>, Out> func)
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
        public static Operation<Out> Instead<Out>(this Operation<Operation<Out>> op, Func<Operation<Out>, Out> func)
        {
            if (!op.Succeeded || !op.Result.Succeeded) return Operation.Run(() => func(op.Result));
            return op.Result;
        }

        ///...
    }
}
