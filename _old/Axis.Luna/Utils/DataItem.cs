using System;
using static Axis.Luna.Extensions.ObjectExtensions;
using static Axis.Luna.Extensions.EnumerableExtensions;
using Axis.Luna.Operation;

namespace Axis.Luna.Utils
{
    public interface IDataItem
    {
        string Name { get; set; }

        string Data { get; set; }

        CommonDataType Type { get; set; }
    }

    public class DataItem: IDataItem
    {
        public string Name { get; set; }

        public string Data { get; set; }

        public CommonDataType Type { get; set; }

        public override int GetHashCode() => ValueHash(Name.Enumerate());
        public override bool Equals(object obj)
            => obj.Cast<DataItem>()
                  .Pipe(_da => _da.Name == Name && _da.Data == Data);

        public override string ToString() => $"[{Name}: {this.DisplayData()}]";
    }


    public static class DataItemHelper
    {
        public static readonly string DefaultDateTimeFormat = "yyy/MMM/dd HH:mm:ss";


        public static string DisplayData(this IDataItem @this)
        {
            switch (@this.Type)
            {
                case CommonDataType.Boolean:
                case CommonDataType.Real:
                case CommonDataType.Decimal:
                case CommonDataType.Integer:
                case CommonDataType.String:
                case CommonDataType.Url:
                case CommonDataType.IPV4:
                case CommonDataType.IPV6:
                case CommonDataType.Phone:
                case CommonDataType.Email:
                case CommonDataType.Location:
                case CommonDataType.TimeSpan:
                case CommonDataType.Guid:
                case CommonDataType.JsonObject: return @this.Data;
                case CommonDataType.DateTime: return ResolvedOp.Try(() => DateTime.Parse(@this.Data).ToString(DefaultDateTimeFormat)).Result;

                case CommonDataType.Tags: return TagBuilder.Create(@this.Data).ToString();

                case CommonDataType.Binary: return "B64-Encoded";

                case CommonDataType.UnknownType:
                default: return "Unknown-Type";
            }
        }

        public static object ParseData(this IDataItem @this, Func<string, object> converter = null)
        {
            switch (@this.Type)
            {
                case CommonDataType.Boolean: return bool.Parse(@this.Data);
                case CommonDataType.Real: return double.Parse(@this.Data);
                case CommonDataType.Decimal: return decimal.Parse(@this.Data);
                case CommonDataType.Integer: return long.Parse(@this.Data);
                case CommonDataType.Url: return new Uri(@this.Data);
                case CommonDataType.String: return @this.Data;

                case CommonDataType.TimeSpan: return TimeSpan.Parse(@this.Data);
                case CommonDataType.DateTime: return DateTime.Parse(@this.Data);
                case CommonDataType.Binary: return Convert.FromBase64String(@this.Data);

                case CommonDataType.Tags: return TagBuilder.Create(@this.Data).Tags;

                case CommonDataType.IPV4:
                case CommonDataType.IPV6:
                case CommonDataType.Phone:
                case CommonDataType.Email:
                case CommonDataType.Location:
                case CommonDataType.Guid: return Guid.Parse(@this.Data);
                case CommonDataType.UnknownType: return converter?.Invoke(@this.Data) ?? @this.Data;

                case CommonDataType.JsonObject: return converter.Invoke(@this.Data);

                default: throw new Exception($"unknown type: {@this.Type}");
            }
        }

        public static T ParseData<T>(this IDataItem @this, Func<string, T> converter = null)
        {
            if (@this.Type == CommonDataType.JsonObject)
                return converter.Invoke(@this.Data);

            else if (@this.Type == CommonDataType.Tags)
                return (T)(TagBuilder.Parse(@this.Data) as object);

            else if (converter == null)
                return (T)@this.ParseData();

            else
                return (T)@this.ParseData(_data => converter.Invoke(_data).Cast<object>());
        }
    }
}
