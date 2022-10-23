using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public readonly struct BasicDate : IBasicValue
    {
        private readonly Metadata[] _metadata;

        public BasicTypes Type => BasicTypes.Date;

        public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

        public DateTimeOffset? Value { get; }

        internal BasicDate(DateTimeOffset? value, params Metadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.ToArray();
        }

        public override bool Equals(object obj)
            => obj is BasicDate other
             && other.Value == Value;

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override string ToString() => Value?.ToString();


        public static bool operator ==(BasicDate first, BasicDate second) => first.Value == second.Value;

        public static bool operator !=(BasicDate first, BasicDate second) => !(first == second);
    }
}
