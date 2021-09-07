using System;

namespace Axis.Luna.Common.Types.Base
{
    public class GuidData : IDataType<Guid?>
    {
        public override DataTypes Type => DataTypes.Guid;

        public override Guid? Value { get; set; }

        public override bool Equals(object obj)
            => obj is GuidData other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();


        public static bool operator ==(GuidData first, GuidData second) => first?.Value == second?.Value;

        public static bool operator !=(GuidData first, GuidData second) => !(first == second);
    }
}
