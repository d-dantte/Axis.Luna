using System;

namespace Axis.Luna.Common.Automata
{
    /// <summary>
    /// Exception that signifies an error raised while transitioning from one state to another
    /// </summary>
    public class StateTransitionException : Exception
    {
        public string PreviousState { get; }

        public string NewState { get; }

        public StateTransitionException(
            string previousState,
            string newState,
            Exception cause = null)
            : base("An error occured while transitioning states", cause)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }
}
