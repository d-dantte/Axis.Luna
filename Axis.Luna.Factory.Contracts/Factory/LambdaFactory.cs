
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Luna.Factory.Contracts.Factory
{
    public class LambdaFactory<TInstance> : IFactory<TInstance>
    {
        private readonly Func<TInstance> instanceFactory;

        public LambdaFactory(Func<TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(instanceFactory);
            this.instanceFactory = instanceFactory;
        }

        public TInstance Create() => instanceFactory.Invoke();
    }

    public class LambdaFactory<TInstance, TTarget> :
        IDiscriminatedFactory<TInstance, TTarget>
    {
        private readonly Func<TInstance> instanceFactory;

        public LambdaFactory(Func<TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(instanceFactory);
            this.instanceFactory = instanceFactory;
        }

        public TInstance Create() => instanceFactory.Invoke();
    }

    public static class LambdaFactory
    {
        public static Func<IServiceProvider, LambdaFactory<TInstance, TTarget>> FactoryProviderFor<TInstance, TTarget>(
            Func<IServiceProvider, TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(instanceFactory);

            return serviceProvider => new LambdaFactory<TInstance, TTarget>(
                () => instanceFactory.Invoke(serviceProvider));
        }

        public static Func<IServiceProvider, LambdaFactory<TInstance>> FactoryProviderFor<TInstance>(
            Func<IServiceProvider, TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(instanceFactory);

            return serviceProvider => new LambdaFactory<TInstance>(
                () => instanceFactory.Invoke(serviceProvider));
        }

        #region Transient
        public static IServiceCollection AddLambdaFactory<TInstance, TTarget>(
            this IServiceCollection services,
            Func<IServiceProvider, TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(instanceFactory);

            return services.AddTransient<IDiscriminatedFactory<TInstance, TTarget>>(FactoryProviderFor<TInstance, TTarget>(instanceFactory));
        }

        public static IServiceCollection AddLambdaFactory<TInstance>(
            this IServiceCollection services,
            Func<IServiceProvider, TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(instanceFactory);

            return services.AddTransient<IFactory<TInstance>>(FactoryProviderFor<TInstance>(instanceFactory));
        }
        #endregion

        #region Scoped
        public static IServiceCollection AddScopedLambdaFactory<TInstance, TTarget>(
            this IServiceCollection services,
            Func<IServiceProvider, TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(instanceFactory);

            return services.AddScoped<IDiscriminatedFactory<TInstance, TTarget>>(FactoryProviderFor<TInstance, TTarget>(instanceFactory));
        }

        public static IServiceCollection AddScopedLambdaFactory<TInstance>(
            this IServiceCollection services,
            Func<IServiceProvider, TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(instanceFactory);

            return services.AddScoped<IFactory<TInstance>>(FactoryProviderFor<TInstance>(instanceFactory));
        }
        #endregion

        #region Singleton
        public static IServiceCollection AddSingletonLambdaFactory<TInstance, TTarget>(
            this IServiceCollection services,
            Func<IServiceProvider, TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(instanceFactory);

            return services.AddSingleton<IDiscriminatedFactory<TInstance, TTarget>>(FactoryProviderFor<TInstance, TTarget>(instanceFactory));
        }

        public static IServiceCollection AddSingletonLambdaFactory<TInstance>(
            this IServiceCollection services,
            Func<IServiceProvider, TInstance> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(instanceFactory);

            return services.AddSingleton<IFactory<TInstance>>(FactoryProviderFor<TInstance>(instanceFactory));
        }
        #endregion
    }
}
