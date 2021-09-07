namespace Axis.Luna.Common.Types.Base
{
    public class BoolData : IDataType<bool?>
    {
        public override DataTypes Type => DataTypes.Bool;

        public override bool? Value { get; set; }

        public override bool Equals(object obj)
            => obj is BoolData other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();


        public static bool operator ==(BoolData first, BoolData second) => first?.Value == second?.Value;

        public static bool operator !=(BoolData first, BoolData second) => !(first == second);
    }
}
