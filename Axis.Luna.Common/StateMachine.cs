using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axis.Luna.Common
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

    /// <summary>
    /// A single-event state machine
    /// </summary>
    /// <typeparam name="TData">The state data</typeparam>
    public class StateMachine<TData> where TData : class
    {
        /// <summary>
        /// The state of the state machine's processing cycle
        /// </summary>
        internal enum MachineState
        {
            Act,
            Transition
        }


        private readonly Dictionary<string, IState<TData>> _states = new Dictionary<string, IState<TData>>();
        private MachineState _machineState = MachineState.Transition;
        private string _nextState = null;

        /// <summary>
        /// The current state of the system
        /// </summary>
        public string CurrentState { get; private set; }

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
        public string[] States => _states.Keys.ToArray();

        /// <summary>
        /// Indicates if the state machine can continue processing.
        /// </summary>
        public bool IsMachineInEndState => StateData is null;

        /// <summary>
        /// A count of the states in the machine
        /// </summary>
        public int StateCount => _states.Count;

        public StateMachine(TData stateData, string startState, params IState<TData>[] states)
        {
            StateData = stateData ?? throw new ArgumentNullException(nameof(stateData));

            StartState = startState.ThrowIf(
                string.IsNullOrWhiteSpace,
                new ArgumentException($"Invalid {nameof(startState)}"));

            _nextState = StartState;

            states
                .ThrowIfNull(new ArgumentNullException(nameof(states)))
                .ThrowIf(
                    ArrayExtensions.IsEmpty,
                    new ArgumentException("Empty states not allowed"))
                .ForAll(state =>
                {
                    if (string.IsNullOrWhiteSpace(state.StateName))
                        throw new InvalidOperationException("Invalid state name");

                    _states
                        .TryAdd(state.StateName, state)
                        .ThrowIf(false, new InvalidOperationException($"Duplicate state name: {state.StateName}"));
                });

            if (!_states.ContainsKey(startState))
                throw new ArgumentException($"{StartState} is not one of the supplied states");
        }

        /// <summary>
        /// Fires an event at the current state, and transitions to the next state if necessary
        /// </summary>
        /// <returns>true if the state machine is still active, false if it has reached it's end-state. Repeated calls to an end-state machine returns false</returns>
        public bool TryAct()
        {
            return !IsMachineInEndState && _machineState switch
            {
                MachineState.Act => !this
                    .PropagateAction()
                    .Transition()
                    .IsMachineInEndState,

                MachineState.Transition => this
                    .Transition()
                    .TryAct(),

                _ => throw new InvalidOperationException($"Invalid machine processing state: {_machineState}")
            };
        }


        public TData Act()
        {
            while (TryAct()) ;

            return StateData;
        }

        //public Task<TData> ActAsync() => Task.Run(Act);

        private StateMachine<TData> Transition()
        {
            // special case
            if (CurrentState == _nextState)
            {
                _nextState = null;
                _machineState = MachineState.Act;
            }
            else
            {
                var isEndState = _nextState is null;
                var stateExists = !isEndState
                    ? _states.ContainsKey(_nextState)
                    : false;

                if (!isEndState && !stateExists)
                    throw new StateTransitionException(CurrentState, _nextState);

                try
                {
                    // CurrentState is null ONLY when we are entering the start-state
                    if (CurrentState != null)
                    {
                        var previousState = _states[CurrentState];
                        previousState.Leaving(_nextState, StateData);
                    }

                    // _nextState is null ONLY when entering the end-state
                    if (_nextState != null)
                    {
                        var nextState = _states[_nextState];
                        nextState.Entering(CurrentState, StateData);
                    }

                    CurrentState = _nextState;
                    _nextState = null;

                    // successfully transitioned into the end-state
                    if (CurrentState is null)
                        StateData = null;

                    _machineState = MachineState.Act;
                }
                catch (Exception e)
                {
                    throw new StateTransitionException(
                        CurrentState,
                        _nextState,
                        e);
                }
            }

            return this;
        }

        private StateMachine<TData> PropagateAction()
        {
            var state = _states[CurrentState];
            _nextState = state.Act(StateData);
            _machineState = MachineState.Transition;

            return this;
        }
    }

    public class GenericState<TData> : IState<TData> where TData : class
    {
        private readonly Func<TData, string> _act;
        private readonly Action<string, TData> _entering;
        private readonly Action<string, TData> _leaving;

        public string StateName { get; }

        public GenericState(
            string stateName,
            Func<TData, string> act,
            Action<string, TData> entering = null,
            Action<string, TData> leaving = null)
        {
            StateName = stateName;
            _act = act ?? throw new ArgumentNullException(nameof(act));
            _entering = entering;
            _leaving = leaving;
        }



        public string Act(TData data) => _act.Invoke(data);

        public void Entering(string previousState, TData data) => _entering?.Invoke(previousState, data);

        public void Leaving(string nextState, TData data) => _leaving?.Invoke(nextState, data);
    }
}
