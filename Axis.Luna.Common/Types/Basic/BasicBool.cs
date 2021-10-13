using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicBool : IBasicValue<bool?>
    {
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.Bool;

        public bool? Value { get; }

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicBool(bool? value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicBool(bool? value, params BasicMetadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
            => obj is BasicBool other
             && other.Value == Value;

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override string ToString() => Value?.ToString();


        public static bool operator ==(BasicBool first, BasicBool second) => first.Value == second.Value;

        public static bool operator !=(BasicBool first, BasicBool second) => !(first == second);
    }
}
