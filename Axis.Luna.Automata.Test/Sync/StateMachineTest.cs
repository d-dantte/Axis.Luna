using Axis.Luna.Automata.Sync;
using NSubstitute;

namespace Axis.Luna.Automata.Test.Sync
{
    [TestClass]
    public class StateMachineTest
    {
        [TestMethod]
        public void Constructor_WithValidArgs_ReturnsInstance()
        {
            var state = Substitute.For<IState<object>>();
            _ = state.StateName.Returns("start");

            var instance = new StateMachine<object>(0, state);
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Constructor_WithInvalidArgs_ThrowsException()
        {
            var state = Substitute.For<IState<object>>();
            _ = state.StateName.Returns(default(string)!);

            var state2 = Substitute.For<IState<object>>();
            _ = state2.StateName.Returns("start");

            Assert.ThrowsException<ArgumentNullException>(
                () => new StateMachine<object>(null!, state));

            Assert.ThrowsException<ArgumentNullException>(
                () => new StateMachine<object>(0, null!));

            Assert.ThrowsException<InvalidOperationException>(
                () => new StateMachine<object>(0, state));

            Assert.ThrowsException<InvalidOperationException>(
                () => new StateMachine<object>(0, state2, state2));

            Assert.ThrowsException<ArgumentException>(
                () => new StateMachine<object>(0, Array.Empty<IState<object>>()));
        }

        [TestMethod]
        public void Property_Tests()
        {
            var state = Substitute.For<IState<object>>();
            _ = state.StateName.Returns("start");
            var instance = new StateMachine<object>(0, state);

            var states = instance.States;
            Assert.IsNotNull(states);
            Assert.AreEqual(1, states.Length);
            Assert.AreEqual("start", states[0]);

            var currentState = instance.CurrentState;
            Assert.AreEqual("start", currentState);

            Assert.IsFalse(instance.IsMachineInEndState);

            Assert.AreEqual(1, instance.StateCount);
        }

        [TestMethod]
        public void TryAct_Tests()
        {
            var data = new object();
            var state1 = Substitute.For<IState<object>>();
            _ = state1.StateName.Returns("start");
            _ = state1.Act(Arg.Any<string?>(), Arg.Any<object>()).Returns("mid");
            var state2 = Substitute.For<IState<object>>();
            _ = state2.StateName.Returns("mid");
            _ = state2.Act(Arg.Any<string?>(), Arg.Any<object>()).Returns(default(string)!);
            var instance = new StateMachine<object>(data, state1, state2);

            var result = instance.TryAct();
            Assert.IsTrue(result);
            state1.Received().Act(null, data);
            state2.Received().Entering("start", data);
            Assert.AreEqual("mid", instance.CurrentState);

            result = instance.TryAct();
            Assert.IsFalse(result);
            state2.Received().Act("start", data);
            Assert.AreEqual(null, instance.CurrentState);
        }

        [TestMethod]
        public void TryAct_WithInvalidNextState_Tests()
        {
            var data = new object();
            var state1 = Substitute.For<IState<object>>();
            _ = state1.StateName.Returns("start");
            _ = state1.Act(Arg.Any<string?>(), Arg.Any<object>()).Returns("mid");
            var instance = new StateMachine<object>(data, state1);

            Assert.ThrowsException<InvalidOperationException>(() => instance.TryAct());
        }
    }
}
