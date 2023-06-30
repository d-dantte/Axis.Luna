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


        #region ToBits
        //[Benchmark]
        public void ToBits10()
        {
            var t = Common.BitSequence.ToBits(Bytes10);
        }

        //[Benchmark]
        public void ToBits100()
        {

            var t = Common.BitSequence.ToBits(Bytes100);
        }

        //[Benchmark]
        public void ToBits1000()
        {
            var t = Common.BitSequence.ToBits(Bytes1000);
        }
        #endregion

        #region  ChunkTest
        [Benchmark]
        public void Chunk1_long()
        {
            var result = BitSequence2.Chunk(bytes, 3..52);
        }

        [Benchmark]
        public void Chunk1_same()
        {
            var result = BitSequence2.Chunk(bytes, 3..8);
        }

        [Benchmark]
        public void Chunk2_long()
        {
            var result = BitSequence2.Chunk2(bytes, 3..52);
        }

        [Benchmark]
        public void Chunk2_same()
        {
            var result = BitSequence2.Chunk2(bytes, 3..8);
        }
        #endregion
    }
}
