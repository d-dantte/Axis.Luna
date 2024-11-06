namespace Axis.Luna.Factory.Contracts.Options
{
    /// <summary>
    /// Discriminated option for extracting a setting instance based on a discriminant type given as a generic param
    /// </summary>
    /// <typeparam name="TSetting">The type of the setting instance</typeparam>
    /// <typeparam name="TDiscriminant">
    ///     The type of the object that is requesting for the setting factory. This is usually the type into which
    ///     the <see cref="IDiscriminatedOptions{TSetting, TDiscriminant}"/> will be injected.
    /// </typeparam>
    public interface IDiscriminatedOptions<TSetting, TDiscriminant>
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
