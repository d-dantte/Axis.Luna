using System;

namespace Axis.Luna.Operation
{

    public class OperationException: Exception
    {
        public OperationError Error { get; private set; }

        public OperationException(OperationError error)
        : base(error.Message?? "Unknown Error", error.GetException())
        {
            Error = error;
        }        
    }


    public class OperationError
    {
        internal Exception _exception;

        public string Message { get; set; }
        public string Code { get; set; }
        public object Data { get; set; }


        public Exception GetException() => _exception ?? (_exception = new Exception(Message ?? "Unknown Error"));


        public OperationError(Exception ex = null)
        {
            _exception = ex;
        }

        public OperationError()
        { }
    }
}
