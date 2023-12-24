using BenchmarkDotNet.Attributes;
using System.Collections.Immutable;

namespace Axis.Luna.Unions.Benchmarks
{
    [MemoryDiagnoser(false)]
    public class UnionTypeMetadataBenchmark
    {
        private static readonly UnionTypeMetadata Union2 = new UnionTypeMetadata(
            "Be.Nice",
            "Firsts",
            UnionTypeMetadata.TypeForm.Struct,
            Array.Empty<TypeArg>().ToImmutableArray(),
            ImmutableArray.Create<TypeArg>(
                typeof(int),
                typeof(string)));

        private static readonly UnionTypeMetadata Union21 = new UnionTypeMetadata(
            "Be.Nice",
            "Firsts",
            UnionTypeMetadata.TypeForm.Struct,
            ImmutableArray.Create<TypeArg>("T1"),
            ImmutableArray.Create<TypeArg>(
                typeof(int),
                "T1"));

        private static readonly UnionTypeMetadata Union3 = new UnionTypeMetadata(
            "Be.Nice",
            "Firsts",
            UnionTypeMetadata.TypeForm.Struct,
            Array.Empty<TypeArg>().ToImmutableArray(),
            ImmutableArray.Create<TypeArg>(
                typeof(int),
                typeof(string),
                typeof(Guid)));

        private static readonly UnionTypeMetadata Union32 = new UnionTypeMetadata(
            "Be.Nice",
            "Firsts",
            UnionTypeMetadata.TypeForm.Struct,
            ImmutableArray.Create<TypeArg>("T1", "T2"),
            ImmutableArray.Create<TypeArg>(
                typeof(int),
                "T1", "T2"));

        private static readonly UnionTypeMetadata Union4 = new UnionTypeMetadata(
            "Be.Nice",
            "Firsts",
            UnionTypeMetadata.TypeForm.Struct,
            Array.Empty<TypeArg>().ToImmutableArray(),
            ImmutableArray.Create<TypeArg>(
                typeof(int),
                typeof(string),
                typeof(Guid),
                typeof(decimal)));

        private static readonly UnionTypeMetadata Union43 = new UnionTypeMetadata(
            "Be.Nice",
            "Firsts",
            UnionTypeMetadata.TypeForm.Struct,
            ImmutableArray.Create<TypeArg>("T1", "T2", "T3"),
            ImmutableArray.Create<TypeArg>(
                typeof(int),
                "T1", "T2", "T3"));

        [Benchmark]
        public void GenerateUnion2() => Union2.GenerateImplementation();

        [Benchmark]
        public void GenerateUnion3() => Union3.GenerateImplementation();

        [Benchmark]
        public void GenerateUnion4() => Union4.GenerateImplementation();

        [Benchmark]
        public void GenerateUnion21() => Union21.GenerateImplementation();

        [Benchmark]
        public void GenerateUnion32() => Union32.GenerateImplementation();

        [Benchmark]
        public void GenerateUnion43() => Union43.GenerateImplementation();
    }
}
