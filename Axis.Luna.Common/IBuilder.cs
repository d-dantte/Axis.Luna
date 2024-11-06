namespace BioCommons.Utils.Builder
{
    /// <summary>
    /// A simple builder interface that builds an instance given another instance (typically options).
    /// </summary>
    /// <typeparam name="TOptions">The dependent (options) instance</typeparam>
    /// <typeparam name="TInstance">The instance to be built</typeparam>
    public interface IBuilder<TOptions, TInstance>
    {
        /// <summary>
        /// Build the instance
        /// </summary>
        /// <param name="options">The options to use</param>
        /// <returns>The built instance</returns>
        TInstance Build(TOptions options);
    }

    /// <summary>
    /// Targeted/Discriminated builder.
    /// </summary>
    public interface IBuilder<TOptions, TInstance, TTarget> : IBuilder<TOptions, TInstance>
    {
    }
}
