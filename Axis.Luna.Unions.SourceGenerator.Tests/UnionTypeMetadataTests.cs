using System.Collections.Immutable;

namespace Axis.Luna.Unions.Tests
{
    [TestClass]
    public class UnionTypeMetadataTests
    {
        [TestMethod]
        public void TestConstruction()
        {
            var typeMeta = new UnionTypeMetadata(
                "Be.Nice",
                "Firsts",
                UnionTypeMetadata.TypeForm.Struct,
                Array.Empty<TypeArg>().ToImmutableArray(),
                ImmutableArray.Create<TypeArg>(
                    typeof(int),
                    typeof(string)));

            Assert.AreEqual("Be.Nice", typeMeta.TypeNamespace);
            Assert.AreEqual("Firsts", typeMeta.TypeName);
            Assert.AreEqual(UnionTypeMetadata.TypeForm.Struct, typeMeta.Form);
            Assert.AreEqual(0, typeMeta.TypeArity);
            Assert.AreEqual(2, typeMeta.UnionTypeArgs.Length);

            typeMeta = new UnionTypeMetadata(
                "Be.Nice",
                "Firsts",
                UnionTypeMetadata.TypeForm.Struct,
                ImmutableArray.Create<TypeArg>("T1", "T2"),
                ImmutableArray.Create<TypeArg>(
                    typeof(int),
                    "T1", "T2"));

            Assert.AreEqual("Be.Nice", typeMeta.TypeNamespace);
            Assert.AreEqual("Firsts", typeMeta.TypeName);
            Assert.AreEqual(UnionTypeMetadata.TypeForm.Struct, typeMeta.Form);
            Assert.AreEqual(2, typeMeta.TypeArity);
            Assert.AreEqual(3, typeMeta.UnionTypeArgs.Length);
        }
    }
}


