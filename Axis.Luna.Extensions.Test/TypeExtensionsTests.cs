using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;

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
