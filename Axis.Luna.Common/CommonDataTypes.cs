using System;

namespace Axis.Luna.Common
{
    [Obsolete]
    public enum CommonDataType
    {
        /// <summary>
        /// Conventional string
        /// </summary>
        String,

        /// <summary>
        /// Conventional signed integer - defaults to Int64
        /// </summary>
        Integer,

        /// <summary>
        /// Conventional unsigned integer - defaults to UInt64
        /// </summary>
        UnsignedInteger,

        /// <summary>
        /// Conventional real number - defaults to Double
        /// </summary>
        Real,

        /// <summary>
        /// Conventional decimal/currency/etc.
        /// </summary>
        Decimal,

        /// <summary>
        /// Boolean value
        /// </summary>
        Boolean,

        /// <summary>
        /// Represents a string of bytes
        /// </summary>
        Binary, 

        /// <summary>
        /// Json object/value-map
        /// </summary>
        StructMap,

        /// <summary>
        /// Date-time value
        /// </summary>
        DateTime ,

        /// <summary>
        /// A time-stamp
        /// </summary>
        TimeSpan,

        /// <summary>
        /// Url
        /// </summary>
        Url,

        /// <summary>
        /// IP Address
        /// </summary>
        IPV4,

        /// <summary>
        /// IP Address
        /// </summary>
        IPV6,

        /// <summary>
        /// Phone number
        /// </summary>
        Phone,

        /// <summary>
        /// Email address
        /// </summary>
        Email,

        /// <summary>
        /// Location information (longitude,latitude,altitude)
        /// </summary>
        Location,

        /// <summary>
        /// Global unique identifier
        /// </summary>
        Guid,

        UnknownType,

        /// <summary>
        /// Name Value Pairs in the format: name:value; name:value; name:value;
        /// </summary>
        NVP,

        /// <summary>
        /// Comma Separated Values
        /// </summary>
        CSV
    }
}
