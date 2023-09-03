
namespace Axis.Luna.FInvoke.Tests
{
    [TestClass]
    public class InstanceInvokerTests
    {
        [TestMethod]
        public void Invoke_PropertyOfStruct()
        {
            var t = typeof(XStruct);
            var nameProperty = t.GetProperty("Name");
            var descProperty = t.GetProperty("Description");
            var blehProperty = t.GetProperty("Bleh");
            var dobProperty =  t.GetProperty("Dob");

            var nameSetter = InstanceInvoker.InvokerFor(nameProperty!.GetSetMethod());
            var descSetter = InstanceInvoker.InvokerFor(descProperty!.GetSetMethod());
            var blehSetter = InstanceInvoker.InvokerFor(blehProperty!.GetSetMethod());
            var dobSetter = InstanceInvoker.InvokerFor(dobProperty!.GetSetMethod());
            var charCount = InstanceInvoker.InvokerFor(t.GetMethod("NameCharCount"));

            var obj = new XStruct("initial name");
            object boxedObj = obj;

            var newName = "The new name";
            nameSetter.Invoke(boxedObj, newName);
            blehSetter.Invoke(boxedObj, "blehritoriousas");
            descSetter.Invoke(boxedObj, "some description");
            dobSetter.Invoke(boxedObj, DateTimeOffset.Now);
            var count = charCount.Invoke(boxedObj);
            Assert.AreEqual(newName.Length, (int)count);

            obj = (XStruct)boxedObj;
        }

        [TestMethod]
        public void Invoker_RefType_Tests()
        {
            var tinvoker = new TypeInvoker(typeof(XClass));
            var instance = tinvoker.ConstructorInvokers.Invokers
                .FirstOrDefault()!
                .New("the initial name")
                as XClass;

            var result = tinvoker.InstanceMethodInvokers.Keys
                .FirstOrDefault(m => m.Name.Equals("NameCharCount"))
                .ApplyTo(m => tinvoker.InstanceMethodInvokers[m])
                .Invoke(instance);

            result = tinvoker.InstanceMethodInvokers.Keys
                .FirstOrDefault(m => m.Name.Equals("MutateName"))
                .ApplyTo(m => tinvoker.InstanceMethodInvokers[m])
                .Invoke(instance, "new name");

            result = tinvoker.InstanceMethodInvokers.Keys
                .FirstOrDefault(m => m.Name.Equals("Method2"))
                .ApplyTo(m => tinvoker.InstanceMethodInvokers[m])
                .Invoke(instance);

            result = tinvoker.InstanceMethodInvokers.Keys
                .FirstOrDefault(m => m.Name.Equals("Method1"))
                .ApplyTo(m => tinvoker.InstanceMethodInvokers[m])
                .Invoke(instance, "random string to count");

            result = tinvoker.InstanceGetters["Name"].Invoke(instance);
            result = tinvoker.InstanceSetters["Description"].Invoke(instance, "everlasting description");
        }

        [TestMethod]
        public void Invoker_ValueType_Tests()
        {
            var tinvoker = new TypeInvoker(typeof(XStruct));
            object instance = (XStruct)tinvoker.ConstructorInvokers.Invokers
                .FirstOrDefault()!
                .New("the initial name");

            var result = tinvoker.InstanceMethodInvokers.Keys
                .FirstOrDefault(m => m.Name.Equals("NameCharCount"))
                .ApplyTo(m => tinvoker.InstanceMethodInvokers[m])
                .Invoke(instance);

            result = tinvoker.InstanceMethodInvokers.Keys
                .FirstOrDefault(m => m.Name.Equals("MutateName"))
                .ApplyTo(m => tinvoker.InstanceMethodInvokers[m])
                .Invoke(instance, "new name");

            result = tinvoker.InstanceMethodInvokers.Keys
                .FirstOrDefault(m => m.Name.Equals("Method2"))
                .ApplyTo(m => tinvoker.InstanceMethodInvokers[m])
                .Invoke(instance);

            result = tinvoker.InstanceMethodInvokers.Keys
                .FirstOrDefault(m => m.Name.Equals("Method1"))
                .ApplyTo(m => tinvoker.InstanceMethodInvokers[m])
                .Invoke(instance, "random string to count");

            result = tinvoker.InstanceGetters["Name"].Invoke(instance);
            result = tinvoker.InstanceSetters["Description"].Invoke(instance, "everlasting description");
        }

        public interface IMethodProvider
        {
            int Method1(string arg);
            void Method2();
            void MutateName(string arg);
        }

        public class XClass: IMethodProvider
        {
            public XClass(string name)
            {
                Name = name;
            }

            public string? Name { get; set; }
            public string? Bleh { get; set; }
            public DateTimeOffset Dob { get; set; }
            public string? Description { get; set; }

            public int Method1(string arg) => arg?.Length ?? -1;

            public void Method2()
            {
            }

            public void MutateName(string arg)
            {
                Name = arg;
            }

            public int NameCharCount() => Name?.Length ?? -1;
        }

        public struct XStruct: IMethodProvider
        {
            public XStruct(string name)
            {
                Name = name;
            }

            public string? Name { get; set; }
            public string? Bleh { get; set; }
            public DateTimeOffset Dob { get; set; }
            public string? Description { get; set; }

            public int Method1(string arg) => arg?.Length ?? -1;

            public void Method2()
            {
            }

            public void MutateName(string arg)
            {
                Name = arg;
            }

            public int NameCharCount() => Name?.Length ?? -1;
        }
    }
}
