using Axis.Luna.Common.Types.Basic;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Test.Types.Basic
{
    [TestClass]
    public class BasicStructTests
    {
        [TestMethod]
        public void CopyIsolationTest()
        {
            var struct1 = default(BasicStruct);
            var struct2 = new BasicStruct();

            Assert.AreEqual(struct1, struct2);
            Assert.IsTrue(struct1.EquivalentTo(struct2));
            Assert.IsTrue(struct1.ExactyCopyOf(struct2));
            Assert.IsTrue(struct1.Equals(struct2));

            // pre interraction copy
            var (mutatedCopy1, mutatedCopy2) = CopyAndTest(struct1, struct2);

            Assert.AreNotEqual(struct1, mutatedCopy1);
            Assert.IsFalse(struct1.EquivalentTo(mutatedCopy1));
            Assert.IsFalse(struct1.ExactyCopyOf(mutatedCopy1));

            Assert.AreNotEqual(struct2, mutatedCopy2);
            Assert.IsFalse(struct2.EquivalentTo(mutatedCopy2));
            Assert.IsFalse(struct2.ExactyCopyOf(mutatedCopy2));
        }

        #region Consstruction
        [TestMethod]
        public void DefaultConstructor_ShouldConstructDefaultValue()
        {
            var default1 = default(BasicStruct);
            var default2 = new BasicStruct();

            Assert.IsTrue(default1.IsDefault);
            Assert.IsTrue(default2.IsDefault);
            Assert.AreEqual(default1, default2);
        }

        [TestMethod]
        public void Constructor_ShouldConstructDefaultValue()
        {
            var default1 = new BasicStruct((IEnumerable<BasicMetadata>)new BasicMetadata[] { "stuff;" });
            var default2 = new BasicStruct("me:you;");

            Assert.IsFalse(default1.IsDefault);
            Assert.IsFalse(default2.IsDefault);
            Assert.AreEqual(default1, default2);
        }
        #endregion

        #region Accessors
        [TestMethod]
        public void StructInstance_ShouldHaveStructType()
        {
            Assert.AreEqual(BasicTypes.Struct, default(BasicStruct).Type);
            Assert.AreEqual(BasicTypes.Struct, new BasicStruct().Type);
            Assert.AreEqual(BasicTypes.Struct, new BasicStruct("stuff;").Type);
            Assert.AreEqual(BasicTypes.Struct, new BasicStruct(new BasicMetadata[0].AsEnumerable()).Type);
        }
        #endregion

        #region Indexers
        [TestMethod]
        public void SetProperty_SetsTheProperty()
        {
            var sName = "abra-kadabra";
            var sValue = "Mighty Man";
            var metadata = new BasicMetadata[] { "first;", "second;" };
            var pName = new BasicStruct.PropertyName(sName, metadata);

            var @struct = new BasicStruct
            {
                [pName] = sValue
            };

            //confirm property count
            Assert.AreEqual(1, @struct.Count);

            //confirm property
            Assert.IsTrue(@struct.HasProperty(sName));

            @struct[sName].AsStringValue().Consume(
                value => Assert.AreEqual(sValue, value),
                () => throw new System.Exception(""));

            @struct[pName].AsStringValue().Consume(
                value => Assert.AreEqual(sValue, value),
                () => throw new System.Exception(""));

            @struct.Value.First(kvp => kvp.Key.Equals(pName)).Value.AsStringValue().Consume(
                value => Assert.AreEqual(sValue, value),
                () => throw new System.Exception(""));

        }

        [TestMethod]
        public void SetProperty_WithMetadata_SetsTheProperty()
        {
            var sName = "abra-kadabra";
            var metadata = new BasicMetadata[] { "first;", "second;" };
            var pName = new BasicStruct.PropertyName(sName, metadata);

            var @struct = new BasicStruct
            {
                [pName] = "Mighty Man"
            };

            //confirm metadata
            Assert.IsTrue(metadata.SequenceEqual(@struct.PropertyMetadata[sName]));
            Assert.IsTrue(metadata.SequenceEqual(@struct.PropertyNames.First(name => name.Equals(pName)).Metadata));
            Assert.IsTrue(metadata.SequenceEqual(@struct.Value.First(kvp => kvp.Key.Equals(pName)).Key.Metadata));
        }

        [TestMethod]
        public void MutatorIndexer_WithStringPropertyName_ShouldNotReplaceMetadata()
        {
            var sName = "abra-kadabra";
            var metadata1 = new BasicMetadata[] { "first;", "second;" };
            var metadata2 = new BasicMetadata[] { "mad-laugh;" };
            var pName1 = new BasicStruct.PropertyName(sName, metadata1);
            var pName2 = new BasicStruct.PropertyName(sName, metadata2);

            var @struct = new BasicStruct
            {
                [pName1] = "Mighty Man"
            };
            Assert.AreEqual(metadata1.Length, @struct.PropertyMetadata[sName].Length);

            //replace property with string name; metadata is unchanged
            @struct[sName] = "Big Man";
            Assert.AreEqual(metadata1.Length, @struct.PropertyMetadata[sName].Length);
        }

        [TestMethod]
        public void MutatorIndexer_WithStringPropertyName_ShouldReplaceMetadata()
        {
            var sName = "abra-kadabra";
            var metadata1 = new BasicMetadata[] { "first;", "second;" };
            var metadata2 = new BasicMetadata[] { "mad-laugh;" };
            var pName1 = new BasicStruct.PropertyName(sName, metadata1);
            var pName2 = new BasicStruct.PropertyName(sName, metadata2);

            var @struct = new BasicStruct
            {
                [pName1] = "Mighty Man"
            };
            Assert.AreEqual(metadata1.Length, @struct.PropertyMetadata[sName].Length);

            //replace property with PropertyName instance; metadata is changed
            @struct[pName2] = "Big Man";
            Assert.AreNotEqual(metadata1.Length, @struct.PropertyMetadata[sName].Length);
            Assert.AreEqual(metadata2.Length, @struct.PropertyMetadata[sName].Length);
            Assert.IsTrue(metadata2.SequenceEqual(@struct.PropertyMetadata[sName]));
        }
        #endregion

        private (BasicStruct, BasicStruct) CopyAndTest(BasicStruct copy1, BasicStruct copy2)
        {
            Assert.AreEqual(copy1, copy2);
            Assert.IsTrue(copy1.EquivalentTo(copy2));
            Assert.IsTrue(copy1.ExactyCopyOf(copy2));

            copy1["name"] = "daniel";
            copy2["name"] = "daniel";

            Assert.AreEqual(copy1, copy2);
            Assert.IsTrue(copy1.EquivalentTo(copy2));
            Assert.IsFalse(copy1.ExactyCopyOf(copy2));

            return (copy1, copy2);
        }
    }
}
