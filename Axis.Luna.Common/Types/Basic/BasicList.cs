using Axis.Luna.Extensions;
using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    /// <summary>
    /// Represents an immutable list.
    /// </summary>
    public readonly struct BasicList : IBasicValue, IValueProvider<IBasicValue[]>
    {
        private readonly Metadata[] _metadata;
        private readonly IBasicValue[] _values;

        public BasicTypes Type => BasicTypes.List;

        public int Count => _values?.Length ?? 0;

        public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

        public IBasicValue[] Value => _values?.ToArray();

        internal BasicList(IBasicValue[] value, params Metadata[] metadata)
        {
            _values = value;
            _metadata = metadata?.ToArray();
        }

        internal BasicList(BasicValueWrapper[] value, params Metadata[] metadata)
            : this(value?.Select(v => v.Value).ToArray(), metadata)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is BasicList other)
            {
                // both default
                if (other._values == null && _values == null)
                    return true;

                return other.Count == Count
                    && other._values.SequenceEqual(_values);
            }

            return false;
        }

        public override int GetHashCode()
            => _values != null
                ? Luna.Extensions.Common.ValueHash(_values?.HardCast<IBasicValue, object>())
                : 0;

        public override string ToString() => Value.ToString();


        public static bool operator ==(BasicList first, BasicList second) => first.Equals(second) == true;

        public static bool operator !=(BasicList first, BasicList second) => !(first == second);
    }
}
