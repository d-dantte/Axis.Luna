﻿using System;
using System.Runtime.ExceptionServices;

namespace Axis.Luna.Operation.Lazy
{
    //[DebuggerStepThrough]
    public class LazyOperation<R> : Operation<R>
    {
        private OperationError _error;
        private readonly LazyAwaiter<R> _awaiter;

        internal LazyOperation(Func<R> func)
        {
            if (func == null) throw new NullReferenceException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter<R>(new Lazy<R>(func, true));
        }

        internal LazyOperation(Lazy<R> lazy)
        {
            _awaiter = new LazyAwaiter<R>(lazy ?? throw new NullReferenceException("Invalid Lazy factory supplied"));
        }

        public override bool? Succeeded => _awaiter.IsSuccessful;


        public override IAwaiter<R> GetAwaiter() => _awaiter;

        public override OperationError Error => _error;

        public override R Resolve()
        {
            if (_error != null)
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

            try
            {
                return _awaiter.GetResult();
            }
            catch (OperationException oe)
            {
                _error = oe.Error;
                return ExceptionDispatchInfo
                    .Capture(_error.GetException())
                    .Throw<R>();
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    message: e.Message,
                    code: "GeneralError",
                    exception: e);

                throw;
            }
        }


        public static implicit operator LazyOperation<R>(Func<R> func) => new LazyOperation<R>(func);

        public static implicit operator LazyOperation<R>(Lazy<R> lazy) => new LazyOperation<R>(lazy);
    }


    public class LazyOperation : Operation
    {
        private OperationError _error;
        private readonly LazyAwaiter _awaiter;

        internal LazyOperation(Action action)
        {
            if (action == null) throw new NullReferenceException("Invalid delegate supplied");

            _awaiter = new LazyAwaiter(
                new Lazy<object>(
                    isThreadSafe: true,
                    valueFactory: () =>
                    {
                        action.Invoke();
                        return true;
                    }));
        }

        public override bool? Succeeded => _awaiter.IsSuccessful;

        public override OperationError Error => _error;

        public override IAwaiter GetAwaiter() => _awaiter;

        public override void Resolve()
        {
            if (_error != null)
                ExceptionDispatchInfo.Capture(_error.GetException()).Throw();

            try
            {
                _awaiter.GetResult();
            }
            catch (OperationException oe)
            {
                _error = oe.Error;
                ExceptionDispatchInfo
                    .Capture(_error.GetException())
                    .Throw();
            }
            catch (Exception e)
            {
                _error = new OperationError(
                    message: e.Message,
                    code: "GeneralError",
                    exception: e);

                throw;
            }
        }

        
        public static implicit operator LazyOperation(Action action) => new LazyOperation(action);
    }
}
