namespace Axis.Luna.Options
{
    /// <summary>
    /// Targeted option for providing a setting instance based on the generic types given.
    /// </summary>
    /// <typeparam name="TSetting">The type of the setting instance</typeparam>
    /// <typeparam name="TTarget">
    ///     The type of the object that is requesting for the setting factory. This is usually the type into which
    ///     the <see cref="IOptions{TSetting, TTarget}"/> will be injected.
    /// </typeparam>
    public interface IOptions<TSetting, TTarget>
    {
        /// <summary>
        /// Gets the setting instance
        /// </summary>
        /// <returns></returns>
        TSetting Get();

        /// <summary>
        /// Gets the setting, or if not possible, returns the default value
        /// </summary>
        /// <param name="valueProvider">The default value provider, called each time this method is invoked, with the original value as an argument</param>
        /// <returns>The setting instance, or the default value</returns>
        TSetting Get(Func<TSetting, TSetting> valueProvider);
    }
}
