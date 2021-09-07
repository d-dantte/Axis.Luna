using Axis.Luna.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Types.Base
{
    public class StructData : IDataType<IEnumerable<KeyValuePair<string, DataType>>>
    {
        private readonly Dictionary<string, DataType> _properties = new Dictionary<string, DataType>();

        public override DataTypes Type => DataTypes.Struct;

        public override IEnumerable<KeyValuePair<string, DataType>> Value
        {
            get => _properties;
            set
            {
                _properties.Clear();
                value?.ForAll(kvp => _properties[kvp.Key] = kvp.Value);
            }
        }

        public DataType this[string key]
        {
            get => _properties[key];
            set => _properties[key] = value;
        }

        public string[] Keys => _properties.Keys.ToArray();

        public int Count => _properties.Count;

        public bool Remove(string key) => _properties.Remove(key);

        public bool TryGetValue(string key, out DataType value) => _properties.TryGetValue(key, out value);

        public StructData Append(string key, DataType value)
        {
            this[key] = value;
            return this;
        }

        public StructData Append(KeyValuePair<string, DataType> value) => Append(value.Key, value.Value);


        public override bool Equals(object obj)
        {
            if (!(obj is StructData other))
                return false;

            else if (Count != other.Count)
                return false;

            else if (!_properties.All(kvp =>
            {
                if (!other.TryGetValue(kvp.Key, out var value))
                    return false;

                else if (this[kvp.Key] == null && value == null)
                    return true;

                else
                    return this[kvp.Key]?.Equals(value) == true;
            }))
                return false;

            else return true;
        }

        public override int GetHashCode()
        {
            var keyHash = Luna.Extensions.Common.ValueHash(
                _properties.Keys.ToArray());

            var valueHash = Luna.Extensions.Common.ValueHash(
                _properties.Values.ToArray());

            return Luna.Extensions.Common.ValueHash(keyHash, valueHash);
        }

        public static bool operator ==(StructData first, StructData second)
        {
            if (first is null && second is null)
                return true;

            else 
                return first?.Equals(second) == true;
        }

        public static bool operator !=(StructData first, StructData second) => !(first == second);
    }
}
