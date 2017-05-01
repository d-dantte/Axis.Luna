using System;

namespace Axis.Luna.Operation
{
    public interface IOperation
    {
        bool? Succeeded { get; }

        void Resolve();
        Exception GetException();


        #region Continuations
        IOperation Then(Action continuation, Action<Exception> error = null);
        IOperation<R> Then<R>(Func<R> continuation, Action<Exception> error = null);
        #endregion

        #region Transformers
        IOperation<R> Then<R>(Func<Action, IOperation<R>> continuation, Action<Exception> error = null);
        IOperation Then(Func<Action, IOperation> continuation, Action<Exception> error = null);
        #endregion

        #region Error
        IOperation Otherwise(Action<Exception> errorContinuation);
        IOperation<R> Otherwise<R>(Func<Exception, R> errorContinuation, Func<R> successContinuation);
        #endregion

        #region Finally
        IOperation Finally(Action @finally);
        #endregion
    }

    public interface IOperation<out R>
    {
        bool? Succeeded { get; }
        R Result { get; }

        R Resolve();
        Exception GetException();


        #region Continuations
        IOperation Then(Action<R> continuation, Action<Exception> error = null);
        IOperation<S> Then<S>(Func<R, S> continuation, Action<Exception> error = null);
        #endregion

        #region Transformers
        IOperation Then(Func<Func<R>, IOperation> continuation, Action<Exception> error = null);
        IOperation<S> Then<S>(Func<Func<R>, IOperation<S>> continuation, Action<Exception> error = null);
        #endregion

        #region Error
        IOperation Otherwise(Action<Exception> errorContinuation);
        IOperation<S> Otherwise<S>(Func<Exception, S> errorContinuation, Func<S> successContinuation);
        #endregion

        #region Finally
        IOperation<R> Finally(Action @finally);
        #endregion
    }
}
