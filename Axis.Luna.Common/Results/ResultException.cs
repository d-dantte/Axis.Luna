using System;
using System.Diagnostics;

namespace Axis.Luna.Common.Results
{
    /// <summary>
    /// An exception created having a stack trace pointing to the site where this exception was created.
    /// Facilitates <see cref="IResult{TData}"/>s having stack traces that correspond to where the Error result was created.
    /// This was born from the need to have stack traces in the result exception, expecially in cases where the result
    /// passed into the Result was not thrown prior.
    /// </summary>
    public class ResultException : Exception
    {
        internal ResultException(Exception cause)
        : base("See Inner Exception", cause)
        {
            if (cause is null)
                throw new ArgumentNullException(nameof(cause));

            this.OverwriteStackTrace(new StackTrace(1));
        }

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
