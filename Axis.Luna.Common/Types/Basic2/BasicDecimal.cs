using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    public partial interface IBasicValue
    {
        public struct BasicDecimal : IBasicValue
        {
            private readonly Metadata[] _metadata;

            public BasicTypes Type => BasicTypes.Decimal;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public decimal? Value { get; }

            internal BasicDecimal(decimal? value, params Metadata[] metadata)
            {
                Value = value;
                _metadata = metadata?.ToArray();
            }

            public override bool Equals(object obj)
                => obj is BasicDecimal other
                 && other.Value == Value;

            public override int GetHashCode() => Value?.GetHashCode() ?? 0;

            public override string ToString() => Value?.ToString();


            public static bool operator ==(BasicDecimal first, BasicDecimal second) => first.Value == second.Value;

            public static bool operator !=(BasicDecimal first, BasicDecimal second) => !(first == second);
        }
    }
}
