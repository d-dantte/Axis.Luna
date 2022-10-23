using Axis.Luna.Common.Types.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Axis.Luna.Common.Test.Types.Basic
{
    [TestClass]
    public class MetadataTests
    {
        #region Construction tests
        [TestMethod]
        public void Constructor_WithValidData_ShouldCreateValidObject()
        {
            var key = "some_key";
            var value = "some_value";
            var metadata = new Metadata(key, value);

            Assert.AreEqual(key, metadata.Key);
            Assert.AreEqual(value, metadata.Value);

            metadata = new Metadata(new KeyValuePair<string, string>(key, value));

            Assert.AreEqual(key, metadata.Key);
            Assert.AreEqual(value, metadata.Value);

            key = "some_key";
            value = null;
            metadata = new Metadata(key, value);

            Assert.AreEqual(key, metadata.Key);
            Assert.AreEqual(value, metadata.Value);

            metadata = new Metadata(new KeyValuePair<string, string>(key, value));

            Assert.AreEqual(key, metadata.Key);
            Assert.AreEqual(value, metadata.Value);
        }

        [TestMethod]
        public void Constructor_WithInvalidData_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Metadata(null));
        }
        #endregion

        #region Equality 
        [TestMethod]
        public void Equals_ShouldTestEquality()
        {
            var key = "some_key";
            var value = "some_value";
            var metadata = new Metadata(key, value);
            var metadata2 = new Metadata(key, value);
            var metadata3 = new Metadata(key);
            var metadata4 = new Metadata("somethingelse", "bleh");

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.AreEqual(metadata, metadata);
            Assert.IsTrue(metadata.Equals(metadata));
            Assert.IsTrue(metadata == metadata);
            Assert.IsFalse(metadata != metadata);

            Assert.AreEqual(metadata3, metadata3);
            Assert.IsTrue(metadata3.Equals(metadata3));
            Assert.IsTrue(metadata3 == metadata3);
            Assert.IsFalse(metadata3 != metadata3);

            Assert.AreEqual(metadata, metadata2);
            Assert.IsTrue(metadata.Equals(metadata2));
            Assert.IsTrue(metadata == metadata2);
            Assert.IsFalse(metadata != metadata2);

            Assert.AreEqual(metadata2, metadata);
            Assert.IsTrue(metadata2.Equals(metadata));
            Assert.IsTrue(metadata2 == metadata);
            Assert.IsFalse(metadata2 != metadata);

            Assert.AreNotEqual(metadata, metadata3);
            Assert.IsFalse(metadata.Equals(metadata3));
            Assert.IsFalse(metadata == metadata3);
            Assert.IsTrue(metadata != metadata3);

            Assert.AreNotEqual(metadata3, metadata);
            Assert.IsFalse(metadata3.Equals(metadata));
            Assert.IsFalse(metadata3 == metadata);
            Assert.IsTrue(metadata3 != metadata);

            Assert.AreNotEqual(metadata, metadata4);
            Assert.IsFalse(metadata.Equals(metadata4));
            Assert.IsFalse(metadata == metadata4);
            Assert.IsTrue(metadata != metadata4);

            Assert.AreNotEqual(metadata4, metadata);
            Assert.IsFalse(metadata4.Equals(metadata));
            Assert.IsFalse(metadata4 == metadata);
            Assert.IsTrue(metadata4 != metadata);
#pragma warning restore CS1718 // Comparison made to same variable
        }
        #endregion

        #region ToString
        [TestMethod]
        public void ToString_ShouldOutputCorrectFormat()
        {
            var key = "some_key";
            var value = "some_value";
            var metadata = new Metadata(key, value);
            Assert.AreEqual($"{key}:{value};", metadata.ToString());

            metadata = new Metadata(key);
            Assert.AreEqual($"{key};", metadata.ToString());

            metadata = default;
            Assert.AreEqual("", metadata.ToString());
        }
        #endregion`
    }
}
