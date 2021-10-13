using Axis.Luna.Extensions;
using System;
using System.Linq;

namespace Axis.Luna.Common.Types
{
    [Obsolete]
    public struct DataItem
    {
        /// <summary>
        /// Serializes the data-item using css-like syntax
        /// </summary>
        private static readonly Func<DataItem, string> _serializer = item =>
        {
            return $"Type:{item.Type}; Name:{item.Name}; Data:{Encode(item.Data)};";
        };

        /// <summary>
        /// Deserialize the data-item using css-like syntax
        /// </summary>
        private static readonly Func<string, DataItem> _deserializer = @string =>
        {
            if (string.IsNullOrWhiteSpace(@string))
                return default;

            return @string
                .Split(';')
                .Select(part => part.Trim().Split(':'))
                .ToDictionary(
                    keySelector: prop => prop[0],
                    elementSelector: prop => prop[1])
                .ApplyTo(props => new DataItem(
                    type: props[nameof(Type)].ParseEnum<CommonDataType>(),
                    name: props[nameof(Name)],
                    data: Decode(props.GetOrDefault(nameof(Data)))));
        };

        /// <summary>
        /// The name for this data item
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The string representation of the data
        /// </summary>
        public string Data { get; private set; }

        /// <summary>
        /// The data-type
        /// </summary>
        public CommonDataType Type { get; private set; }


        public DataItem(CommonDataType type, string name, string data)
        {
            Type = type;
            Name = name.ThrowIf(string.IsNullOrWhiteSpace, new ArgumentException(nameof(name)));
            Data = data;
        }

        public DataItem(CommonDataType type, string name, object data)
        {
            Type = type;
            Name = name.ThrowIf(string.IsNullOrWhiteSpace, new ArgumentException(nameof(name)));
            Data = data?.ToString();
        }


        public string ToString(Func<DataItem, string> serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            return serializer.Invoke(this);
        }

        public override string ToString() => _serializer.Invoke(this);

        public override bool Equals(object obj)
        {
            return obj is DataItem other
                && other.Name.NullOrEquals(Name)
                && other.Data.NullOrEquals(Data)
                && other.Type.Equals(Type);
        }

        public override int GetHashCode() => HashCode.Combine(Type, Name, Data);

        public static bool operator==(DataItem first, DataItem second) => first.Equals(second);

        public static bool operator !=(DataItem first, DataItem second) => !(first.Equals(second));


        public static DataItem Parse(string @string) => _deserializer.Invoke(@string);

        public static DataItem Parse(string @string, Func<string, DataItem> parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            return parser.Invoke(@string);
        }


        public static bool TryParse(string @string, out DataItem item)
        {
            try
            {
                item = _deserializer.Invoke(@string);
                return true;
            }
            catch
            {
                item = default;
                return false;
            }
        }

        public static bool TryParse(string @string, Func<string, DataItem> parser, out DataItem item)
        {
            item = default;
            try
            {
                if (parser == null)
                    return false;

                item = parser.Invoke(@string);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string Decode(string encodedData)
        {
            return encodedData?
                .Replace("@3", ";")
                .Replace("@2", ";")
                .Replace("@1", "@");
        }

        public static string Encode(string decodedData)
        {
            return decodedData?
                .Replace(";", "@3")
                .Replace(":", "@2")
                .Replace("@", "@1");
        }
    }
}
