using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    public partial interface IBasicValue
    {
        public readonly struct BasicTimeSpan : IBasicValue
        {
            private readonly Metadata[] _metadata;

            public BasicTypes Type => BasicTypes.TimeSpan;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public TimeSpan? Value { get; }

            internal BasicTimeSpan(TimeSpan? value, params Metadata[] metadata)
            {
                Value = value;
                _metadata = metadata?.ToArray();
            }

            public override bool Equals(object obj)
                => obj is BasicTimeSpan other
                 && other.Value == Value;

            public override int GetHashCode() => Value?.GetHashCode() ?? 0;

            public override string ToString() => Value.ToString();


            public static bool operator ==(BasicTimeSpan first, BasicTimeSpan second) => first.Value == second.Value;

            public static bool operator !=(BasicTimeSpan first, BasicTimeSpan second) => !(first == second);
        }
    }
}
