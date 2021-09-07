namespace Axis.Luna.Common.Types.Base
{
    public class DecimalData: IDataType<decimal?>
    {
        public override DataTypes Type => DataTypes.Decimal;

        public override decimal? Value { get; set; }

        public override bool Equals(object obj)
            => obj is DecimalData other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();


        public static bool operator ==(DecimalData first, DecimalData second) => first?.Value == second?.Value;

        public static bool operator !=(DecimalData first, DecimalData second) => !(first == second);
    }
}
