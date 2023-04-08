using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Common.Test
{
    [TestClass]
    public class BigDecimalTests
    {
        [TestMethod]
        public void DecimalTest() 
        {
            var x = DoubleConverter.Deconstruct(3.001d);
            Console.WriteLine(x);
        }
    }
}
