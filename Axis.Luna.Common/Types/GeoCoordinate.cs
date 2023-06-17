using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Luna.Common.Types
{
    /// <summary>
    /// Geographic coordinate representing Longitude, Latitude, and optionally Altitude.
    /// The string representation of this is <c>longitude, latitude[, altitude]</c>
    /// </summary>
    public struct GeoCoordinate: 
        IEquatable<GeoCoordinate>,
        IDefaultValueProvider<GeoCoordinate>
    {
        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Altitude
        /// </summary>
        public double? Altitude { get; }

        
        public GeoCoordinate(double longitude, double latitude, double? altitude = null)
        {
            Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
        }

        public bool IsDefault => Longitude == 0 && Latitude == 0 && Altitude is null;

        public GeoCoordinate Default => default;

        public bool Equals(GeoCoordinate coordinate)
        {
            return Longitude == coordinate.Longitude
                && Latitude == coordinate.Latitude
                && Altitude == coordinate.Altitude;
        }

        public static GeoCoordinate Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException("Value cannot be null");

            var values = value
                .Split(',')
                .Select(v => v.Trim())
                .Select(double.Parse)
                .ToArray();

            if (values.Length < 2 || values.Length > 3)
                throw new FormatException("Value is not in the correct format");

            return new GeoCoordinate(
                values[0],
                values[1],
                values.Length == 3 ? values[2] : (double?) null);
        }

        public static bool TryParse(string value, out GeoCoordinate coord)
        {
            try
            {
                coord = GeoCoordinate.Parse(value);
                return true;
            }
            catch
            {
                coord = default;
                return false;
            }
        }

        #region Overrides
        public override bool Equals(object obj)
        {
            return obj is GeoCoordinate other
                && Equals(other);
        }

        public override int GetHashCode() => ValueHash(Longitude, Latitude, Altitude);

        public override string ToString()
        {
            var stringValue = $"{Longitude}, {Latitude}";

            if (Altitude != null)
                stringValue += $", {Altitude}";

            return stringValue;
        }

        public static bool operator ==(GeoCoordinate first, GeoCoordinate second) => first.Equals(second);

        public static bool operator !=(GeoCoordinate first, GeoCoordinate second) => !first.Equals(second);
        #endregion
    }
}
