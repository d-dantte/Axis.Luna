using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicDateTime : IBasicValue<DateTimeOffset?>
    {
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.Date;

        public DateTimeOffset? Value { get; }

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicDateTime(DateTimeOffset? value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicDateTime(DateTimeOffset? value, params BasicMetadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
            => obj is BasicDateTime other
             && other.Value == Value;

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override string ToString() => Value?.ToString();


        public static bool operator ==(BasicDateTime first, BasicDateTime second) => first.Value == second.Value;

        public static bool operator !=(BasicDateTime first, BasicDateTime second) => !(first == second);
    }
}
