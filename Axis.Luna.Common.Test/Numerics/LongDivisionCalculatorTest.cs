using Axis.Luna.Common.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Common.Test.Numerics
{
    [TestClass]
    public class LongDivisionCalculatorTest
    {
        [TestMethod]
        public void Divide_With()
        {
            #region 2/1
            var calc = new LongDivisionMechine(2, 1);
            var result = calc.Divide();
            Assert.AreEqual(new BigDecimal(2), result);
            #endregion

            #region 100/4
            calc = new LongDivisionMechine(100, 4);
            result = calc.Divide();
            Assert.AreEqual(new BigDecimal(25), result);
            #endregion

            #region 4/5
            calc = new LongDivisionMechine(4, 5);
            result = calc.Divide();
            Assert.AreEqual(new BigDecimal(8, 1), result);
            #endregion
        }
    }
}
