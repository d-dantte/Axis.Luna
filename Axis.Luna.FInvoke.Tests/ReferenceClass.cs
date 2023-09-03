namespace Axis.Luna.FInvoke.Tests
{
    internal class InvokerReferenceClass
    {
        public static object ConstructClass(object arg)
        {
            return new ClassRef((string)arg);
        }

        public static object ConstructStruct(object arg)
        {
            return new StructRef((string)arg);
        }

        public static void WriteStructProperty(StructRef sref)
        {
            sref.Description = "some description";
        }

        public static void MutateStructRefDescription(ref object obj, string desc)
        {
            StructRef sref = (StructRef)obj;
            sref.Description = desc;
            obj = sref;
        }

        public static void MutateSClassRefDescription(ref object obj, string desc)
        {
            ClassRef sref = (ClassRef)obj;
            sref.Description = desc;
        }

        public static void ValueCall2(object obj, string arg1)
        {
            ValueCallImpl2(ref obj, arg1);
        }

        public static void ValueCallImpl2(ref object obj, string arg1)
        {
            var sref = (StructRef)obj;
            IMethodProvider imp = sref;
            imp.MutateName(arg1);
        }

        public static void ValueCall3(object boxed, string arg1)
        {
            var unboxed = (StructRef)boxed;
            unboxed.MutateName(arg1);
        }
    }

    [TestClass]
    public class InvokerReferenceTests
    {
        [TestMethod]
        public void StructMutatorTest()
        {
            var sref = new StructRef("bleh");
            InvokerReferenceClass.ValueCall2(sref, "new name via mutate");
        }
    }

    public interface IMethodProvider
    {
        int Method1(string arg);
        void Method2();
        void MutateName(string arg);
    }


    public struct StructRef: IMethodProvider
    {
        public StructRef(string name)
        {
            Name = name;
        }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset Dob { get; set; }

        public int Method1(string arg) => arg?.Length ?? -1;

        public void Method2() { }

        public void MutateName(string arg)
        {
            Name = arg;
        }
    }

    public class ClassRef : IMethodProvider
    {
        public ClassRef(string name)
        {
            Name = name;
        }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset Dob { get; set; }

        public int Method1(string arg) => arg?.Length ?? -1;

        public void Method2() { }

        public void MutateName(string arg)
        {
            Name = arg;
        }
    }
}
