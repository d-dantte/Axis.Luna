using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicGuid : IBasicValue<Guid?>
    {
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.Guid;

        public Guid? Value { get; }

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicGuid(Guid? value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicGuid(Guid? value, params BasicMetadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
            => obj is BasicGuid other
             && other.Value == Value;

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override string ToString() => Value?.ToString();


        public static bool operator ==(BasicGuid first, BasicGuid second) => first.Value == second.Value;

        public static bool operator !=(BasicGuid first, BasicGuid second) => !(first == second);
    }
}
