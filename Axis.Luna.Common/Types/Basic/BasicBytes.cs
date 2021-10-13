using Axis.Luna.Extensions;
using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicBytes : IBasicValue<byte[]>
    {
        private readonly BasicMetadata[] _metadata;
        private readonly byte[] _bytes;
        private readonly int _hashCode;

        public BasicTypes Type => BasicTypes.Bytes;

        public byte[] Value => _bytes?.ToArray();

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicBytes(byte[] value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicBytes(byte[] value, params BasicMetadata[] metadata)
        {

            _bytes = value?.ToArray();
            _hashCode = Luna.Extensions.Common.ValueHash(_bytes);
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
            => obj is BasicBytes other
             && other.Value.NullOrTrue(Value, System.Linq.Enumerable.SequenceEqual);

        public override int GetHashCode() => _hashCode;

        public override string ToString() => Value?.ToString();


        public static bool operator ==(BasicBytes first, BasicBytes second) => first.Equals(second);

        public static bool operator !=(BasicBytes first, BasicBytes second) => !(first == second);
    }
}
