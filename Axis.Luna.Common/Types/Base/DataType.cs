using System;
using System.Collections.Generic;

namespace Axis.Luna.Common.Types.Base
{
    public enum DataTypes
    {
        Struct,
        List,
        Int,
        Real,
        Decimal,
        Bool,
        String,
        Date,
        TimeSpan,
        Guid,
        Bytes
    }

    public abstract class DataType
    {
        public abstract DataTypes Type { get; }

        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();


        public static implicit operator DataType(bool boolean) => new BoolData { Value = boolean };

        public static implicit operator DataType(long value) => new IntData { Value = value };

        public static implicit operator DataType(double value) => new RealData { Value = value };

        public static implicit operator DataType(decimal value) => new DecimalData { Value = value };

        public static implicit operator DataType(byte[] value) => new ByteData { Value = value };

        public static implicit operator DataType(Guid value) => new GuidData { Value = value };

        public static implicit operator DataType(DateTimeOffset value) => new DateData { Value = value };

        public static implicit operator DataType(TimeSpan value) => new TimeSpanData { Value = value };

        public static implicit operator DataType(string value) => new StringData { Value = value };

        public static implicit operator DataType(DataType[] value) => new ListData { Value = value };

        public static implicit operator DataType(List<DataType> value) => new ListData { Value = value };
    }

    public abstract class IDataType<TValue>: DataType
    {
        public abstract TValue Value { get; set; }
    }
}
