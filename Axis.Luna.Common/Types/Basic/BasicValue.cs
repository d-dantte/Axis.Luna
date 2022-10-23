using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    /// <summary>
    /// TODO: add support for Uri, GeoLocation
    /// </summary>
    public enum BasicTypes
    {
        Struct,
        List,
        Int,
        UInt,
        Real,
        Decimal,
        Bool,
        String,
        Date,
        TimeSpan,
        Guid,
        Bytes
    }

    /// <summary>
    /// A value container. The contained value may or may not be absent. Each implementation of this interface is a <c>struct</c> whose
    /// default state represents the state where the contained value is missing.
    /// </summary>
    public partial interface IBasicValue
    {
        #region Of
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(bool? value, params Metadata[] metadata) => new BasicBool(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(long? value, params Metadata[] metadata) => new BasicInt(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(ulong? value, params Metadata[] metadata) => new BasicUInt(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(double? value, params Metadata[] metadata) => new BasicReal(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(decimal? value, params Metadata[] metadata) => new BasicDecimal(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(string value, params Metadata[] metadata) => new BasicString(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(DateTimeOffset? value, params Metadata[] metadata) => new BasicDate(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(TimeSpan? value, params Metadata[] metadata) => new BasicTimeSpan(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(Guid? value, params Metadata[] metadata) => new BasicGuid(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(byte[] value, params Metadata[] metadata) => new BasicBytes(value, metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(IEnumerable<IBasicValue> value, params Metadata[] metadata) => new BasicList(value?.ToArray(), metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IBasicValue Of(IEnumerable<BasicStruct.Property> value, params Metadata[] metadata) => new BasicStruct(value?.ToArray(), metadata);

        #endregion

        #region Members
        /// <summary>
        /// The underlying type of this value
        /// </summary>
        BasicTypes Type { get; }

        /// <summary>
        /// Enables the ability to add extra information about this value. Information here is subject to interpretation of the consumer of the data
        /// </summary>
        Metadata[] Metadata { get; }
        #endregion
    }
}
