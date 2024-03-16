using Axis.Luna.Automata.Sync;

namespace Axis.Luna.Automata.Test.Sync
{
    [TestClass]
    public class DefaultStateTest
    {
        [TestMethod]
        public void Constructor_WithValidArgs_ReturnsInstance()
        {
            var act = (string? s, object r) => default(string?);
            var entering = (string? s, object r) => { };
            var instance = new DefaultState<object>("s", act, entering);

            Assert.IsNotNull(instance);
            Assert.AreEqual("s", instance.StateName);
        }

        [TestMethod]
        public void Constructor_WithInvalidArgs_ThrowsExceptions()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultState<object>(
                null!,
                (x, y) => null!,
                (x, y) => { }));
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultState<object>(
                "t",
                null,
                (x, y) => { }));
        }

        [TestMethod]
        public void Entering_CallsDelegate()
        {
            var value = 0;
            var act = (string? s, object r) => default(string?);
            var entering = (string? s, object r) => { value++; };
            var instance = new DefaultState<object>("s", act, entering);

            instance.Entering("new-state", new object());
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void Entering_NoOp_IfDelegateAbsent()
        {
            var act = (string? s, object r) => default(string?);
            var instance = new DefaultState<object>("s", act, null);

            instance.Entering("new-state", new object());
        }

        [TestMethod]
        public void Act_CallsDelegate()
        {
            var act = (string? s, object r) => "new-state";
            var entering = (string? s, object r) => {  };
            var instance = new DefaultState<object>("current-state", act, entering);

            var newState = instance.Act("prev-state", new object());
            Assert.AreEqual("new-state", newState);
        }
    }
}
