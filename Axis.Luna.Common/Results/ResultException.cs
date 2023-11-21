using System;
using System.Diagnostics;

namespace Axis.Luna.Common.Results
{
    /// <summary>
    /// An exception created having a stack trace pointing to the site where this exception was created.
    /// Facilitates <see cref="IResult{TData}"/>s having stack traces that correspond to where the Error result was created.
    /// This was born from the need to have stack traces in the result exception, especially in cases where the exception
    /// passed into the Result was not thrown prior.
    /// </summary>
    public class ResultException : Exception
    {
        private readonly StackTrace _trace;
        private readonly Lazy<string> _traceString;

        public override string StackTrace => _traceString.Value;

        internal ResultException(Exception cause)
        : this(cause, new StackTrace(1))
        {
        }

        private ResultException(Exception cause, StackTrace trace)
        : base("See Inner, Exception", cause)
        {
            ArgumentNullException.ThrowIfNull(cause);
            ArgumentNullException.ThrowIfNull(trace);

            _trace = trace;
            _traceString = new(_trace.ToString);
            //this.OverwriteStackTrace(_trace);
        }

        private ResultException(Exception cause, StackTrace trace, Lazy<string> traceString)
        : base("See Inner, Exception", cause)
        {
            ArgumentNullException.ThrowIfNull(cause);
            ArgumentNullException.ThrowIfNull(trace);
            ArgumentNullException.ThrowIfNull(traceString);

            _trace = trace;
            _traceString = traceString;
        }

        public ResultException Copy(Exception cause) => new(cause, _trace, _traceString);

        public override int GetHashCode()
        {
            return InnerException.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is ResultException rex
                && this.InnerException.Equals(rex.InnerException);
        }
    }
}
