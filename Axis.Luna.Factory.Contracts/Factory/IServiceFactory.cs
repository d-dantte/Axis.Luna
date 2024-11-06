namespace Axis.Luna.Factory.Contracts.Factory
{
    public interface IServiceFactory<out TService>
    {
        /// <summary>
        /// Creates an instance of the service, given a service provider for dependency resolution operations.
        /// </summary>
        /// <param name="serviceProvider">The supplied service provider</param>
        /// <returns>The service if successfully created, null if creation was not possible</returns>
        TService Create(IServiceProvider serviceProvider);
    }
}
