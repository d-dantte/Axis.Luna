using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{

    /// <summary>
    /// This type wraps around <see cref="IBasicValue"/> instances as a convenient way for them to be implicitly converted from their native values,
    /// while assigning them via the indexer of a <see cref="BasicStruct"/> instance.
    /// </summary>
    public readonly struct BasicValueWrapper
    {
        public IBasicValue Value { get; }

        internal BasicValueWrapper(IBasicValue value)
        {
            Value = value;
        }

        #region implicits

        #region ints
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(sbyte? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(sbyte value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(short? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(short value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(int? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(int value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(long? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(long value) => new BasicValueWrapper(IBasicValue.Of(value));
        #endregion

        #region uints
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(byte? value) => new BasicValueWrapper(IBasicValue.Of((ulong?)value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(byte value) => new BasicValueWrapper(IBasicValue.Of((ulong?)value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(ushort? value) => new BasicValueWrapper(IBasicValue.Of((ulong?)value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(ushort value) => new BasicValueWrapper(IBasicValue.Of((ulong?)value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(uint? value) => new BasicValueWrapper(IBasicValue.Of((ulong?)value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(uint value) => new BasicValueWrapper(IBasicValue.Of((ulong?)value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(ulong? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(ulong value) => new BasicValueWrapper(IBasicValue.Of(value));
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(double? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(decimal? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(bool? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(string value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(DateTimeOffset? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(TimeSpan? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(Guid? value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(byte[] value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(IBasicValue[] value) => new BasicValueWrapper(IBasicValue.Of(value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(BasicValueWrapper[] value)
            => new BasicValueWrapper(IBasicValue.Of(value?.Select(v => v.Value)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(BasicStruct value) => new BasicValueWrapper(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BasicValueWrapper(BasicStruct.Initializer value)
            => new BasicValueWrapper(IBasicValue.Of(value?.Properties, value?.Metadata));

        #endregion
    }
}
