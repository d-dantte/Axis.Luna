using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Common.Benchmarks.Numerics
{
    public class BigDecimalX
    {
        [Benchmark]
        public void DecimalShift_1()
        {
            _ = Common.Numerics.BigDecimal.DecimalShift(123456789, 1);
        }

        [Benchmark]
        public void DecimalShiftX_1()
        {
            _ = Common.Numerics.BigDecimal.PowerShift(123456789, 1);
        }

        [Benchmark]
        public void DecimalShift_10()
        {
            _ = Common.Numerics.BigDecimal.DecimalShift(123456789, 10);
        }

        [Benchmark]
        public void DecimalShiftX_10()
        {
            _ = Common.Numerics.BigDecimal.PowerShift(123456789, 10);
        }

        [Benchmark]
        public void DecimalShift_50()
        {
            _ = Common.Numerics.BigDecimal.DecimalShift(123456789, 50);
        }

        [Benchmark]
        public void DecimalShiftX_50()
        {
            _ = Common.Numerics.BigDecimal.PowerShift(123456789, 50);
        }

        [Benchmark]
        public void DecimalShift_350()
        {
            _ = Common.Numerics.BigDecimal.DecimalShift(123456789, 350);
        }

        [Benchmark]
        public void DecimalShiftX_350()
        {
            _ = Common.Numerics.BigDecimal.PowerShift(123456789, 350);
        }

        [Benchmark]
        public void DecimalShift__1()
        {
            _ = Common.Numerics.BigDecimal.DecimalShift(123456789, -1);
        }

        [Benchmark]
        public void DecimalShiftX__1()
        {
            _ = Common.Numerics.BigDecimal.PowerShift(123456789, -1);
        }

        [Benchmark]
        public void DecimalShift__5()
        {
            _ = Common.Numerics.BigDecimal.DecimalShift(123456789, -5);
        }

        [Benchmark]
        public void DecimalShiftX__5()
        {
            _ = Common.Numerics.BigDecimal.PowerShift(123456789, -5);
        }

        [Benchmark]
        public void DecimalShift__12()
        {
            _ = Common.Numerics.BigDecimal.DecimalShift(123456789123456789L, -12);
        }

        [Benchmark]
        public void DecimalShiftX__12()
        {
            _ = Common.Numerics.BigDecimal.PowerShift(123456789123456789L, -12);
        }

        [Benchmark]
        public void DecimalShift__50()
        {
            _ = Common.Numerics.BigDecimal.DecimalShift(123456789123456789L, -50);
        }

        [Benchmark]
        public void DecimalShiftX__50()
        {
            _ = Common.Numerics.BigDecimal.PowerShift(123456789123456789L, -50);
        }
    }
}
