using Microsoft.Extensions.Options;

namespace Axis.Luna.Factory.Contracts.Options
{
    public class DiscriminatedOption<TSetting, TDiscriminant> :
        IDiscriminatedOptions<TSetting, TDiscriminant>
        where TSetting : class
    {
        private readonly string discriminator;
        private readonly IOptionsMonitor<TSetting> options;

        public DiscriminatedOption(
            IOptionsMonitor<TSetting> optionsMonitor,
            string discriminator)
        {
            ArgumentNullException.ThrowIfNull(optionsMonitor);
            ArgumentNullException.ThrowIfNull(discriminator);

            options = optionsMonitor;
            this.discriminator = discriminator;
        }

        public TSetting Get() => options.Get(discriminator);

        public TSetting Get(
            Func<TSetting, TSetting> valueProvider)
        {
            ArgumentNullException.ThrowIfNull(valueProvider);

            return valueProvider.Invoke(Get());
        }
    }
}
