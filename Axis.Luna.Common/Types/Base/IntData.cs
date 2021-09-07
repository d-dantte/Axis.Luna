namespace Axis.Luna.Common.Types.Base
{
    public class IntData : IDataType<long?>
    {
        public override DataTypes Type => DataTypes.Int;

        public override long? Value { get; set; }

        public override bool Equals(object obj)
            => obj is IntData other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();


        public static bool operator ==(IntData first, IntData second) => first?.Value == second?.Value;

        public static bool operator !=(IntData first, IntData second) => !(first == second);
    }
}
