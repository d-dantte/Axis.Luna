namespace Axis.Luna.Common.Segments
{
    public interface IOffsetable
    {
        int Offset { get; }
    }

    public interface ILongOffsetable
    {
        long LongOffset { get; }
    }
}
