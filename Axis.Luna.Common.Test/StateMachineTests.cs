using Axis.Luna.Common.Automata;
using Axis.Luna.Common.Automata.Sync;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Test
{
    [TestClass]
    public class StateMachineTests
    {
        [TestMethod]
        public void Constructor_WithValidArgs_ReturnsInstance()
        {
            var mockState = new Mock<IState<List<string>>>();
            mockState
                .Setup(s => s.StateName)
                .Returns("first-state");
            var data = new List<string>();
            var statemachine = new StateMachine<List<string>>(
                data,
                "first-state",
                mockState.Object);

            // assert initial state
            Assert.IsNotNull(statemachine);
            Assert.IsNull(statemachine.CurrentState);
            Assert.AreEqual("first-state", statemachine.StartState);
            Assert.AreEqual(data, statemachine.StateData);
            Assert.IsFalse(statemachine.IsMachineInEndState);
        }

        [TestMethod]
        public void Constructor_WithInvalidArgs_ThrowsExceptions()
        {
            var mockState = new Mock<IState<List<string>>>();
            mockState
                .Setup(s => s.StateName)
                .Returns("first-state");

            var emptyNameState = new Mock<IState<List<string>>>();
            emptyNameState
                .Setup(s => s.StateName)
                .Returns("");

            var whitespaceNameState = new Mock<IState<List<string>>>();
            whitespaceNameState
                .Setup(s => s.StateName)
                .Returns(" \t\n\r");

            var data = new List<string>();


            Assert.ThrowsException<ArgumentNullException>(() => new StateMachine<List<string>>(
                null,
                "first-state",
                mockState.Object));

            Assert.ThrowsException<ArgumentException>(() => new StateMachine<List<string>>(
                data,
                null,
                mockState.Object));

            Assert.ThrowsException<ArgumentException>(() => new StateMachine<List<string>>(
                data,
                "",
                mockState.Object));

            Assert.ThrowsException<ArgumentException>(() => new StateMachine<List<string>>(
                data,
                " \t\n\r",
                mockState.Object));

            Assert.ThrowsException<ArgumentNullException>(() => new StateMachine<List<string>>(
                data,
                "first-state",
                null));

            Assert.ThrowsException<ArgumentException>(() => new StateMachine<List<string>>(
                data,
                "first-state",
                Array.Empty<IState<List<string>>>()));


            Assert.ThrowsException<ArgumentNullException>(() => new StateMachine<List<string>>(
                null,
                "first-state",
                new Mock<IState<List<string>>>().Object));


            Assert.ThrowsException<InvalidOperationException>(() => new StateMachine<List<string>>(
                data,
                "first-state",
                emptyNameState.Object));


            Assert.ThrowsException<InvalidOperationException>(() => new StateMachine<List<string>>(
                data,
                "first-state",
                whitespaceNameState.Object));


            Assert.ThrowsException<InvalidOperationException>(() => new StateMachine<List<string>>(
                data,
                "first-state",
                mockState.Object,
                mockState.Object));


            Assert.ThrowsException<ArgumentException>(() => new StateMachine<List<string>>(
                data,
                "last-state",
                mockState.Object));

        }

        #region Transition
        [TestMethod]
        public void Transition_FromInitialState_ShouldTransition()
        {
            var mockState = new Mock<IState<List<string>>>();
            mockState
                .Setup(s => s.StateName)
                .Returns("first-state");
            mockState
                .Setup(s => s.Act(It.IsAny<List<string>>()))
                .Returns<List<string>>(data =>
                {
                    data.Add(data.Count.ToString());
                    return "first-state";
                });
            mockState
                .Setup(s => s.Entering(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((previous, data) =>
                {
                    data.Add(previous);
                });
            mockState
                .Setup(s => s.Leaving(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((next, data) =>
                {
                    data.Add(next);
                });

            var data = new List<string>();
            var statemachine = new StateMachine<List<string>>(
                data,
                "first-state",
                mockState.Object);

            var result = statemachine.TryAct();
            Assert.IsTrue(result);
            Assert.AreEqual("first-state", statemachine.CurrentState);
            Assert.AreEqual(2, data.Count);
            Assert.IsNull(data[0]);
            Assert.AreEqual("1", data[1]);

        }

        [TestMethod]
        public void Transition_BetweenValidStates_ShouldTransition()
        {
            #region state 1
            var mockState = new Mock<IState<List<string>>>();
            mockState
                .Setup(s => s.StateName)
                .Returns("first-state");
            mockState
                .Setup(s => s.Act(It.IsAny<List<string>>()))
                .Returns<List<string>>(data =>
                {
                    data.Add("first-state: acting");
                    return "second-state";
                });
            mockState
                .Setup(s => s.Entering(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((previous, data) =>
                {
                    data.Add($"Event: entering; prev: {previous}; next: first-state");
                });
            mockState
                .Setup(s => s.Leaving(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((next, data) =>
                {
                    data.Add($"Event: leaving; prev: first-state; next: {next}");
                });
            #endregion

            #region state 2
            var mockState2 = new Mock<IState<List<string>>>();
            mockState2
                .Setup(s => s.StateName)
                .Returns("second-state");
            mockState2
                .Setup(s => s.Act(It.IsAny<List<string>>()))
                .Returns<List<string>>(data =>
                {
                    data.Add("second-state: acting");
                    return data.Count < 10
                        ? "first-state"
                        : null;
                });
            mockState2
                .Setup(s => s.Entering(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((previous, data) =>
                {
                    data.Add($"Event: entering; prev: {previous}; next: second-state");
                });
            mockState2
                .Setup(s => s.Leaving(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((next, data) =>
                {
                    data.Add($"Event: leaving; prev: second-state; next: {next}");
                });
            #endregion

            var data = new List<string>();
            var statemachine = new StateMachine<List<string>>(
                data,
                "first-state",
                mockState.Object,
                mockState2.Object);

            var expected = new List<string>
            {
                "Event: entering; prev: ; next: first-state",
                "first-state: acting",
                "Event: leaving; prev: first-state; next: second-state",
                "Event: entering; prev: first-state; next: second-state",
                "second-state: acting",
                "Event: leaving; prev: second-state; next: first-state",
                "Event: entering; prev: second-state; next: first-state",
                "first-state: acting",
                "Event: leaving; prev: first-state; next: second-state",
                "Event: entering; prev: first-state; next: second-state",
                "second-state: acting",
                "Event: leaving; prev: second-state; next: "
            };

            var result = statemachine.TryAct();
            Assert.IsTrue(result);
            result = statemachine.TryAct();
            Assert.IsTrue(result);
            result = statemachine.TryAct();
            Assert.IsTrue(result);
            result = statemachine.TryAct();
            Assert.IsFalse(result);
            result = statemachine.TryAct();
            Assert.IsFalse(result);
            Assert.IsTrue(statemachine.IsMachineInEndState);
            Assert.IsNull(statemachine.StateData);
            Assert.IsNull(statemachine.CurrentState);
            Assert.IsTrue(expected.SequenceEqual(data));
        }

        [TestMethod]
        public void Transition_ToUnknownState_ShouldThrowException()
        {
            #region state 2
            var mockState2 = new Mock<IState<List<string>>>();
            mockState2
                .Setup(s => s.StateName)
                .Returns("first-state");
            mockState2
                .Setup(s => s.Act(It.IsAny<List<string>>()))
                .Returns<List<string>>(data =>
                {
                    return "second-state";
                });
            #endregion

            var data = new List<string>();
            var statemachine = new StateMachine<List<string>>(
                data,
                "first-state",
                mockState2.Object);

            Assert.ThrowsException<StateTransitionException>(() => statemachine.TryAct());
        }

        [TestMethod]
        public void Transition_WithFailingEnterEvent_ShouldThrowException()
        {
            #region state 2
            var mockState2 = new Mock<IState<List<string>>>();
            mockState2
                .Setup(s => s.StateName)
                .Returns("first-state");
            mockState2
                .Setup(s => s.Act(It.IsAny<List<string>>()))
                .Returns<List<string>>(data =>
                {
                    return "first-state";
                });
            mockState2
                .Setup(s => s.Entering(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((previous, data) =>
                {
                    throw new Exception("entering exception");
                });
            #endregion

            var data = new List<string>();
            var statemachine = new StateMachine<List<string>>(
                data,
                "first-state",
                mockState2.Object);

            var ex = Assert.ThrowsException<StateTransitionException>(() => statemachine.TryAct());
            Assert.AreEqual("entering exception", ex.InnerException.Message);
        }

        [TestMethod]
        public void Transition_WithFailingLeaveEvent_ShouldThrowException()
        {
            #region state 1
            var mockState = new Mock<IState<List<string>>>();
            mockState
                .Setup(s => s.StateName)
                .Returns("first-state");
            mockState
                .Setup(s => s.Act(It.IsAny<List<string>>()))
                .Returns<List<string>>(data =>
                {
                    return "second-state";
                });
            mockState
                .Setup(s => s.Leaving(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((next, data) =>
                {
                    throw new Exception("leaving exception");
                });
            #endregion

            #region state 2
            var mockState2 = new Mock<IState<List<string>>>();
            mockState2
                .Setup(s => s.StateName)
                .Returns("second-state");
            mockState2
                .Setup(s => s.Act(It.IsAny<List<string>>()))
                .Returns<List<string>>(data =>
                {
                    return "first-state";
                });
            #endregion

            var data = new List<string>();
            var statemachine = new StateMachine<List<string>>(
                data,
                "first-state",
                mockState.Object,
                mockState2.Object);

            var ex = Assert.ThrowsException<StateTransitionException>(() => statemachine.TryAct());
            Assert.AreEqual("leaving exception", ex.InnerException.Message);
        }
        #endregion
    }
}
