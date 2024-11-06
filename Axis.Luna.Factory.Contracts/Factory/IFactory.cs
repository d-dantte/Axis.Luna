namespace Axis.Luna.Factory.Contracts.Factory
{
    /// <summary>
    /// A factory contract.
    /// With respect to dependency injection, these factories are intended for instances where the resolved type needs special logic to be created.
    /// This logic is typically beyond the regular type construction offered by the DI container.
    /// <para/>
    /// DI containers usually support registering types against factory/resolution functions. Those serve identical purposes to the factories,
    /// except that the type itself gets injected at the dependency site, while a factory is inject when using factories.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    public interface IFactory<out TInstance>
    {
        /// <summary>
        /// Creates the instance, throwing an exception if it cannot be created
        /// </summary>
        /// <returns>The created instance</returns>
        /// <exception cref="InvalidOperationException">When the setting cannot be retrieved/provided</exception>
        TInstance Create();
    }

    /// <summary>
    /// Discriminated factory. Enables discrimination of factories that create the same <see cref="TInstance"/>,
    /// based on the <see cref="TDiscriminant"/> type.
    /// </summary>
    /// <typeparam name="TInstance">The type of the instance to be created</typeparam>
    /// <typeparam name="TDiscriminant">
    ///     Discriminator type that helps with creating an differentiating the factory, or the instance created.
    /// </typeparam>
    public interface IDiscriminatedFactory<out TInstance, TDiscriminant> : IFactory<TInstance>
    {
    }
}
