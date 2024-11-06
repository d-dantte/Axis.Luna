namespace Axis.Luna.Factory.Contracts.Builder
{
    /// <summary>
    /// A simple builder interface that builds an instance
    /// </summary>
    /// <typeparam name="TInstance">The instance to be built</typeparam>
    public interface IBuilder<out TInstance>
    {
        /// <summary>
        /// Build the instance from internally available data/information
        /// </summary>
        /// <returns>The built instance</returns>
        TInstance Build();
    }

    /// <summary>
    /// A simple builder interface that builds an instance given another instance (typically options).
    /// </summary>
    /// <typeparam name="TOptions">The dependent (options) instance</typeparam>
    /// <typeparam name="TInstance">The instance to be built</typeparam>
    public interface IBuilder<in TOptions, out TInstance>
    {
        /// <summary>
        /// Build the instance from externally provided (and internally available) data/information
        /// </summary>
        /// <param name="options">The options to use</param>
        /// <returns>The built instance</returns>
        TInstance Build(TOptions options);
    }

    /// <summary>
    /// Discriminated builder.
    /// </summary>
    public interface IDiscriminatedIBuilder<TOptions, TInstance, TDiscriminant> : IBuilder<TOptions, TInstance>
    {
    }
}
