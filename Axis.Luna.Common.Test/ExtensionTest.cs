using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Axis.Luna.Common.Test
{
    [TestClass]
    public class ExtensionTest
    {
        [TestMethod]
        public void CSVParserTest()
        {
            var values = "".ParseLineAsCSV().ToArray();
            Assert.AreEqual(0, values.Length);


            values = "abcd".ParseLineAsCSV().ToArray();
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(typeof(string), values[0].GetType());

            values = "3".ParseLineAsCSV().ToArray();
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(typeof(long), values[0].GetType());

            values = "4.211".ParseLineAsCSV().ToArray();
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(typeof(decimal), values[0].GetType());

            values = "True".ParseLineAsCSV().ToArray();
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(typeof(bool), values[0].GetType());

            values = "19/9/2018 12:11:09 PM +01:00".ParseLineAsCSV().ToArray();
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(typeof(DateTimeOffset), values[0].GetType());

            values = "3.04:48:00".ParseLineAsCSV().ToArray();
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(typeof(TimeSpan), values[0].GetType());

            values = "62f57ecd-e684-405b-8915-2a30483561c8".ParseLineAsCSV().ToArray();
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(typeof(Guid), values[0].GetType());


            values = "abcd,xyz,123".ParseLineAsCSV().ToArray();
            Assert.AreEqual(3, values.Length);
            Assert.AreEqual(typeof(string), values[0].GetType());
            Assert.AreEqual(typeof(string), values[1].GetType());
            Assert.AreEqual(typeof(long), values[2].GetType());

            values = "abcd,'xyz,123'".ParseLineAsCSV().ToArray();
            Assert.AreEqual(2, values.Length);
            Assert.AreEqual(typeof(string), values[0].GetType());
            Assert.AreEqual(typeof(string), values[1].GetType());


            values = "abcd,'xyz',123, true, ".ParseLineAsCSV().ToArray();
            Assert.AreEqual(5, values.Length);
            Assert.AreEqual(typeof(string), values[0].GetType());
            Assert.AreEqual(typeof(string), values[1].GetType());
            Assert.AreEqual(typeof(long), values[2].GetType());

        }
    }
}
