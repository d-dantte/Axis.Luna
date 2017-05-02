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

        IOperation Then(Func<IOperation> continuation, Action<Exception> error = null);
        IOperation<S> Then<S>(Func<IOperation<S>> continuation, Action<Exception> error = null);
        #endregion

        #region Error
        IOperation Otherwise(Action<Exception> errorContinuation);
        IOperation<S> Otherwise<S>(Func<Exception, S> errorContinuation, Func<S> successContinuation);

        IOperation Otherwise(Func<Exception, IOperation> errorContinuation);
        IOperation<S> Otherwise<S>(Func<Exception, IOperation<S>> errorContinuation, Func<S> successContinuation);
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

        IOperation Then(Func<R, IOperation> continuation, Action<Exception> error = null);
        IOperation<S> Then<S>(Func<R, IOperation<S>> continuation, Action<Exception> error = null);
        #endregion

        #region Error
        IOperation Otherwise(Action<Exception> errorContinuation);
        IOperation<S> Otherwise<S>(Func<Exception, S> errorContinuation, Func<R, S> successContinuation);

        IOperation Otherwise(Func<Exception, IOperation> errorContinuation);
        IOperation<S> Otherwise<S>(Func<Exception, IOperation<S>> errorContinuation, Func<R, S> successContinuation);
        #endregion

        #region Finally
        IOperation<R> Finally(Action @finally);
        #endregion
    }
}
