using System;

namespace Axis.Luna.Common.Types.Base
{
    public class TimeSpanData : IDataType<TimeSpan?>
    {

        public override DataTypes Type => DataTypes.TimeSpan;

        public override TimeSpan? Value { get; set; }

        public override bool Equals(object obj)
            => obj is TimeSpanData other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();


        public static bool operator ==(TimeSpanData first, TimeSpanData second) => first?.Value == second?.Value;

        public static bool operator !=(TimeSpanData first, TimeSpanData second) => !(first == second);
    }
}
