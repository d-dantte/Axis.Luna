namespace Axis.Luna.Common.Contracts
{
    public interface IDataItem
    {
        string Name { get; set; }

        string Data { get; set; }

        CommonDataType Type { get; set; }

        string DisplayData();
    }
}
