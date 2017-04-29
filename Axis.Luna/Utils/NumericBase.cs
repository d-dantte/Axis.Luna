using Axis.Luna.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Utils
{
    public class NumericBase<Digit>
    {
        private List<Digit> _digits = new List<Digit>();

        public int Base { get; private set; }

        public NumericBase(Digit[] digits)
        {
            Base = digits.Length;
            _digits.AddRange(digits);
        }

        public IEnumerable<Digit> Convert(int value)
        {
            var list = new List<Digit>();
            Fraction f = null;
            int n = value;
            do
            {
                f = Base.Divide(n);
                list.Add(_digits[(int)f.Remainder]);
            }
            while ((n = (int)f.Multiples) > 0);

            return list.Reverse<Digit>();
        }
    }
}
