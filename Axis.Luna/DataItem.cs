using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axis.Luna.Extensions.ObjectExtensions;
using static Axis.Luna.Extensions.EnumerableExtensions;

namespace Axis.Luna
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
            => obj.As<DataItem>()
                  .Pipe(_da => _da.Name == Name && _da.Data == Data);

        public override string ToString() => $"[{Name}: {DisplayData()}]";


        private string DisplayData()
        {
            switch(Type)
            {
                case CommonDataType.Boolean:
                case CommonDataType.Real:
                case CommonDataType.Integer:
                case CommonDataType.String:
                case CommonDataType.Url:
                case CommonDataType.IPV4:
                case CommonDataType.IPV6:
                case CommonDataType.Phone:
                case CommonDataType.Email:
                case CommonDataType.Location:
                case CommonDataType.TimeSpan:
                case CommonDataType.JsonObject: return Data;
                case CommonDataType.DateTime: return Eval(() => DateTime.Parse(Data).ToString(), ex => "");

                case CommonDataType.Binary: return "Binary-Data";

                case CommonDataType.UnknownType:
                default: return "Unknown-Type";
            }
        }
    }
}
