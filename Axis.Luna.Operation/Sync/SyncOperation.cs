using System;

namespace Axis.Luna.Operation.Sync
{

    public class SyncOperation<R> : Operation<R>
    {
        private readonly OperationError _error;
        private readonly SyncAwaiter<R> _awaiter;

        internal SyncOperation(R value)
        {
            _awaiter = new SyncAwaiter<R>(value);
        }

        internal SyncOperation(OperationError error)
        {
            _error = error;
            _awaiter = new SyncAwaiter<R>(_error.GetException());
        }

        public override bool? Succeeded => _awaiter.IsSuccessful;

        public override OperationError Error => _error;

        public override IAwaiter<R> GetAwaiter() => _awaiter;

        public override R Resolve() => _awaiter.GetResult();


        public static implicit operator SyncOperation<R>(R value) => new SyncOperation<R>(value);

        public static implicit operator SyncOperation<R>(OperationError error) => new SyncOperation<R>(error);
    }

    public class SyncOperation : Operation
    {
        private readonly OperationError _error;
        private readonly SyncAwaiter _awaiter;


        public override bool? Succeeded => _awaiter.IsSuccessful;
        public override OperationError Error => throw new NotImplementedException();

        internal SyncOperation(OperationError error)
        {
            _error = error;
            _awaiter = new SyncAwaiter(_error.GetException());
        }

        public override IAwaiter GetAwaiter() => _awaiter;

        public override void Resolve() => _awaiter.GetResult();


        public static implicit operator SyncOperation(OperationError error) => new SyncOperation(error);
    }
}
