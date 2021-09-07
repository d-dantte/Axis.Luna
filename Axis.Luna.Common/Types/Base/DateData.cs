using System;

namespace Axis.Luna.Common.Types.Base
{
    public class DateData : IDataType<DateTimeOffset?>
    {
        public override DataTypes Type => DataTypes.Date;

        public override DateTimeOffset? Value { get; set; }

        public override bool Equals(object obj)
            => obj is DateData other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();


        public static bool operator ==(DateData first, DateData second) => first?.Value == second?.Value;

        public static bool operator !=(DateData first, DateData second) => !(first == second);
    }
}
