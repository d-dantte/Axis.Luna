using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;

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
    }

    public class ABCD
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public double Weight { get; set; }
    }


}
