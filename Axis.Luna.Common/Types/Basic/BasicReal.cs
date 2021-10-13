using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicReal : IBasicValue<double?>
    {
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.Real;

        public double? Value { get; }

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicReal(double? value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicReal(double? value, params BasicMetadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
            => obj is BasicReal other
             && other.Value == Value;

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override string ToString() => Value?.ToString();


        public static bool operator ==(BasicReal first, BasicReal second) => first.Value == second.Value;

        public static bool operator !=(BasicReal first, BasicReal second) => !(first == second);
    }
}
