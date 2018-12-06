using Axis.Luna.Common.Contracts;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axis.Luna.Common
{
    public static class Extensions
    {
        public static IEnumerable<object> ParseLineAsCSV(this string line)
        {
            var valueString = new StringBuilder();
            var isQuoting = false;
            foreach(var ch in line)
            {
                if (!isQuoting && ch == ',') //end of value
                {
                    var value = valueString.ToString();
                    valueString.Clear();
                    yield return ConvertValue(value);
                }

                else if (ch == '\'' && valueString.Length == 0) //start quoting
                {
                    isQuoting = true;
                    valueString.Append(ch);
                }

                else if (isQuoting && ch == '\'') //stop quoting
                {
                    isQuoting = false;
                    valueString.Append(ch);
                }

                else valueString.Append(ch);
            }

            if (isQuoting)
                throw new Exception("No closing quote mark found");

            else if (valueString.Length > 0)
                yield return ConvertValue(valueString.ToString());
        }

        public static IEnumerable<V> ParseLineAsCSV<V>(this string line)
        {
            var valueString = new StringBuilder();
            var isQuoting = false;
            foreach (var ch in line)
            {
                if (!isQuoting && ch == ',') //end of value
                {
                    var value = valueString.ToString();
                    valueString.Clear();
                    yield return ConvertValue<V>(value);
                }

                else if (ch == '\'' && valueString.Length == 0) //start quoting
                {
                    isQuoting = true;
                    valueString.Append(ch);
                }

                else if (isQuoting && ch == '\'') //stop quoting
                {
                    isQuoting = false;
                    valueString.Append(ch);
                }

                else valueString.Append(ch);
            }

            if (isQuoting)
                throw new Exception("No closing quote mark found");

            else if (valueString.Length > 0)
                yield return ConvertValue<V>(valueString.ToString());
        }

        public static string ToCSV(this IEnumerable<object> objects)
        {
            return objects
                .Select(_obj =>
                {
                    if (_obj.GetType().IsNumeric()
                     || _obj is bool)
                        return _obj.ToString();

                    else
                        return $"'{_obj}'";
                })
                .JoinUsing(",")
                .Pipe(_line => $"[{_line}]");
        }

        public static Type ClrType(this CommonDataType type)
        {
            switch(type)
            {
                case CommonDataType.Binary: return typeof(byte);
                case CommonDataType.Boolean: return typeof(bool);
                case CommonDataType.CSV: return typeof(IEnumerable<object>);
                case CommonDataType.DateTime: return typeof(DateTimeOffset);
                case CommonDataType.Decimal: return typeof(decimal);
                case CommonDataType.Email: return typeof(string);
                case CommonDataType.Guid: return typeof(string);
                case CommonDataType.Integer: return typeof(long);
                case CommonDataType.IPV4: return typeof(string);
                case CommonDataType.IPV6: return typeof(string);
                case CommonDataType.JsonObject: return typeof(string);
                case CommonDataType.Location: return typeof(string);
                case CommonDataType.Phone: return typeof(string);
                case CommonDataType.Real: return typeof(double);
                case CommonDataType.String: return typeof(string);
                case CommonDataType.NVP: return typeof(string);
                case CommonDataType.TimeSpan: return typeof(TimeSpan);
                case CommonDataType.UnknownType: return typeof(string);
                case CommonDataType.Url: return typeof(Uri);
                default: throw new Exception($"unknown type {type}");
            }
        }

        public static object ClrValue(this IDataItem dataItem)
        {
            if (dataItem == null) return null;
            switch(dataItem.Type)
            {
                case CommonDataType.Binary: return Convert.FromBase64String(dataItem.Data);
                case CommonDataType.Boolean: return bool.Parse(dataItem.Data);
                case CommonDataType.CSV: return dataItem.Data.ParseLineAsCSV();
                case CommonDataType.DateTime: return DateTimeOffset.Parse(dataItem.Data);
                case CommonDataType.Decimal: return decimal.Parse(dataItem.Data);
                case CommonDataType.Email: return dataItem.Data;
                case CommonDataType.Guid: return Guid.Parse(dataItem.Data);
                case CommonDataType.Integer: return long.Parse(dataItem.Data);
                case CommonDataType.IPV4:
                case CommonDataType.IPV6: return dataItem.Data;
                case CommonDataType.JsonObject: return dataItem.Data;
                case CommonDataType.Location: return dataItem.Data;
                case CommonDataType.Phone: return dataItem.Data;
                case CommonDataType.Real: return double.Parse(dataItem.Data);
                case CommonDataType.String: return dataItem.Data;
                case CommonDataType.NVP: return dataItem.SerializeTag();
                case CommonDataType.TimeSpan: return TimeSpan.Parse(dataItem.Data);
                case CommonDataType.Url: return new Uri(dataItem.Data?.Trim() ?? throw new Exception("Invalid Uri"));
                case CommonDataType.UnknownType:
                default: return dataItem.Data;
            }
        }

        private static object ConvertValue(string value)
        {
            value = value?.Trim('\'');
            if (long.TryParse(value, out var longValue)) return longValue;
            else if (decimal.TryParse(value, out var decimalValue)) return decimalValue;
            else if (double.TryParse(value, out var doubleValue)) return doubleValue;
            else if (Guid.TryParse(value, out var guidValue)) return guidValue;
            else if (DateTimeOffset.TryParse(value, out var dateTimeOffsetValue)) return dateTimeOffsetValue;
            else if (TimeSpan.TryParse(value, out var timespanValue)) return timespanValue;
            else if (bool.TryParse(value, out var boolValue)) return boolValue;
            else return value;
        }

        private static V ConvertValue<V>(string value) => (V) ConvertValue(value);

        #region Tags
        private static string EncodeTagValue(string decoded)
        {
            return decoded
                .Replace(";", "&scol")
                .Replace(":", "&col");
        }
        private static string DecodeTagValue(string encoded)
        {
            return encoded
                .Replace("&scol", ";")
                .Replace("&col", ":");
        }

        public static IEnumerable<Tag> ParseTags<Tag>(this string serializedTags)
        where Tag: IDataItem, new()
        {
            var tagType = typeof(Tag);

            var tagProperties = tagType
                .GetProperties()
                .Select(_prop => _prop.Name.ValuePair(_prop))
                .ToDictionary();

            return serializedTags?
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseTag<Tag>);
        }
        public static bool TryParseTags<Tag>(this string serializedTags, out IEnumerable<Tag> tags)
        where Tag: IDataItem, new()
        {
            try
            {
                tags = ParseTags<Tag>(serializedTags);
                return true;
            }
            catch
            {
                tags = null;
                return false;
            }
        }

        public static Tag ParseTag<Tag>(this string serializedTag)
        where Tag: IDataItem, new()
        {
            var tuples = serializedTag
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(DecodeTagValue)
                .ToArray();

            var tag = new Tag();
            tag.Initialize(tuples);

            return tag;
        }
        public static bool TryParseTag<Tag>(this string serializedTag, out IDataItem tag)
        where Tag: IDataItem, new()
        {
            try
            {
                tag = ParseTag<Tag>(serializedTag);
                return true;
            }
            catch
            {
                tag = null;
                return false;
            }
        }

        public static string SerializeTag(this IDataItem dataItem)
        {
            return dataItem
                .Tupulize()
                .Select(EncodeTagValue)
                .JoinUsing("|");
        }

        public static string SerializeTags(this IEnumerable<IDataItem> dataItems)
        {
            return dataItems
                .Select(SerializeTag)
                .JoinUsing("");
        }
        #endregion
    }
}