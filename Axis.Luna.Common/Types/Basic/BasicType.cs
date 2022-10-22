using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;

namespace Axis.Luna.Common.Types.Basic
{
    /// <summary>
    /// TODO: add support for UnsignedInt, Uri, GeoLocation
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

    public interface IBasicValue
    {
        /// <summary>
        /// The underlying type of this value
        /// </summary>
        BasicTypes Type { get; }

        /// <summary>
        /// Enables the ability to add extra information about this value. Information here is subject to interpretation of the consumer of the data
        /// </summary>
        BasicMetadata[] Metadata { get; }

        bool Equals(object obj);

        int GetHashCode();
    }

    public interface IBasicValue<TValue>: IBasicValue
    {
        TValue Value { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct BasicValue : IBasicValue
    {
        private readonly IBasicValue _innerValue;

        public BasicTypes Type => _innerValue?.Type ?? BasicTypes.NullValue;

        public BasicMetadata[] Metadata => _innerValue?.Metadata ?? Array.Empty<BasicMetadata>();


        public BasicValue(IBasicValue innerType)
        {
            if (innerType is BasicValue)
                throw new ArgumentException($"Cannot have innerType of type: {nameof(BasicValue)}");

            _innerValue = innerType ?? throw new ArgumentNullException(nameof(innerType));
        }


        public override bool Equals(object obj)
        {
            return obj is BasicValue other
                && other._innerValue.NullOrEquals(_innerValue);
        }

        public override int GetHashCode() => _innerValue?.GetHashCode() ?? 0;

        public override string ToString() => _innerValue?.ToString() ?? "";

        public static bool operator ==(BasicValue first, BasicValue second) => first.Equals(second);

        public static bool operator !=(BasicValue first, BasicValue second) => !first.Equals(second);

        #region Type checks
        public bool TryGetBool(out BasicBool? value)
        {
            if (_innerValue is BasicBool)
            {
                value = (BasicBool)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetInt(out BasicInt? value)
        {
            if (_innerValue is BasicInt)
            {
                value = (BasicInt)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetUInt(out BasicUInt? value)
        {
            if (_innerValue is BasicUInt)
            {
                value = (BasicUInt)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetReal(out BasicReal? value)
        {
            if (_innerValue is BasicReal)
            {
                value = (BasicReal)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetDecimal(out BasicDecimal? value)
        {
            if (_innerValue is BasicDecimal)
            {
                value = (BasicDecimal)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetBytes(out BasicBytes? value)
        {
            if (_innerValue is BasicBytes)
            {
                value = (BasicBytes)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetGuid(out BasicGuid? value)
        {
            if (_innerValue is BasicGuid)
            {
                value = (BasicGuid)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetDateTime(out BasicDateTime? value)
        {
            if (_innerValue is BasicDateTime)
            {
                value = (BasicDateTime)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetTimeSpan(out BasicTimeSpan? value)
        {
            if (_innerValue is BasicTimeSpan)
            {
                value = (BasicTimeSpan)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetString(out BasicString? value)
        {
            if (_innerValue is BasicString)
            {
                value = (BasicString)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetList(out BasicList? value)
        {
            if (_innerValue is BasicList)
            {
                value = (BasicList)this;
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetStruct(out BasicStruct? value)
        {
            if (_innerValue is BasicStruct)
            {
                value = (BasicStruct)this;
                return true;
            }

            value = null;
            return false;
        }
        #endregion

        #region Direct conversions
        public BasicBool? AsBool() => _innerValue is BasicBool b ? b : (BasicBool?)null;
        public BasicInt? AsInt() => _innerValue is BasicInt b ? b : (BasicInt?)null;
        public BasicUInt? AsUInt() => _innerValue is BasicUInt b ? b : (BasicUInt?)null;
        public BasicReal? AsReal() => _innerValue is BasicReal b ? b : (BasicReal?)null;
        public BasicDecimal? AsDecimal() => _innerValue is BasicDecimal b ? b : (BasicDecimal?)null;
        public BasicBytes? AsBytes() => _innerValue is BasicBytes b ? b : (BasicBytes?)null;
        public BasicDateTime? AsDateTime() => _innerValue is BasicDateTime b ? b : (BasicDateTime?)null;
        public BasicTimeSpan? AsTimeSpan() => _innerValue is BasicTimeSpan b ? b : (BasicTimeSpan?)null;
        public BasicString? AsString() => _innerValue is BasicString b ? b : (BasicString?)null;
        public BasicList? AsList() => _innerValue is BasicList b ? b : (BasicList?)null;
        public BasicStruct? AsStruct() => _innerValue is BasicStruct b ? b : (BasicStruct?)null;
        #endregion

        #region Implicits
        public static implicit operator BasicValue(bool value) => new BasicValue(new BasicBool(value));

        public static implicit operator BasicValue(long value) => new BasicValue(new BasicInt(value));

        public static implicit operator BasicValue(ulong value) => new BasicValue(new BasicUInt(value));

        public static implicit operator BasicValue(double value) => new BasicValue(new BasicReal(value));

        public static implicit operator BasicValue(decimal value) => new BasicValue(new BasicDecimal(value));

        public static implicit operator BasicValue(byte[] value) => new BasicValue(new BasicBytes(value));

        public static implicit operator BasicValue(Guid value) => new BasicValue(new BasicGuid(value));

        public static implicit operator BasicValue(DateTimeOffset value) => new BasicValue(new BasicDateTime(value));

        public static implicit operator BasicValue(TimeSpan value) => new BasicValue(new BasicTimeSpan(value));

        public static implicit operator BasicValue(string value) => new BasicValue(new BasicString(value));

        public static implicit operator BasicValue(BasicValue[] value) => new BasicValue(new BasicList(value));

        public static implicit operator BasicValue(HashSet<BasicValue> value) => new BasicValue(new BasicList((IEnumerable<BasicValue>)value));

        public static implicit operator BasicValue(List<BasicValue> value) => new BasicValue(new BasicList((IEnumerable<BasicValue>)value));


        public static implicit operator BasicValue(BasicBool value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicInt value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicUInt value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicReal value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicDecimal value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicBytes value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicGuid value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicDateTime value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicTimeSpan value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicString value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicList value) => new BasicValue(value);

        public static implicit operator BasicValue(BasicStruct value) => new BasicValue(value);
        #endregion

        #region Explicits
        public static explicit operator BasicBool(BasicValue value) => (BasicBool)value._innerValue;

        public static explicit operator BasicInt(BasicValue value) => (BasicInt)value._innerValue;

        public static explicit operator BasicUInt(BasicValue value) => (BasicUInt)value._innerValue;

        public static explicit operator BasicReal(BasicValue value) => (BasicReal)value._innerValue;

        public static explicit operator BasicDecimal(BasicValue value) => (BasicDecimal)value._innerValue;

        public static explicit operator BasicBytes(BasicValue value) => (BasicBytes)value._innerValue;

        public static explicit operator BasicGuid(BasicValue value) => (BasicGuid)value._innerValue;

        public static explicit operator BasicDateTime(BasicValue value) => (BasicDateTime)value._innerValue;

        public static explicit operator BasicTimeSpan(BasicValue value) => (BasicTimeSpan)value._innerValue;

        public static explicit operator BasicString(BasicValue value) => (BasicString)value._innerValue;

        public static explicit operator BasicList(BasicValue value) => (BasicList)value._innerValue;

        public static explicit operator BasicStruct(BasicValue value) => (BasicStruct)value._innerValue;
        #endregion

    }

    /// <summary>
    /// Key value pair formated like css properties: <c>key-1:value1;</c>
    /// </summary>
    public struct BasicMetadata
    {
        public string Key { get; }
        public string Value { get; }

        public BasicMetadata(string key, string value = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value;
        }

        public BasicMetadata(KeyValuePair<string, string> metadata)
            : this(metadata.Key, metadata.Value)
        { }

        public BasicMetadata((string key, string value) metadata)
            : this(metadata.key, metadata.value)
        { }

        public override string ToString()
        {
            if (this == default)
                return "";

            if (string.IsNullOrEmpty(Value))
                return $"{Key};";

            return $"{Key}:{Value};";
        }

        public override bool Equals(object obj)
        {
            return obj is BasicMetadata other
                && other.Key.NullOrEquals(Key)
                && other.Value.NullOrEquals(Value);
        }

        public override int GetHashCode() => HashCode.Combine(Key, Value);

        public static bool TryParse(string value, out BasicMetadata metadata)
        {
            if (TryParse(value, out IResult<BasicMetadata> result))
            {
                metadata = result
                    .As<IResult<BasicMetadata>.DataResult>()
                    .Data;
                return true;
            }

            metadata = default;
            return false;
        }

        private static bool TryParse(string value, out IResult<BasicMetadata> result)
        {
            if (value == null)
            {
                result = IResult<BasicMetadata>.Of(new ArgumentNullException(nameof(value)));
                return false;
            }

            var parts = value
                .Trim()
                .TrimEnd(';')
                .Split(':');

            if (parts.Length < 1 || parts.Length > 2)
            {
                result = IResult<BasicMetadata>.Of(new FormatException($"Invalid metadata format: {value}"));
                return false;
            }

            result = IResult<BasicMetadata>.Of(new BasicMetadata(parts[0], parts.Length > 1 ? parts[1] : null));
            return true;
        }

        public static bool operator ==(BasicMetadata first, BasicMetadata second) => first.Equals(second);

        public static bool operator !=(BasicMetadata first, BasicMetadata second) => !first.Equals(second);

        public static implicit operator BasicMetadata(string value)
        {
            if (!TryParse(value, out IResult<BasicMetadata> result))
                throw result
                    .As<IResult<BasicMetadata>.ErrorResult>()
                    .Cause();

            else return result
                    .As<IResult<BasicMetadata>.DataResult>()
                    .Data;
        }
    }
}
