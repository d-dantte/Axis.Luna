using System;

namespace Axis.Luna.Operation
{
    public interface IOperation
    {
        bool? Succeeded { get; }

        void Resolve();
        Exception GetException();


        #region Continuations
        IOperation Then(Func<IOperation> continuation, Action<Exception> error = null);
        IOperation<S> Then<S>(Func<IOperation<S>> continuation, Action<Exception> error = null);

        IOperation Then(Action continuation, Action<Exception> error = null);
        IOperation<R> Then<R>(Func<R> continuation, Action<Exception> error = null);

        IOperation ContinueWith(Action<IOperation> continuation);
        IOperation<R> ContinueWith<R>(Func<IOperation, R> continuation);

        IOperation ContinueWith(Func<IOperation, IOperation> continuation);
        IOperation<S> ContinueWith<S>(Func<IOperation, IOperation<S>> continuation);
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


        IOperation ContinueWith(Action<IOperation<R>> continuation);
        IOperation<S> ContinueWith<S>(Func<IOperation<R>, S> continuation);

        IOperation ContinueWith(Func<IOperation<R>, IOperation> continuation);
        IOperation<S> ContinueWith<S>(Func<IOperation<R>, IOperation<S>> continuation);
        #endregion

        #region Finally
        IOperation<R> Finally(Action @finally);
        #endregion
    }
}
