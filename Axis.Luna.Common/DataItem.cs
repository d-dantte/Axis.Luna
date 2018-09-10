using Axis.Luna.Common.Contracts;
using Axis.Luna.Extensions;
using static Axis.Luna.Extensions.Common;

namespace Axis.Luna.Common
{
    public class DataItem : IDataItem
    {
        public string Name { get; set; }

        public string Data { get; set; }

        public CommonDataType Type { get; set; }

        public override int GetHashCode() => ValueHash(Name.Enumerate());

        public override bool Equals(object obj)
        => obj.As<DataItem>()
              .Pipe(_da => _da.Name == Name && _da.Data == Data);

        public override string ToString() => $"[{Name}: {this.DisplayData()}]";

        public virtual string DisplayData() => Data;
    }
}
