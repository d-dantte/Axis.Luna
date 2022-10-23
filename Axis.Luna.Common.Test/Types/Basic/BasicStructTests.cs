using Axis.Luna.Common.Types.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Axis.Luna.Common.Test.Types.Basic
{
    [TestClass]
    public class BasicStructTests
    {
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
        public void Constructor_ShouldConstructNonDefaultValue()
        {
            var v1 = new BasicStruct(
                new BasicStruct.Initializer { ["stuff"] = true });
            var v2 = new BasicStruct(
                new[] { new BasicStruct.Property("prop", IBasicValue.Of(34))},
                new Metadata[] { "bleh" });

            Assert.IsFalse(v1.IsDefault);
            Assert.IsFalse(v2.IsDefault);
            Assert.AreNotEqual(v1, v2);
        }
        #endregion

        #region Accessors
        [TestMethod]
        public void StructInstance_ShouldHaveStructType()
        {
            var v1 = new BasicStruct(
                new BasicStruct.Initializer { ["stuff"] = true });
            var v2 = new BasicStruct(
                new[] { new BasicStruct.Property("prop", IBasicValue.Of(34)) },
                new Metadata[] { "bleh" });

            Assert.AreEqual(BasicTypes.Struct, default(BasicStruct).Type);
            Assert.AreEqual(BasicTypes.Struct, new BasicStruct().Type);
            Assert.AreEqual(BasicTypes.Struct, v1.Type);
            Assert.AreEqual(BasicTypes.Struct, v2.Type);
        }
        #endregion

        #region Indexers
        [TestMethod]
        public void SetProperty_SetsTheProperty()
        {
            var sName = "abra-kadabra";
            var sValue = "Mighty Man";
            var metadata = new Metadata[] { "first;", "second;" };
            var pName = new BasicStruct.PropertyName(sName, metadata);

            BasicStruct @struct = new BasicStruct.Initializer
            {
                [pName] = sValue
            };

            //confirm property count
            Assert.AreEqual(1, @struct.PropertyCount);

            //confirm property
            Assert.IsTrue(@struct.HasProperty(sName));

            BasicString bs = (BasicString)@struct[sName];
            Assert.AreEqual(sValue, bs.Value);

            bs = (BasicString)@struct[pName];
            Assert.AreEqual(sValue, bs.Value);
        }

        [TestMethod]
        public void SetProperty_WithMetadata_SetsTheProperty()
        {
            var sName = "abra-kadabra";
            var metadata = new Metadata[] { "first;", "second;" };
            var pName = new BasicStruct.PropertyName(sName, metadata);

            BasicStruct @struct = new BasicStruct.Initializer
            {
                [pName] = "Mighty Man"
            };

            //confirm metadata
            Assert.IsTrue(metadata.SequenceEqual(@struct.PropertyMetadataFor(sName)));
            Assert.IsTrue(metadata.SequenceEqual(@struct.PropertyNames.First(name => name.Equals(pName)).Metadata));
        }

        [TestMethod]
        public void MutatorIndexer_WithStringPropertyName_ShouldReplaceMetadata()
        {
            var sName = "abra-kadabra";
            var metadata1 = new Metadata[] { "first;", "second;" };
            var metadata2 = new Metadata[] { "mad-laugh;" };
            var pName1 = new BasicStruct.PropertyName(sName, metadata1);
            var pName2 = new BasicStruct.PropertyName(sName, metadata2);

            BasicStruct @struct = new BasicStruct.Initializer
            {
                [pName1] = "Mighty Man"
            };
            Assert.AreEqual(metadata1.Length, @struct.PropertyMetadataFor(sName).Length);

            //replace property with PropertyName instance; metadata is changed
            @struct.Value[pName2] = "Big Man";
            Assert.AreNotEqual(metadata1.Length, @struct.PropertyMetadataFor(sName).Length);
            Assert.AreEqual(metadata2.Length, @struct.PropertyMetadataFor(sName).Length);
            Assert.IsTrue(metadata2.SequenceEqual(@struct.PropertyMetadataFor(sName)));
        }
        #endregion

        private (BasicStruct, BasicStruct) CopyAndTest(BasicStruct copy1, BasicStruct copy2)
        {
            Assert.AreEqual(copy1, copy2);
            Assert.IsTrue(copy1.EquivalentTo(copy2));
            Assert.IsTrue(copy1.ExactyCopyOf(copy2));

            copy1.Value["name"] = "daniel";
            copy2.Value["name"] = "daniel";

            Assert.AreEqual(copy1, copy2);
            Assert.IsTrue(copy1.EquivalentTo(copy2));
            Assert.IsFalse(copy1.ExactyCopyOf(copy2));

            return (copy1, copy2);
        }
    }
}
