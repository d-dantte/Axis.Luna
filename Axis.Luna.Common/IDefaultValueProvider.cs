namespace Axis.Luna.Common
{
    /// <summary>
    /// struct contract for providing default values
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IDefaultValueProvider<TValue>
    where TValue : struct
    {
        /// <summary>
        /// Indicates if this instance is the default for the given type
        /// </summary>
        bool IsDefault { get; }

        /// <summary>
        /// The default value for the given type
        /// </summary>
        public TValue Default { get; }
    }
}
