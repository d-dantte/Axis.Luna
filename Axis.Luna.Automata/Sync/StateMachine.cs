using System.Collections.Immutable;

namespace Axis.Luna.Automata.Sync
{

    /// <summary>
    /// A single-event state machine
    /// </summary>
    /// <typeparam name="TData">
    /// The state data. State data is constrained to ref-types because it is expected that
    /// modifications to the type will be made.
    /// </typeparam>
    public class StateMachine<TData> where TData : class
    {

        private readonly Dictionary<string, IState<TData>> _states = new();
        private string? _currentState = null;
        private string? _previousState = null;

        /// <summary>
        /// The current state of the system
        /// </summary>
        public string? CurrentState => _currentState;

        /// <summary>
        /// The start state of the machine
        /// </summary>
        public string StartState { get; }

        /// <summary>
        /// The state data
        /// </summary>
        public TData StateData { get; private set; }

        /// <summary>
        /// A list of the states in the machine
        /// </summary>
        public ImmutableArray<string> States => _states.Keys.ToImmutableArray();

        /// <summary>
        /// Indicates if the state machine can continue processing.
        /// </summary>
        public bool IsMachineInEndState => StateData is null;

        /// <summary>
        /// A count of the states in the machine
        /// </summary>
        public int StateCount => _states.Count;

        public StateMachine(TData stateData, params IState<TData>[] states)
        {
            ArgumentNullException.ThrowIfNull(stateData);
            ArgumentNullException.ThrowIfNull(states);

            if (states.Length == 0)
                throw new ArgumentException($"Invalid {nameof(states)}: empty state arrays are forbiddens");

            for (int cnt = 0; cnt < states.Length; cnt++)
            {
                var state = states[cnt];

                if (string.IsNullOrWhiteSpace(state.StateName))
                    throw new InvalidOperationException("Invalid state name: null/empty/blank");

                if (!_states.TryAdd(state.StateName, state))
                    throw new InvalidOperationException($"Duplicate state name: {state.StateName}");
            }

            StateData = stateData;
            StartState = states[0].StateName;
            _currentState = StartState;
        }

        /// <summary>
        /// Fires an event at the current state, and transitions to the next state if necessary.
        /// </summary>
        /// <returns>
        /// true if the state machine is still active, false if it has reached it's end-state.
        /// Repeated calls to an end-state machine returns false
        /// </returns>
        public bool TryAct()
        {
            var nextState = _states[_currentState!].Act(_previousState, StateData);
            _previousState = _currentState;

            if (nextState is not null)
            {
                if (!_states.TryGetValue(nextState!, out var state))
                    throw new InvalidOperationException(
                        $"Invalid state-transition: new-state not found - {nextState}");

                state.Entering(_currentState, StateData);
            }

            _currentState = nextState;

            return _currentState is not null;
        }
    }
}
