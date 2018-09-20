﻿using System;
using System.Collections.Generic;
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
                case CommonDataType.Tags: return typeof(string);
                case CommonDataType.TimeSpan: return typeof(TimeSpan);
                case CommonDataType.UnknownType: return typeof(string);
                case CommonDataType.Url: return typeof(Uri);
                default: throw new Exception($"unknown type {type}");
            }
        }

        private static object ConvertValue(string value)
        {
            value = value?.Trim('\'');
            if (long.TryParse(value, out long lval)) return lval;
            else if (decimal.TryParse(value, out decimal dval)) return dval;
            else if (double.TryParse(value, out double ddval)) return ddval;
            else if (Guid.TryParse(value, out Guid gval)) return gval;
            else if (DateTimeOffset.TryParse(value, out DateTimeOffset dtoval)) return dtoval;
            else if (TimeSpan.TryParse(value, out TimeSpan tsval)) return tsval;
            else if (bool.TryParse(value, out bool bval)) return bval;
            else return value;
        }

        private static V ConvertValue<V>(string value) => (V) ConvertValue(value);
    }
}
