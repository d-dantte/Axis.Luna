using System;

namespace Axis.Luna.Operation.Sync
{

    public class SyncOperation<R> : Operation<R>, IResolvable<R>
    {
        private readonly OperationError _error;
        private readonly SyncAwaiter<R> _awaiter;

        internal SyncOperation(R value)
        {
            _error = null;
            _awaiter = new SyncAwaiter<R>(value);
        }

        internal SyncOperation(OperationError error)
        {
            _error = error ?? throw new ArgumentNullException(nameof(error));
            _awaiter = new SyncAwaiter<R>(_error.GetException());
        }

        public override bool? Succeeded => _awaiter.IsSuccessful;

        public override OperationError Error => _error;

        public override IAwaiter<R> GetAwaiter() => _awaiter;

		#region resolvable
		public R Resolve() => _awaiter.GetResult();

        public R ResolveSafely()
        {
            if (Succeeded == true)
                return _awaiter.GetResult();

            else
                return default;
        }

        public bool TryResolve(out R result, out OperationError error)
        {
            if(Succeeded == true)
            {
                result = _awaiter.GetResult();
                error = default;
                return true;
            }
            else // Succeeded = false
            {
                result = default;
                error = Error;
                return false;
            }
        }
        #endregion

        public static implicit operator SyncOperation<R>(R value) => new SyncOperation<R>(value);

        public static implicit operator SyncOperation<R>(OperationError error) => new SyncOperation<R>(error);
    }

    public class SyncOperation : Operation, IResolvable
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

        #region resolvable
        public void Resolve() => _awaiter.GetResult();

        public void ResolveSafely()
        {
            //no-op
        }

        public bool TryResolve(out OperationError error)
        {
            if (Succeeded == true)
            {
                error = default;
                return true;
            }
            else // Succeeded = false
            {
                error = Error;
                return false;
            }
        }
        #endregion

        public static implicit operator SyncOperation(OperationError error) => new SyncOperation(error);
    }
}
