using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    public partial interface IBasicValue
    {
        public struct BasicInt : IBasicValue
        {
            private readonly Metadata[] _metadata;

            public BasicTypes Type => BasicTypes.Int;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public long? Value { get; }

            internal BasicInt(long? value, params Metadata[] metadata)
            {
                Value = value;
                _metadata = metadata?.ToArray();
            }

            public override bool Equals(object obj)
                => obj is BasicInt other
                 && other.Value == Value;

            public override int GetHashCode() => Value?.GetHashCode() ?? 0;

            public override string ToString() => Value?.ToString();


            public static bool operator ==(BasicInt first, BasicInt second) => first.Value == second.Value;

            public static bool operator !=(BasicInt first, BasicInt second) => !(first == second);
        }
    }
}
