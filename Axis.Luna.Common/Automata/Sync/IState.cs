namespace Axis.Luna.Common.Automata.Sync
{
    /// <summary>
    /// A state within the state machine
    /// </summary>
    /// <typeparam name="TData">The state data</typeparam>
    public interface IState<TData> where TData : class
    {
        /// <summary>
        /// The name of this state
        /// </summary>
        string StateName { get; }

        /// <summary>
        /// Called by the state machine when this state is being entered.
        /// </summary>
        /// <param name="previousState">The previous state: null if this is the first state</param>
        /// <param name="data">The state data</param>
        void Entering(string previousState, TData data);

        /// <summary>
        /// Called by the state machine when this state is being exited.
        /// </summary>
        /// <param name="nextState">The state to which the machine will transition</param>
        /// <param name="data">The state data</param>
        void Leaving(string nextState, TData data);

        /// <summary>
        /// Called by the state machine for each externally triggered event.
        /// </summary>
        /// <param name="data">The state data that may or may not be modified by this method</param>
        /// <returns>The new state to transition to: null means stop, <see cref="StateName"/> means repeat this state, any other value means transition to that state </returns>
        string Act(TData data);

    }
}
