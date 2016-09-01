using Axis.Luna.MetaTypes;
using System;
using System.Collections.Concurrent;
namespace Axis.Luna.Extensions
{
    using Axis.Luna.MetaTypes;

    public static class OperationExtensions
    {
        public static Operation Then(this Operation<@void> op, Action<Operation<@void>> action)
        {
            if (op.Succeeded) return Operation.Run(() => action(op));
            else return Operation.Fail(op.Message);
        }

        public static Operation<Out> Then<In, Out>(this Operation<In> op, Func<Operation<In>, Out> func)
        {
            if (op.Succeeded) return Operation.Run(() => func(op));
            else return Operation.Fail<Out>(op.Message);
        }
        public static Operation<Out> Then<In, Out>(this Operation<In> op, Func<Operation<In>, Operation<Out>> func)
        {
            if (op.Succeeded) return func(op);
            else return Operation.Fail<Out>(op.Message);
        }

        public static Operation<Out> Then<Out>(this Operation<@void> op, Func<Operation<@void>, Out> func)
        {
            if (op.Succeeded) return Operation.Run(() => func(op));
            else return Operation.Fail<Out>(op.Message);
        }
        public static Operation<Out> Then<Out>(this Operation<@void> op, Func<Operation<@void>, Operation<Out>> func)
        {
            if (op.Succeeded) return func(op);
            else return Operation.Fail<Out>(op.Message);
        }

        public static Operation Then<In>(this Operation<In> op, Action<Operation<In>> action)
        {
            if (op.Succeeded) return Operation.Run(() => action(op));
            else return Operation.Fail(op.Message);
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
        public static Operation<Out> Instead<Out>(this Operation<Out> op, Func<Operation<Out>, Operation<Out>> func)
        {
            if (!op.Succeeded) return Operation.Run(() => func(op));
            return op;
        }

        public static Operation Instead(this Operation<@void> op, Action<Operation<@void>> action)
        {
            if (!op.Succeeded) return Operation.Try(() => action(op));
            else return Operation.Fail(op.GetException());
        }

        ///...
    }
}
