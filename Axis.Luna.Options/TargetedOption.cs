using Microsoft.Extensions.Options;

namespace Axis.Luna.Options
{
    public class TargetedOption<TSetting, TTarget> :
        IOptions<TSetting, TTarget>
        where TSetting : class
    {
        private readonly string discriminator;
        private readonly IOptionsMonitor<TSetting> options;

        public TargetedOption(
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
