using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Axis.Luna.Factory.Contracts.Options
{
    public static class DiscriminatedOptionsExtensions
    {
        public static IServiceCollection AddDiscriminatedOptionSupport(
            this IServiceCollection services)
            => AddDiscriminatedOptionSupport(services, null);

        public static IServiceCollection AddDiscriminatedOptionSupport(
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

        #region Configure Options
        public static IServiceCollection ConfigureDiscriminatedOption<TSetting, TDiscriminant>(
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

            return services.AddDiscriminatedOption<TSetting, TDiscriminant>(discriminator);
        }

        public static IServiceCollection ConfigureDiscriminatedOption<TSetting, TDiscriminant>(
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

            return services.AddDiscriminatedOption<TSetting, TDiscriminant>(discriminator);
        }

        public static IServiceCollection ConfigureDiscriminatedOption<TSetting, TDiscriminant>(
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

            return services.AddDiscriminatedOption<TSetting, TDiscriminant>(discriminator);
        }


        public static IServiceCollection ConfigureDiscriminatedOption<TSetting, TDiscriminant>(
            this IServiceCollection services,
            string  sectionKey)
            where TSetting : class
            => ConfigureDiscriminatedOption<TSetting, TDiscriminant>(services, typeof(TDiscriminant).Name, sectionKey);

        public static IServiceCollection ConfigureDiscriminatedOption<TSetting, TDiscriminant>(
            this IServiceCollection services,
            IConfigurationSection section)
            where TSetting : class
            => ConfigureDiscriminatedOption<TSetting, TDiscriminant>(services, typeof(TDiscriminant).Name, section);

        public static IServiceCollection ConfigureDiscriminatedOption<TSetting, TDiscriminant>(
            this IServiceCollection services,
            Action<TSetting, IConfiguration, IServiceProvider> settingBuilder)
            where TSetting : class
            => ConfigureDiscriminatedOption<TSetting, TDiscriminant>(services, typeof(TDiscriminant).Name, settingBuilder);
        #endregion

        #region Add Options
        public static IServiceCollection AddOptions<TSetting>(
            this IServiceCollection services,
            Action<OptionsBuilder<TSetting>> builderAction)
            where TSetting: class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(builderAction);

            var builder = services.AddOptions<TSetting>();
            builderAction.Invoke(builder);
            return services;
        }
        #endregion

        public static IServiceCollection AddDiscriminatedOption<TSetting, TDiscriminant>(
            this IServiceCollection services,
            string discriminator)
            where TSetting : class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(discriminator);

            _ = services.AddSingleton<IDiscriminatedOptions<TSetting, TDiscriminant>>(
                serviceProvider => new DiscriminatedOption<TSetting, TDiscriminant>(
                    serviceProvider.GetService<IOptionsMonitor<TSetting>>()!,
                    discriminator));

            return services;
        }
    }
}
