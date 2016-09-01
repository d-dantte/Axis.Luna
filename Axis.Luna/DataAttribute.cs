using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axis.Luna.Extensions.ObjectExtensions;
using static Axis.Luna.Extensions.EnumerableExtensions;

namespace Axis.Luna
{
    public class DataAttribute
    {
        public string Name { get; set; }

        public string Data { get; set; }

        public CommonDataType Type { get; set; }

        public override string ToString() => $"[{Name}: {DisplayData()}]";

        public override int GetHashCode() => ValueHash(Name.Enumerate());
        public override bool Equals(object obj)
            => obj.As<DataAttribute>()
                  .Pipe(_da => _da.Name == Name && _da.Data == Data);


        private string DisplayData()
        {
            switch(Type)
            {
                case CommonDataType.Boolean:
                case CommonDataType.Real:
                case CommonDataType.Integer:
                case CommonDataType.String:
                case CommonDataType.Url:
                case CommonDataType.TimeSpan:
                case CommonDataType.JsonObject: return Data;
                case CommonDataType.DateTime: return Eval(() => DateTime.Parse(Data).ToString(), ex => "");

                case CommonDataType.Binary: return "Binary-Data";

                case CommonDataType.UnknownType:
                default: return "Unknown-Type";
            }
        }
    }


    public enum CommonDataType
    {
        String,
        Integer,
        Real,
        Boolean,
        Binary,

        JsonObject,

        DateTime,
        TimeSpan,

        Url,

        UnknownType
    }
}
