using System;

namespace Axis.Luna.Operation.Sync
{

    public class SyncOperation<R> : Operation<R>
    {
        private OperationError _error;
        private readonly SyncAwaiter<R> _awaiter;
        private readonly bool _succeeded;

        internal SyncOperation(R value)
        {
            _succeeded = true;
            _awaiter = new SyncAwaiter<R>(value);
        }

        internal SyncOperation(OperationError error)
        {
            _succeeded = false;
            _error = error;
            _awaiter = new SyncAwaiter<R>(_error.GetException());
        }

        public override bool? Succeeded => _succeeded;

        public override OperationError Error => _error;

        public override IAwaiter<R> GetAwaiter() => _awaiter;

        public override R Resolve() => _awaiter.GetResult();
    }

    public class SyncOperation : Operation
    {
        private OperationError _error;
        private SyncAwaiter _awaiter;


        public override bool? Succeeded => false;
        public override OperationError Error => throw new NotImplementedException();

        internal SyncOperation(OperationError error)
        {
            _error = error;
            _awaiter = new SyncAwaiter(_error.GetException());
        }

        public override IAwaiter GetAwaiter() => _awaiter;

        public override void Resolve() => _awaiter.GetResult();
    }
}
