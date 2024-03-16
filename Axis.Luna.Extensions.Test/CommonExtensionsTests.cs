using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class CommonExtensionsUnitTests
    {
        [TestMethod]
        public void MiscTests()
        {
            var sarr = new string[] { null };
            var arr = sarr
            .HardCast<string, object>()
            .ToArray();

            sarr = null;
            Assert.ThrowsException<ArgumentNullException>(() => sarr
                .HardCast<string, object>()
                .ToArray());

            arr = sarr
                ?.HardCast<string, object>()
                .ToArray();
            Assert.IsNull(arr);
        }

        [TestMethod]
        public void As_WhenValueIsNull_ShouldReturnDefault()
        {
            object value = null;

            Assert.AreEqual(default, value.As<int>());
            Assert.AreEqual(default, value.As<string>());
        }

        [TestMethod]
        public void As_WhenReturnTypeIsInterface()
        {
            object value = new List<int>();

            Assert.IsNotNull(value.As<IEnumerable<int>>());
            Assert.IsNull(value.As<IEnumerable<double>>());
        }

        [TestMethod]
        public void As_WhenReturnTypeIsSuperClass()
        {
            object value = new CustomList<int>();

            Assert.IsNotNull(value.As<List<int>>());
            Assert.IsNull(value.As<List<double>>());
        }

        [TestMethod]
        public void As_WhenConvertible()
        {
            object value = 54;

            Assert.AreEqual(54L, value.As<long>());
            Assert.AreEqual(default, value.As<Unit>());
        }

        [TestMethod]
        public void As_WhenExplicitConverterExists()
        {
            var value = 554;

            Assert.AreEqual(new Rune(554), value.As<Rune>());
        }

        [TestMethod]
        public void As_WhenImplicitConverterExists()
        {
            var value = 39;

            Assert.AreEqual(new Index(39), value.As<Index>());
        }

        [TestMethod]
        public void As_WhenReturnTypeIsValueType()
        {
            var value = 39;

            Assert.AreEqual(39, value.As<int>());
        }

        [TestMethod]
        public void As_WithBoxedType()
        {
            object value = new Abc();
            Assert.IsNotNull(value.As<ISomething>());
        }

        [TestMethod]
        public void Trim_Tests()
        {
            var x = "abcdddd";
            var r = x.TrimEnd('d');
            Assert.AreEqual("abc", r);
        }

        [TestMethod]
        public void ReboxAs_Tests()
        {
            object x = new Point();
            object r = x.ReboxAs(new Point { X = 43 });
            Assert.AreEqual(43, ((Point)r).X);
            Assert.AreEqual(43, ((Point)x).X);
            Assert.AreEqual(x, r);

            var arr = new object[] { new Point(), x, 6 };
            Assert.AreEqual(0, ((Point)arr[0]).X);
            arr[0].ReboxAs(new Point { X = 91 });
            Assert.AreEqual(91, ((Point)arr[0]).X);
        }

        [TestMethod]
        public void RegularAssignment_Tests()
        {
            var tcwf = new TestClassWithFields();
            FieldInfo[] fields = typeof(TestClassWithFields).GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            fields.ToList().ForEach(field => ReflectionFieldAssignment(tcwf, field, "bleh"));

            Assert.AreEqual("bleh", tcwf.Field1);
            Assert.AreEqual("bleh", tcwf.GetField2());
            Assert.AreEqual("bleh", tcwf.Property1);
        }

        public static void RegularFieldAssignment(TestClassWithFields tcwf, string value)
        {
            tcwf.Field1 = value;
        }

        public static void ReflectionFieldAssignment(object tcwf, FieldInfo field, object value)
        {
            field.SetValue(tcwf, value);
        }

        public interface ISomething { }

        public struct Abc: ISomething
        {

        }

        public struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
        }

        public class TestClass
        {
            public string Name { get; set; }

            public TestClass2 Child { get; set; }
        }

        public class TestClass2
        {
            public string Description { get; set; }
        }

        public class TestClassWithFields
        {
            public string Field1;
            private string Field2;

            public string Property1 { get; set; }

            public string GetField2() => Field2;
        }

        public class TestClass3
        {
            public int ValueField;
            public string RefField;

            public static void SetValueField(TestClass3 instance, int value)
            {
                instance.ValueField = value;
            }
            public static void SetRefField(TestClass3 instance, string value)
            {
                instance.RefField = value;
            }

            public static void SetBoxedValueField(object instance, object value)
            {
                ((TestClass3)instance).ValueField = (int)value;
            }
            public static void SetBoxedRefField(object instance, string value)
            {
                ((TestClass3)instance).RefField = value;
            }

            public static int GetValueField(TestClass3 instance)
            {
                return instance.ValueField;
            }
            public static object GetBoxedValueField(TestClass3 instance)
            {
                return instance.ValueField;
            }
            public static string GetRefField(TestClass3 instance)
            {
                return instance.RefField;
            }
            public static object GetBoxedRefField(TestClass3 instance)
            {
                return instance.RefField;
            }
        }
        public struct TestStruct3
        {
            public int ValueField;
            public object RefField;

            public static void SetValueField(TestStruct3 instance, int value)
            {
                instance.ValueField = value;
            }
            public static void SetRefField(TestStruct3 instance, object value)
            {
                instance.RefField = value;
            }
            public static int GetValueField(TestStruct3 instance)
            {
                return instance.ValueField;
            }
            public static object GetRefField(TestStruct3 instance)
            {
                return instance.RefField;
            }
        }

        public struct TestStructWithFields
        {
            public string Field1;
            private string Field2;

            public string Property1 { get; set; }

            public string GetField2() => Field2;
        }
    }

    public class CustomList<T> : List<T> { }

    public struct Unit { }
}
