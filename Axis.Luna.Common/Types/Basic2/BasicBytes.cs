using Axis.Luna.Extensions;
using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    public partial interface IBasicValue
    {
        public struct BasicBytes : IBasicValue
        {
            private readonly Metadata[] _metadata;
            private readonly byte[] _bytes;
            private readonly int _hashCode;

            public BasicTypes Type => BasicTypes.Bytes;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public byte[] Value => _bytes?.ToArray();

            internal BasicBytes(byte[] value, params Metadata[] metadata)
            {
                _bytes = value?.ToArray();
                _metadata = metadata?.ToArray();
                _hashCode = _bytes != null
                    ? HashCode.Combine(Luna.Extensions.Common.ValueHash(_bytes ?? Array.Empty<byte>()))
                    : 0;
            }

            public override bool Equals(object obj)
                => obj is BasicBytes other
                 && other.Value.NullOrTrue(Value, Enumerable.SequenceEqual);

            public override int GetHashCode() => _hashCode;

            public override string ToString() => Value?.ToString();


            public static bool operator ==(BasicBytes first, BasicBytes second) => first.Equals(second);

            public static bool operator !=(BasicBytes first, BasicBytes second) => !(first == second);
        }
    }
}
