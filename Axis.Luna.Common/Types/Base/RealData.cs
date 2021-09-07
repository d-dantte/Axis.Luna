namespace Axis.Luna.Common.Types.Base
{
    public class RealData : IDataType<double?>
    {
        public override DataTypes Type => DataTypes.Real;

        public override double? Value { get; set; }

        public override bool Equals(object obj)
            => obj is RealData other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();


        public static bool operator ==(RealData first, RealData second) => first?.Value == second?.Value;

        public static bool operator !=(RealData first, RealData second) => !(first == second);
    }
}
