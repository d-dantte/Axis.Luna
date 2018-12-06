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

        public string Message { get; set; }
        public string Code { get; set; }
        public object Data { get; set; }


        public Exception GetException() => _exception ?? (_exception = new Exception(Message ?? "Unknown Error"));


        public OperationError(Exception ex = null)
        {
            _exception = ex;
        }

        public OperationError(string message, string code = null, object data = null, Exception exception = null)
        {
            Message = message;
            Code = code;
            Data = data;
            _exception = exception;
        }

        public OperationError()
        { }

        public void Throw() => throw new OperationException(this);
    }
}
