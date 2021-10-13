using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    /// <summary>
    /// Represents an immutable list.
    /// </summary>
    public struct BasicList : IBasicValue<IEnumerable<BasicValue>>
    {
        private readonly List<BasicValue> _list;
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.List;

        public IEnumerable<BasicValue> Value => _list.AsEnumerable();

        public int? Count => _list?.Count;

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicList(params BasicValue[] data)
            : this((IEnumerable<BasicValue>)data)
        {
        }

        public BasicList(IEnumerable<BasicValue> value)
            : this(value, Array.Empty<BasicMetadata>())
        {
        }

        public BasicList(IEnumerable<BasicValue> value, params BasicMetadata[] metadata)
        {
            _list = value?.ToList();
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BasicList list))
                return false;

            if (list._list == null && _list == null)
                return true;

            if (list._list?.Count != _list?.Count)
                return false;

            return _list.SequenceEqual(list._list);
        }

        public override int GetHashCode()
        {
            if (_list == null)
                return 0;

            else return Luna.Extensions.Common.ValueHash(_list.ToArray());
        }

        public override string ToString() => Value.ToString();


        public static bool operator ==(BasicList first, BasicList second) => first.Equals(second) == true;

        public static bool operator !=(BasicList first, BasicList second) => !(first == second);
    }
}
