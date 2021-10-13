using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicDecimal: IBasicValue<decimal?>
    {
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.Decimal;

        public decimal? Value { get; }

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicDecimal(decimal? value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicDecimal(decimal? value, params BasicMetadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
            => obj is BasicDecimal other
             && other.Value == Value;

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override string ToString() => Value?.ToString();


        public static bool operator ==(BasicDecimal first, BasicDecimal second) => first.Value == second.Value;

        public static bool operator !=(BasicDecimal first, BasicDecimal second) => !(first == second);
    }
}
