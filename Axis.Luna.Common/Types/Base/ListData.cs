using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Types.Base
{
    /// <summary>
    /// Note that this class is a container of data. You cannot manipulate the list it contains via this class. You must first get the list, manipulate, then set it back.
    /// </summary>
    public class ListData : IDataType<IEnumerable<DataType>>
    {
        private List<DataType> _list = null;

        public override DataTypes Type => DataTypes.List;

        public override IEnumerable<DataType> Value
        {
            get => _list?.AsEnumerable();
            set =>_list = value?.ToList();
        }

        public int? Count => _list?.Count;


        public ListData()
        {
        }

        public ListData(params DataType[] data)
        {
            Value = data;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ListData list))
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


        public static bool operator ==(ListData first, ListData second)
        {
            if (first == null && second == null)
                return true;

            else 
                return first?.Equals(second) == true;
        }

        public static bool operator !=(ListData first, ListData second) => !(first == second);
    }
}
