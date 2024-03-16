namespace Axis.Luna.Common.Benchmarks
{
    public static class Extensions
    {
        public static byte[] NextBytes(this Random random, int byteCount)
        {
            var bytes = new byte[byteCount];
            random.NextBytes(bytes);
            return bytes;
        }
    }
}
