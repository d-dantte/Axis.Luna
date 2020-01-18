using System;

namespace Axis.Luna.Operation
{

    public class OperationException: Exception
    {
        public OperationError Error { get; }

        public OperationException(OperationError error)
        : base(error.Message?? "Unknown Error", error.GetException())
        {
            Error = error;
        }        
    }


    public class OperationError
    {
        private Exception _exception;

        public string Message { get; }
        public string Code { get; }
        public object Data { get; }


        public Exception GetException() => _exception ?? (_exception = new Exception(Message ?? "Unknown Error"));


        public OperationError(Exception ex)
            :this(null, null, null, ex)
        {
        }

        public OperationError(string message, string code = null, object data = null, Exception exception = null)
        {
            Message = message;
            Code = code;
            Data = data;
            _exception = exception;
        }

        public OperationError()
            :this(null, null, null, null)
        { }

        public void Throw() => throw new OperationException(this);
    }
}
