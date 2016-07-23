namespace Axis.Luna
{
    using System;
    using System.Threading.Tasks;
    using MetaTypes;

    using static Extensions.ObjectExtensions;
    using static Extensions.ExceptionExtensions;

    public class Operation<R>
    {
        internal Operation() { }
        internal Operation(Func<R> func)
        {
            ThrowNullArguments(() => func);

            try
            {
                Result = func();
                Succeeded = true;
            }
            catch (Exception e)
            {
                Error = e;
                Succeeded = false;
            }
        }

        #region Properties
        internal Exception Error { get; set; }
        private string _message;

        public R Result { get; set; } //not a private setter because of serialization
        public bool Succeeded { get; set; }
        public string Message
        {
            get { return Error?.GetRoot(e => e.InnerException)?.Message ?? _message; }
            set { _message = value; }
        }
        #endregion

        #region Methods 
        public R EmpiricalResult() => Result.ThrowIf(r => Error != null, new Exception("See inner exception", Error));
        public Exception GetException() => Error;
        #endregion
    }

    public class Operation : Operation<@void>
    {
        #region init
        internal Operation(Action action)
        : base(() => Void(action))
        { }
        #endregion

        #region Statics
        public static Operation Run(Action action)
            => new Operation(action);

        public static Operation Try(Action action) => Run(action);

        public static Task<Operation> RunAsync(Action action)
            => Task.Run(() => Run(action));

        public static Task<Operation> TryAsync(Action action) => RunAsync(action);

        public static Operation<Result> Run<Result>(Func<Result> func)
            => new Operation<Result>(func);

        public static Operation<Result> Try<Result>(Func<Result> func) => Run(func);

        public static Task<Operation<Result>> RunAsync<Result>(Func<Result> func)
            => Task.Run(() => Run(func));

        public static Task<Operation<Result>> TryASync<Result>(Func<Result> func) => RunAsync(func);

        public static Operation Run(Func<Operation> func)
            => new Operation(() => func());

        public static Operation Try(Func<Operation> func) => Run(func);

        public static Task<Operation> RunAsync(Func<Operation> func)
            => Task.Run(() => Run(func));

        public static Task<Operation> TryAsync(Func<Operation> func) => RunAsync(func);

        public static Operation<Result> Run<Result>(Func<Operation<Result>> func)
            => func();

        public static Operation<Result> Try<Result>(Func<Operation<Result>> func) => Run(func);

        public static Task<Operation<Result>> RunAsync<Result>(Func<Operation<Result>> func)
            => Task.Run(() => Run(func));

        public static Task<Operation<Result>> TryAsync<Result>(Func<Operation<Result>> func) => RunAsync(func);

        public static Task<Operation> RunAsync(Func<Task<Operation>> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                return Task.Run(() => Fail(e));
            }
        }

        public static Task<Operation> TryAsync(Func<Task<Operation>> func) => RunAsync(func);

        public static Task<Operation<Result>> RunAsync<Result>(Func<Task<Operation<Result>>> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                return Task.Run(() => Fail<Result>(e));
            }
        }

        public static Task<Operation<Result>> TryAsync<Result>(Func<Task<Operation<Result>>> func) => RunAsync(func);

        public static Operation<R> NoOp<R>()
        {
            return Run(() => { return default(R); });
        }
        public static Operation NoOp()
        {
            return Run(() => { });
        }

        public static Operation<Value> FromValue<Value>(Value v)
        {
            return Run(() => { return v; });
        }
        public static Operation<R> Fail<R>(Exception ex = null)
            => Run(() => (ex ?? new Exception("Operation Failed")).Throw<R>());
        public static Operation<R> Fail<R>(string message = null)
            => Run(() => new Exception(message ?? "Operation Failed").Throw<R>());
        public static Operation Fail(string message = null)
            => Run(() => new Exception(message ?? "Operation Failed").Throw());
        public static Operation Fail(Exception ex = null)
            => Run(() => (ex ?? new Exception("Operation Failed")).Throw());

        #endregion
    }
}
