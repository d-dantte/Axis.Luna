using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicString : IBasicValue<string>
    {
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.String;

        public string Value { get; }

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicString(string value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicString(string value, params BasicMetadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
            => obj is BasicString other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;


        public static bool operator ==(BasicString first, BasicString second)
        {
            return first.Value?.Equals(second.Value, StringComparison.InvariantCulture) == true;
        }

        public static bool operator !=(BasicString first, BasicString second) => !(first == second);
    }
}
