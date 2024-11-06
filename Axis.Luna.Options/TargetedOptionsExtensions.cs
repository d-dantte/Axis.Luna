using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Axis.Luna.Options
{
    public static class TargetedOptionsExtensions
    {
        public static IServiceCollection AddTargetedOptionSupport(
            this IServiceCollection services)
            => AddTargetedOptionSupport(services, null);

        public static IServiceCollection AddTargetedOptionSupport(
            this IServiceCollection services,
            IConfiguration? configRoot)
        {
            ArgumentNullException.ThrowIfNull(services);

            if (configRoot is not null)
                services.AddSingleton(configRoot);

            // Add support for options
            _ = services.AddOptions();

            return services;
        }

        #region Configure Optins
        public static IServiceCollection ConfigureTargetedOption<TSetting, TTarget>(
            this IServiceCollection services,
            string discriminator,
            string sectionKey,
            Action<BinderOptions>? bindingOptionsAction = null)
            where TSetting : class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentException.ThrowIfNullOrWhiteSpace(sectionKey);
            ArgumentNullException.ThrowIfNull(discriminator);

            _ = services
                .AddOptions<TSetting>(discriminator)
                .BindConfiguration(sectionKey, bindingOptionsAction)
                .ValidateDataAnnotations();

            return services.AddTargetedOption<TSetting, TTarget>(discriminator);
        }

        public static IServiceCollection ConfigureTargetedOption<TSetting, TTarget>(
            this IServiceCollection services,
            string discriminator,
            IConfigurationSection section)
            where TSetting : class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(section);
            ArgumentNullException.ThrowIfNull(discriminator);

            _ = services
                .AddOptions<TSetting>(discriminator)
                .Bind(section)
                .ValidateDataAnnotations();

            return services.AddTargetedOption<TSetting, TTarget>(discriminator);
        }

        public static IServiceCollection ConfigureTargetedOption<TSetting, TTarget>(
            this IServiceCollection services,
            string discriminator,
            Action<TSetting, IConfiguration, IServiceProvider> settingsBuilder)
            where TSetting : class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(settingsBuilder);
            ArgumentNullException.ThrowIfNull(discriminator);

            _ = services
                .AddOptions<TSetting>(discriminator)
                .Configure(settingsBuilder)
                .ValidateDataAnnotations();

            return services.AddTargetedOption<TSetting, TTarget>(discriminator);
        }


        public static IServiceCollection ConfigureTargetedOption<TSetting, TTarget>(
            this IServiceCollection services,
            string  sectionKey)
            where TSetting : class
            => ConfigureTargetedOption<TSetting, TTarget>(services, typeof(TTarget).Name, sectionKey);

        public static IServiceCollection ConfigureTargetedOption<TSetting, TTarget>(
            this IServiceCollection services,
            IConfigurationSection section)
            where TSetting : class
            => ConfigureTargetedOption<TSetting, TTarget>(services, typeof(TTarget).Name, section);

        public static IServiceCollection ConfigureTargetedOption<TSetting, TTarget>(
            this IServiceCollection services,
            Action<TSetting, IConfiguration, IServiceProvider> settingBuilder)
            where TSetting : class
            => ConfigureTargetedOption<TSetting, TTarget>(services, typeof(TTarget).Name, settingBuilder);
        #endregion

        public static IServiceCollection AddTargetedOption<TSetting, TTarget>(
            this IServiceCollection services,
            string discriminator)
            where TSetting : class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(discriminator);

            _ = services.AddSingleton<IOptions<TSetting, TTarget>>(
                serviceProvider => new TargetedOption<TSetting, TTarget>(
                    serviceProvider.GetService<IOptionsMonitor<TSetting>>()!,
                    discriminator));

            return services;
        }
    }
}
