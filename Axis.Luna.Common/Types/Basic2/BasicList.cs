using Axis.Luna.Extensions;
using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    public partial interface IBasicValue
    {
        /// <summary>
        /// Represents an immutable list.
        /// </summary>
        public readonly struct BasicList : IBasicValue
        {
            private readonly Metadata[] _metadata;
            private readonly IBasicValue[] _values;

            public BasicTypes Type => BasicTypes.List;

            public int Count => _metadata?.Length ?? 0;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public IBasicValue[] Value => _values?.ToArray();

            internal BasicList(IBasicValue[] value, params Metadata[] metadata)
            {
                _values = value;
                _metadata = metadata?.ToArray();
            }

            public override bool Equals(object obj)
            {
                return obj is BasicList other
                    && (other._values == null && _values == null)
                    && other.Count == Count
                    && _values.SequenceEqual(other._values);
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
}
