using BenchmarkDotNet.Attributes;

namespace Axis.Luna.Common.Benchmarks
{
    public class BitSequence
    {
        private static byte[] Bytes10 = new Random(DateTimeOffset.Now.Millisecond).NextBytes(10);
        private static byte[] Bytes100 = new Random(DateTimeOffset.Now.Millisecond).NextBytes(100);
        private static byte[] Bytes1000 = new Random(DateTimeOffset.Now.Millisecond).NextBytes(1000);
        private static byte[] Bytes10000 = new Random(DateTimeOffset.Now.Millisecond).NextBytes(10000);
        private static byte[] Bytes100000 = new Random(DateTimeOffset.Now.Millisecond).NextBytes(100000);
        private static byte[] Bytes1000000 = new Random(DateTimeOffset.Now.Millisecond).NextBytes(1000000);
        private static byte[] bytes = new byte[] { 219, 75, 22, 0, 19, 128, 127, 240 };

    }
}
