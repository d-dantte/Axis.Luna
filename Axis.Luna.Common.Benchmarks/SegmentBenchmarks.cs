using Axis.Luna.Common.Segments;
using BenchmarkDotNet.Attributes;

namespace Axis.Luna.Common.Benchmarks
{
    [MemoryDiagnoser(false)]
    public class SegmentBenchmarks
    {
        [Benchmark]
        public void CreateDefaultSegment()
        {
            var s = default(Segment);
        }

        [Benchmark]
        public void CreateSegment()
        {
            var t = Segment.Of(2, 5);
        }
    }
}
