namespace Axis.Luna.FInvoke.Tests
{
    [TestClass]
    public class ConstructorInvokerTests
    {
        [TestMethod]
        public void InvokerFor_WithNoArgConstructor()
        {
            var ctor = typeof(Sample).GetConstructor(Array.Empty<Type>());
            var invoker = ConstructorInvoker.InvokerFor(ctor);
            var obj = invoker.Func.Invoke(Array.Empty<object>());
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void InvokerFor_WithArgConstructor()
        {
            var ctor = typeof(Sample).GetConstructor(new[] { typeof(int), typeof(Guid), typeof(string) });
            var invoker = ConstructorInvoker.InvokerFor(ctor);
            var obj = invoker.New(5, Guid.NewGuid(), "stuff");
            Assert.IsNotNull(obj);
        }

        public class Sample
        {
            public Sample()
            {

            }

            public Sample(int arg1, Guid arg2, string arg3)
            {
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
            }

            public int Arg1 { get; set; }

            public Guid Arg2 { get; set; }

            public string Arg3 { get; set; }
        }
    }
}
