using Axis.Luna.Extensions.Benchmark.Types;
using BenchmarkDotNet.Attributes;

namespace Axis.Luna.Extensions.Benchmark
{
    public class Enumerable
    {
        public static ISomething[] DummySomethings = new []
        {
            new Something1()
        };

        [Benchmark]
        public Something1[] HardCast_Enumerable()
        {
            return DummySomethings
                .Select(v => (Something1)v)
                .ToArray();
        }

        [Benchmark]
        public Something1[] HardCast_Enumerable_Extension()
        {
            return DummySomethings
                .HardCast<Something1>()
                .ToArray();
        }

        [Benchmark]
        public Something1[] HardCast_Enumerable_GenericExtension()
        {
            return DummySomethings
                .HardCast<ISomething, Something1>()
                .ToArray();
        }
    }
}
