using System;

namespace Axis.Luna.Common.StringEscape
{
    public class InvalidEscapeSequence: Exception
    {
        public CharSequence EscapeSequence { get; }

        public InvalidEscapeSequence(CharSequence sequence, string? message = null)
        : base(message)
        {
            EscapeSequence = sequence;
        }
    }
}
