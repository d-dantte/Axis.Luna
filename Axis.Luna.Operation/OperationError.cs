using Axis.Luna.Common.Types.Base;
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
        public StructData Data { get; }


        public Exception GetException() => _exception;


        public OperationError(Exception ex)
            :this(null, null, null, ex)
        {
        }

        public OperationError(string message, string code = null, StructData data = null, Exception exception = null)
        {
            Message = message;
            Code = code;
            Data = data;
            _exception = exception ?? new Exception(Message ?? "Unknown Error");
        }

        public OperationError()
            :this(null, null, null, null)
        { }

        public void Throw() => throw new OperationException(this);


        public static implicit operator OperationError(Exception exception) => new OperationError(exception);
    }
}
