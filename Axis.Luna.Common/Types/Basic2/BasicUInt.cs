using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    public partial interface IBasicValue
    {
        public readonly struct BasicUInt : IBasicValue
        {
            private readonly Metadata[] _metadata;

            public BasicTypes Type => BasicTypes.UInt;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public ulong? Value { get; }

            internal BasicUInt(ulong? value, params Metadata[] metadata)
            {
                Value = value;
                _metadata = metadata?.ToArray();
            }

            public override bool Equals(object obj)
                => obj is BasicUInt other
                 && other.Value == Value;

            public override int GetHashCode() => Value?.GetHashCode() ?? 0;

            public override string ToString() => Value?.ToString();


            public static bool operator ==(BasicUInt first, BasicUInt second) => first.Value == second.Value;

            public static bool operator !=(BasicUInt first, BasicUInt second) => !(first == second);
        }
    }
}
