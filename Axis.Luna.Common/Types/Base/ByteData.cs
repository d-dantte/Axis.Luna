using System.Linq;

namespace Axis.Luna.Common.Types.Base
{
    public class ByteData : IDataType<byte[]>
    {
        public override DataTypes Type => DataTypes.Bytes;

        public override byte[] Value { get; set; }

        public override bool Equals(object obj)
            => obj is ByteData other
             && other.Value.SequenceEqual(Value);

        public override int GetHashCode() => Luna.Extensions.Common.ValueHash(Value);

        public override string ToString() => Value.ToString();


        public static bool operator ==(ByteData first, ByteData second)
            => (first == null && second == null)
                || first?.Equals(second) == true;

        public static bool operator !=(ByteData first, ByteData second) => !(first == second);
    }
}
