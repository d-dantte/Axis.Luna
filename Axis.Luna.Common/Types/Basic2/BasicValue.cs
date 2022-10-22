using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    /// <summary>
    /// TODO: add support for Uri, GeoLocation
    /// </summary>
    public enum BasicTypes
    {
        /// <summary>
        /// Special null-value type
        /// </summary>
        NullValue,

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

        #region Union types

        /// <summary>
        /// 
        /// </summary>
        public partial struct BasicUInt : IBasicValue
        {
            private readonly Metadata[] _metadata;

            public BasicTypes Type => BasicTypes.UInt;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public ulong? Value { get; }

            internal BasicUInt(ulong? value, params Metadata[] metadata)
            {
                Value = value;
                _metadata = metadata?.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public partial struct BasicTimeSpan : IBasicValue
        {
            private readonly Metadata[] _metadata;

            public BasicTypes Type => BasicTypes.TimeSpan;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public TimeSpan? Value { get; }

            internal BasicTimeSpan(TimeSpan? value, params Metadata[] metadata)
            {
                Value = value;
                _metadata = metadata?.ToArray();
            }
        }
        #endregion
    }
}
