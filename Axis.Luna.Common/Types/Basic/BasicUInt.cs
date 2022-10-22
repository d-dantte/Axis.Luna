using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicUInt : IBasicValue<ulong?>
    {
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.Int;

        public ulong? Value { get; }

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicUInt(ulong? value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicUInt(ulong? value, params BasicMetadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
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
