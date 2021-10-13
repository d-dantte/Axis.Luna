using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Luna.Common.Types.Basic
{

    public static class BaseExtensions
    {
        public static BasicBool AsBasicType(this bool value) => new BasicBool(value);
        public static BasicBool AsBasicType(this bool? value) => new BasicBool(value);

        #region ints
        public static BasicInt AsBasicType(this long value) => new BasicInt(value);
        public static BasicInt AsBasicType(this long? value) => new BasicInt(value);

        public static BasicInt AsBasicType(this int value) => new BasicInt(value);
        public static BasicInt AsBasicType(this int? value) => new BasicInt(value);

        public static BasicInt AsBasicType(this short value) => new BasicInt(value);
        public static BasicInt AsBasicType(this short? value) => new BasicInt(value);

        public static BasicInt AsBasicType(this uint value) => new BasicInt(value);
        public static BasicInt AsBasicType(this uint? value) => new BasicInt(value);

        public static BasicInt AsBasicType(this ushort value) => new BasicInt(value);
        public static BasicInt AsBasicType(this ushort? value) => new BasicInt(value);
        #endregion

        public static BasicReal AsBasicType(this double value) => new BasicReal(value);
        public static BasicReal AsBasicType(this double? value) => new BasicReal(value);

        public static BasicDecimal AsBasicType(this decimal value) => new BasicDecimal(value);
        public static BasicDecimal AsBasicType(this decimal? value) => new BasicDecimal(value);

        public static BasicGuid AsBasicType(this Guid value) => new BasicGuid(value);
        public static BasicGuid AsBasicType(this Guid? value) => new BasicGuid(value);

        public static BasicDateTime AsBasicType(this DateTimeOffset value) => new BasicDateTime(value);
        public static BasicDateTime AsBasicType(this DateTimeOffset? value) => new BasicDateTime(value);

        public static BasicTimeSpan AsBasicType(this TimeSpan value) => new BasicTimeSpan(value);
        public static BasicTimeSpan AsBasicType(this TimeSpan? value) => new BasicTimeSpan(value);

        public static BasicBytes AsBasicType(this byte[] value) => new BasicBytes(value);
        public static BasicString AsBasicType(this string value) => new BasicString(value);
        public static BasicList AsBasicType(this IEnumerable<BasicValue> value) => new BasicList(value);



        /// <summary>
        /// Extracts the inner boolean value if this BasicValue represented a bool value. Null other wise
        /// </summary>
        /// <param name="nullMapper">If this <see cref="BasicValue"/> was not a <see cref="BasicBool"/>, this delegate is invoked.</param>
        /// <returns>The extracted value</returns>
        public static bool? AsBoolValue(this BasicValue? @this, Func<bool> nullMapper = null)
            => @this.Bind(basicValue => basicValue.AsBool().Bind(basicBool => basicBool.Value, nullMapper));

        public static bool? AsBoolValue(this BasicValue @this, Func<bool> nullMapper = null)
            => @this.AsBool().Bind(basicBool => basicBool.Value, nullMapper);

        /// <summary>
        /// Extracts the inner long value if this BasicValue represented a long value. Null other wise
        /// </summary>
        /// <param name="nullMapper">If this <see cref="BasicValue"/> was not a <see cref="BasicInt"/>, this delegate is invoked.</param>
        /// <returns>The extracted value</returns>
        public static long? AsIntValue(this BasicValue? @this, Func<long> nullMapper = null)
            => @this.Bind(basicValue => basicValue.AsInt().Bind(basicInt => basicInt.Value, nullMapper));

        public static long? AsIntValue(this BasicValue @this, Func<long> nullMapper = null)
            => @this.AsInt().Bind(basicInt => basicInt.Value, nullMapper);

        /// <summary>
        /// Extracts the inner double value if this BasicValue represented a double value. Null other wise
        /// </summary>
        /// <param name="nullMapper">If this <see cref="BasicValue"/> was not a <see cref="BasicReal"/>, this delegate is invoked.</param>
        /// <returns>The extracted value</returns>
        public static double? AsRealValue(this BasicValue? @this, Func<double> nullMapper = null)
            => @this.Bind(basicValue => basicValue.AsReal().Bind(basicReal => basicReal.Value, nullMapper));

        public static double? AsRealValue(this BasicValue @this, Func<double> nullMapper = null)
            => @this.AsReal().Bind(basicReal => basicReal.Value, nullMapper);

        /// <summary>
        /// Extracts the inner decimal value if this BasicValue represented a decimal value. Null other wise
        /// </summary>
        /// <param name="nullMapper">If this <see cref="BasicValue"/> was not a <see cref="BasicDecimal"/>, this delegate is invoked.</param>
        /// <returns>The extracted value</returns>
        public static decimal? AsDecimalValue(this BasicValue? @this, Func<decimal> nullMapper = null)
            => @this.Bind(basicValue => basicValue.AsDecimal().Bind(basicDecimal => basicDecimal.Value, nullMapper));

        public static decimal? AsDecimalValue(this BasicValue @this, Func<decimal> nullMapper = null)
            => @this.AsDecimal().Bind(basicDecimal => basicDecimal.Value, nullMapper);

        /// <summary>
        /// Extracts the inner byte[] value if this BasicValue represented a byte[] value. Null other wise
        /// </summary>
        /// <param name="nullMapper">If this <see cref="BasicValue"/> was not a <see cref="BasicBytes"/>, this delegate is invoked.</param>
        /// <returns>The extracted value</returns>
        public static Optional<byte[]> AsBytesValue(this BasicValue? @this, Func<byte[]> nullMapper = null)
            => @this.Map(basicValue => basicValue.AsBytes().Map(basicBytes => basicBytes.Value, nullMapper));

        public static Optional<byte[]> AsBytesValue(this BasicValue @this, Func<byte[]> nullMapper = null)
            => @this.AsBytes().Map(basicBytes => basicBytes.Value, nullMapper);

        /// <summary>
        /// Extracts the inner DateTimeOffset value if this BasicValue represented a Date value. Null other wise
        /// </summary>
        /// <param name="nullMapper">If this <see cref="BasicValue"/> was not a <see cref="BasicDateTime"/>, this delegate is invoked.</param>
        /// <returns>The extracted value</returns>
        public static DateTimeOffset? AsDateTimeValue(this BasicValue? @this, Func<DateTimeOffset> nullMapper = null)
            => @this.Bind(basicValue => basicValue.AsDateTime().Bind(basicDateTime => basicDateTime.Value, nullMapper));

        public static DateTimeOffset? AsDateTimeValue(this BasicValue @this, Func<DateTimeOffset> nullMapper = null)
            => @this.AsDateTime().Bind(basicDateTime => basicDateTime.Value, nullMapper);

        /// <summary>
        /// Extracts the inner TimeSpan value if this BasicValue represented a TimeSpan value. Null other wise
        /// </summary>
        /// <param name="nullMapper">If this <see cref="BasicValue"/> was not a <see cref="BasicTimeSpan"/>, this delegate is invoked.</param>
        /// <returns>The extracted value</returns>
        public static TimeSpan? AsTimeSpanValue(this BasicValue? @this, Func<TimeSpan> nullMapper = null)
            => @this.Bind(basicValue => basicValue.AsTimeSpan().Bind(basicTimeSpan => basicTimeSpan.Value, nullMapper));

        public static TimeSpan? AsTimeSpanValue(this BasicValue @this, Func<TimeSpan> nullMapper = null)
            => @this.AsTimeSpan().Bind(basicTimeSpan => basicTimeSpan.Value, nullMapper);

        /// <summary>
        /// Extracts the inner string value if this BasicValue represented a string value. Null other wise
        /// </summary>
        /// <param name="nullMapper">If this <see cref="BasicValue"/> was not a <see cref="BasicString"/>, this delegate is invoked.</param>
        /// <returns>The extracted value</returns>
        public static Optional<string> AsStringValue(this BasicValue? @this, Func<string> nullMapper = null)
            => @this.Map(basicValue => basicValue.AsString().Map(bsicString => bsicString.Value, nullMapper));

        public static Optional<string> AsStringValue(this BasicValue @this, Func<string> nullMapper = null)
            => @this.AsString().Map(bsicString => bsicString.Value, nullMapper);
    }
}
