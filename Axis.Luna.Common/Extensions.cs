using Axis.Luna.Common.Types;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Axis.Luna.Common
{
    [Obsolete]
    public static class Extensions
    {
        private static readonly DateTimeFormatInfo DateTimeFormatInfo = new CultureInfo("en-GB").DateTimeFormat;

        /// <summary>
        /// NOTE: this method relies on a conversion method that in turn uses the "en-GB" data format to convert date-time. "dd/MM/yyyy"
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
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

        /// <summary>
        /// NOTE: this method relies on a conversion method that in turn uses the "en-GB" data format to convert date-time. "dd/MM/yyyy"
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="line"></param>
        /// <returns></returns>
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

        /// <summary>
        /// NOTE: this method relies on a conversion method that in turn uses the "en-GB" data format to convert date-time. "dd/MM/yyyy"
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static string ToCSV(this IEnumerable<object> objects)
        {
            return objects
                .Select(_obj =>
                {
                    var objType = _obj.GetType();
                    if (objType.IsNumeric()
                     || typeof(bool?).IsAssignableFrom(objType)
                     || typeof(Guid?).IsAssignableFrom(objType))
                        return _obj.ToString();

                    else
                        return $"'{_obj}'";
                })
                .JoinUsing(",")
                .ApplyTo(_line => $"[{_line}]");
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
                case CommonDataType.IPV6: return typeof(IPAddress);
                case CommonDataType.StructMap: return typeof(string);
                case CommonDataType.Location: return typeof(GeoCoordinate);
                case CommonDataType.Phone: return typeof(string);
                case CommonDataType.Real: return typeof(double);
                case CommonDataType.String: return typeof(string);
                case CommonDataType.NVP: return typeof(KeyValuePair<string, string>);
                case CommonDataType.TimeSpan: return typeof(TimeSpan);
                case CommonDataType.UnknownType: return typeof(string);
                case CommonDataType.UnsignedInteger: return typeof(ulong);
                case CommonDataType.Url: return typeof(Uri);
                default: throw new Exception($"unknown type {type}");
            }
        }

        public static object ClrValue(this DataItem dataItem)
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
                case CommonDataType.IPV6: return IPAddress.Parse(dataItem.Data);
                case CommonDataType.StructMap: return dataItem.Data;
                case CommonDataType.Location: return GeoCoordinate.Parse(dataItem.Data);
                case CommonDataType.Phone: return dataItem.Data;
                case CommonDataType.Real: return double.Parse(dataItem.Data);
                case CommonDataType.String: return dataItem.Data;
                case CommonDataType.NVP: return dataItem.ParseNVPairs();
                case CommonDataType.TimeSpan: return TimeSpan.Parse(dataItem.Data);
                case CommonDataType.Url: return new Uri(dataItem.Data?.Trim() ?? throw new Exception("Invalid Uri"));
                case CommonDataType.UnsignedInteger: return ulong.Parse(dataItem.Data);
                case CommonDataType.UnknownType:
                default: return dataItem.Data;
            }
        }

        public static CommonDataType CommonType(this Type type)
        {
            if (typeof(byte?).IsAssignableFrom(type))
                return CommonDataType.Binary;

            else if (typeof(bool?).IsAssignableFrom(type))
                return CommonDataType.Boolean;

            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                return CommonDataType.CSV;

            else if (typeof(DateTimeOffset?).IsAssignableFrom(type))
                return CommonDataType.DateTime;

            else if (typeof(decimal?).IsAssignableFrom(type))
                return CommonDataType.Decimal;

            else if (typeof(Guid?).IsAssignableFrom(type))
                return CommonDataType.Boolean;

            else if (typeof(int?).IsAssignableFrom(type)
                || typeof(short?).IsAssignableFrom(type)
                || typeof(long?).IsAssignableFrom(type))
                return CommonDataType.Integer;

            else if (typeof(IPAddress).IsAssignableFrom(type))
                return CommonDataType.IPV4;

            else if (typeof(float?).IsAssignableFrom(type)
                || typeof(double?).IsAssignableFrom(type))
                return CommonDataType.Real;

            else if (typeof(TimeSpan?).IsAssignableFrom(type))
                return CommonDataType.TimeSpan;

            else if (typeof(Uri).IsAssignableFrom(type))
                return CommonDataType.Url;

            else if (typeof(string).IsAssignableFrom(type))
                return CommonDataType.String;

            else return CommonDataType.StructMap;

        }

        private static object ConvertValue(string value)
        {
            value = value?.Trim('\'');
            if (long.TryParse(value, out var longValue)) return longValue;
            else if (decimal.TryParse(value, out var decimalValue)) return decimalValue;
            else if (double.TryParse(value, out var doubleValue)) return doubleValue;
            else if (Guid.TryParse(value, out var guidValue)) return guidValue;
            else if (DateTimeOffset.TryParse(value, DateTimeFormatInfo, DateTimeStyles.None, out var dateTimeOffsetValue)) return dateTimeOffsetValue;
            else if (TimeSpan.TryParse(value, out var timespanValue)) return timespanValue;
            else if (bool.TryParse(value, out var boolValue)) return boolValue;
            else if (IPAddress.TryParse(value, out var ipaddressValue)) return ipaddressValue;
            else return value;
        }

        private static V ConvertValue<V>(string value) => (V) ConvertValue(value);

        private static IEnumerable<KeyValuePair<string, string>> ParseNVPairs(this DataItem dataItem)
        {
            if (dataItem == null)
                throw new ArgumentNullException("data cannot be null");

            return dataItem.Data
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Select(ParseNVPair);
        }

        private static KeyValuePair<string, string> ParseNVPair(this string pair)
        {
            if (pair == null)
                throw new ArgumentNullException("pair cannot be null");

            var values = pair.Split(':');

            if (values.Length == 0 
                || values.Length > 2
                || values.Contains(string.Empty))
                throw new FormatException("Input string is in the wrong format");

            return new KeyValuePair<string, string>(
                values[0],
                values.Length == 2 ? values[1] : null);
        }
    }
}