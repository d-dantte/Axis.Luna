using System;

namespace Axis.Luna.Common.Types.Base
{
    public class StringData : IDataType<string>
    {
        public override DataTypes Type => DataTypes.String;

        public override string Value { get; set; }

        public override bool Equals(object obj)
            => obj is StringData other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;


        public static bool operator ==(StringData first, StringData second)
        {
            if (first == null && second == null)
                return false;

            else return first?.Value?.Equals(second?.Value, StringComparison.InvariantCulture) == true;
        }

        public static bool operator !=(StringData first, StringData second) => !(first == second);
    }
}
