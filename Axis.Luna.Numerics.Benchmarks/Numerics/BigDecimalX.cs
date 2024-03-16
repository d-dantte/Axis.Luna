using BenchmarkDotNet.Attributes;

namespace Axis.Luna.Numerics.Benchmarks
{
    public class BigDecimalX
    {
        [Benchmark]
        public void DecimalShift_1()
        {
            _ = BigDecimal.DecimalShift(123456789, 1);
        }

        [Benchmark]
        public void DecimalShiftX_1()
        {
            _ = BigDecimal.PowerShift(123456789, 1);
        }

        [Benchmark]
        public void DecimalShift_10()
        {
            _ = BigDecimal.DecimalShift(123456789, 10);
        }

        [Benchmark]
        public void DecimalShiftX_10()
        {
            _ = BigDecimal.PowerShift(123456789, 10);
        }

        [Benchmark]
        public void DecimalShift_50()
        {
            _ = BigDecimal.DecimalShift(123456789, 50);
        }

        [Benchmark]
        public void DecimalShiftX_50()
        {
            _ = BigDecimal.PowerShift(123456789, 50);
        }

        [Benchmark]
        public void DecimalShift_350()
        {
            _ = BigDecimal.DecimalShift(123456789, 350);
        }

        [Benchmark]
        public void DecimalShiftX_350()
        {
            _ = BigDecimal.PowerShift(123456789, 350);
        }

        [Benchmark]
        public void DecimalShift__1()
        {
            _ = BigDecimal.DecimalShift(123456789, -1);
        }

        [Benchmark]
        public void DecimalShiftX__1()
        {
            _ = BigDecimal.PowerShift(123456789, -1);
        }

        [Benchmark]
        public void DecimalShift__5()
        {
            _ = BigDecimal.DecimalShift(123456789, -5);
        }

        [Benchmark]
        public void DecimalShiftX__5()
        {
            _ = BigDecimal.PowerShift(123456789, -5);
        }

        [Benchmark]
        public void DecimalShift__12()
        {
            _ = BigDecimal.DecimalShift(123456789123456789L, -12);
        }

        [Benchmark]
        public void DecimalShiftX__12()
        {
            _ = BigDecimal.PowerShift(123456789123456789L, -12);
        }

        [Benchmark]
        public void DecimalShift__50()
        {
            _ = BigDecimal.DecimalShift(123456789123456789L, -50);
        }

        [Benchmark]
        public void DecimalShiftX__50()
        {
            _ = BigDecimal.PowerShift(123456789123456789L, -50);
        }
    }
}
