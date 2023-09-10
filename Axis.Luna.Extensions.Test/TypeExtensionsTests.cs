using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using static Axis.Luna.Extensions.TypeExtensions;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class TypeExtensionsTests
    {
        [TestMethod]
        public void DefaultTests()
        {
            var objectType = typeof(object);
            var stringType = typeof(string);
            var intType = typeof(int);
            var boolType = typeof(bool);
            var dateTimeType = typeof(DateTime);
            var urlType = typeof(Uri);
            var ipaddressType = typeof(IPAddress);
            var interfaceType = typeof(IEnumerable<int>);


            var objectDefault = objectType.DefaultValue();
            Assert.AreEqual(default(object), objectDefault);

            var stringDefault = stringType.DefaultValue();
            Assert.AreEqual(default(string), stringDefault); ;

            var intDefault = intType.DefaultValue();
            Assert.AreEqual(default(int), intDefault); ;

            var boolDefault = boolType.DefaultValue();
            Assert.AreEqual(default(bool), boolDefault); ;

            var dateTimeDefault = dateTimeType.DefaultValue();
            Assert.AreEqual(default(DateTime), dateTimeDefault);

            var urlDefault = urlType.DefaultValue();
            Assert.AreEqual(default(Uri), urlDefault);

            var ipaddressDefault = ipaddressType.DefaultValue();
            Assert.AreEqual(default(IPAddress), ipaddressDefault);

            var interfaceDefault = interfaceType.DefaultValue();
            Assert.AreEqual(default(IEnumerable<int>), interfaceDefault);
        }

        [TestMethod]
        public void PropertyAccessor()
        {
            var abcd = new ABCD();

            var firstNameProp = abcd.Property(nameof(ABCD.FirstName));
            Assert.IsNotNull(firstNameProp);
            Assert.AreEqual(nameof(ABCD.FirstName), firstNameProp.Name);

            firstNameProp = Property(() => abcd.FirstName);
            Assert.IsNotNull(firstNameProp);
            Assert.AreEqual(nameof(ABCD.FirstName), firstNameProp.Name);

            var objValue = abcd.PropertyValue(nameof(ABCD.FirstName));
            Assert.IsNull(objValue);

            var stringValue = abcd.PropertyValue<string>(nameof(ABCD.FirstName));
            Assert.IsNull(stringValue);

            abcd.SetPropertyValue(nameof(ABCD.FirstName), "Stiletho");
            Assert.AreEqual("Stiletho", abcd.FirstName);
            Assert.IsTrue("Stiletho".Equals(abcd.PropertyValue(nameof(ABCD.FirstName))));
            Assert.AreEqual("Stiletho", abcd.PropertyValue<string>(nameof(ABCD.FirstName)));
            Assert.IsNull(stringValue);

            abcd.SetPropertyValue(nameof(ABCD.Weight), 23.43);
            Assert.AreEqual(23.43, abcd.Weight);
            Assert.IsTrue(23.43.Equals(abcd.PropertyValue(nameof(ABCD.Weight))));
            Assert.AreEqual(23.43, abcd.PropertyValue<double>(nameof(ABCD.Weight)));
        }

        [TestMethod]
        public void MinimumAQSignatureTest()
        {
            Delegate d = (Func<string, bool>) string.IsNullOrEmpty;
            Func<string, bool> func = string.IsNullOrWhiteSpace;
            var aqs = func.MinimalAQSignature();
            aqs = func.MinimalAQSignature();
            Console.WriteLine(aqs);
        }

        [TestMethod]
        public void ImplementationTests()
        {
            Assert.IsTrue(typeof(List<int>).ImplementsGenericInterface(typeof(IList<>)));
            Assert.IsTrue(typeof(int[]).ImplementsGenericInterface(typeof(IEnumerable<>)));
        }

        #region Fields
        [TestMethod]
        public void FieldAccessorFor_Tests()
        {
            FieldInfo finfo = null;
            Assert.ThrowsException<ArgumentNullException>(() => finfo.FieldAccessorFor());

            #region TestClass1
            var tc1 = new TestClass1
            {
                ValueField = Guid.NewGuid(),
                RefField = "stuff",
                RefField2 = "another stuff"
            };
            var fields = typeof(TestClass1)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(f => f.Name, f => f);

            Assert.AreEqual(tc1.ValueField, fields["ValueField"].FieldAccessorFor().Invoke(tc1));
            Assert.AreEqual(tc1.RefField, fields["RefField"].FieldAccessorFor().Invoke(tc1));
            Assert.AreEqual(tc1.RefField2, fields["RefField2"].FieldAccessorFor().Invoke(tc1));
            #endregion

            #region TestStruct1
            var ts1 = new TestStruct1
            {
                ValueField = Guid.NewGuid(),
                RefField = "stuff",
                RefField2 = "another stuff"
            };
            fields = typeof(TestStruct1)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(f => f.Name, f => f);

            Assert.AreEqual(ts1.ValueField, fields["ValueField"].FieldAccessorFor().Invoke(ts1));
            Assert.AreEqual(ts1.RefField, fields["RefField"].FieldAccessorFor().Invoke(ts1));
            Assert.AreEqual(ts1.RefField2, fields["RefField2"].FieldAccessorFor().Invoke(ts1));
            #endregion
        }

        [TestMethod]
        public void TypedFieldAccessorFor_Tests()
        {
            FieldInfo finfo = null;
            Assert.ThrowsException<ArgumentNullException>(() => finfo.TypedFieldAccessorFor<TestStruct1, Guid>());

            #region TestClass1
            var tc1 = new TestClass1
            {
                ValueField = Guid.NewGuid(),
                RefField = "stuff",
                RefField2 = "another stuff"
            };
            var fields = typeof(TestClass1)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(f => f.Name, f => f);

            Assert.AreEqual(tc1.ValueField, fields["ValueField"].TypedFieldAccessorFor<TestClass1, Guid>().Invoke(tc1));
            Assert.AreEqual(tc1.RefField, fields["RefField"].TypedFieldAccessorFor<TestClass1, string>().Invoke(tc1));
            Assert.AreEqual(tc1.RefField2, fields["RefField2"].TypedFieldAccessorFor<TestClass1, object>().Invoke(tc1));
            #endregion

            #region TestStruct1
            var ts1 = new TestStruct1
            {
                ValueField = Guid.NewGuid(),
                RefField = "stuff",
                RefField2 = "another stuff"
            };
            fields = typeof(TestStruct1)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(f => f.Name, f => f);

            Assert.AreEqual(ts1.ValueField, fields["ValueField"].TypedFieldAccessorFor<TestStruct1, Guid>().Invoke(ts1));
            Assert.AreEqual(ts1.RefField, fields["RefField"].TypedFieldAccessorFor<TestStruct1, string>().Invoke(ts1));
            Assert.AreEqual(ts1.RefField2, fields["RefField2"].TypedFieldAccessorFor<TestStruct1, object>().Invoke(ts1));
            #endregion
        }

        [TestMethod]
        public void FieldMutatorFor_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ((FieldInfo)null).FieldMutatorFor());

            #region TestClass1
            var tc1 = new TestClass1
            {
                ValueField = Guid.NewGuid(),
                RefField = "stuff",
                RefField2 = "another stuff"
            };
            var fields = typeof(TestClass1)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(f => f.Name, f => f);

            fields["ValueField"].FieldMutatorFor().Invoke(tc1, Guid.Empty);
            Assert.AreEqual(tc1.ValueField, Guid.Empty);

            fields["RefField"].FieldMutatorFor().Invoke(tc1, "new stuff");
            Assert.AreEqual(tc1.RefField, "new stuff");

            fields["RefField2"].FieldMutatorFor().Invoke(tc1, "yet another stuff");
            Assert.AreEqual(tc1.RefField2, "yet another stuff");
            #endregion

            #region TestStruct1
            object ts1 = new TestStruct1
            {
                ValueField = Guid.NewGuid(),
                RefField = "stuff",
                RefField2 = "another stuff"
            };
            fields = typeof(TestStruct1)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(f => f.Name, f => f);

            fields["ValueField"].FieldMutatorFor().Invoke(ts1, Guid.Empty);
            Assert.AreEqual(((TestStruct1)ts1).ValueField, Guid.Empty);

            fields["RefField"].FieldMutatorFor().Invoke(ts1, "new stuff");
            Assert.AreEqual(((TestStruct1)ts1).RefField, "new stuff");

            fields["RefField2"].FieldMutatorFor().Invoke(ts1, "yet another stuff");
            Assert.AreEqual(((TestStruct1)ts1).RefField2, "yet another stuff");
            #endregion
        }

        [TestMethod]
        public void TypedFieldMutatorFor_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ((FieldInfo)null).TypedFieldMutatorFor<TestClass1, string>());

            var field = typeof(TestClass1).GetField("ValueField");
            Assert.ThrowsException<ArgumentException>(() => field.TypedFieldMutatorFor<object, Guid>());
            Assert.ThrowsException<ArgumentException>(() => field.TypedFieldMutatorFor<TestClass1, string>());

            #region TestClass1
            var tc1 = new TestClass1
            {
                ValueField = Guid.NewGuid(),
                RefField = "stuff",
                RefField2 = "another stuff"
            };
            var fields = typeof(TestClass1)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(f => f.Name, f => f);

            fields["ValueField"].TypedFieldMutatorFor<TestClass1, Guid>().Invoke(tc1, Guid.Empty);
            Assert.AreEqual(tc1.ValueField, Guid.Empty);

            fields["RefField"].TypedFieldMutatorFor<TestClass1, string>().Invoke(tc1, "new stuff");
            Assert.AreEqual(tc1.RefField, "new stuff");

            fields["RefField2"].TypedFieldMutatorFor<TestClass1, object>().Invoke(tc1, "yet another stuff");
            Assert.AreEqual(tc1.RefField2, "yet another stuff");
            #endregion
        }
        #endregion


        public class TestClass1
        {
            public Guid ValueField;
            public string RefField;
            public object RefField2;
        }

        public struct TestStruct1
        {
            public Guid ValueField;
            public string RefField;
            public object RefField2;
        }
    }

    public class ABCD
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public double Weight { get; set; }
    }
}
