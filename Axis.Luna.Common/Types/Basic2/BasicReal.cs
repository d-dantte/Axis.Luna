using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    public partial interface IBasicValue
    {
        public struct BasicReal : IBasicValue
        {
            private readonly Metadata[] _metadata;

            public BasicTypes Type => BasicTypes.Real;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public double? Value { get; }

            internal BasicReal(double? value, params Metadata[] metadata)
            {
                Value = value;
                _metadata = metadata?.ToArray();
            }

            public override bool Equals(object obj)
                => obj is BasicReal other
                 && other.Value == Value;

            public override int GetHashCode() => Value?.GetHashCode() ?? 0;

            public override string ToString() => Value?.ToString();


            public static bool operator ==(BasicReal first, BasicReal second) => first.Value == second.Value;

            public static bool operator !=(BasicReal first, BasicReal second) => !(first == second);
        }
    }
}
