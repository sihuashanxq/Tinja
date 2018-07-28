namespace Tinja.Core.Configurations
{
    using Tinja.Abstractions.Configurations;

    public class ContainerCongfiguration : Configuration, IContainerConfiguration
    {
        public IInjectionConfiguration Injection
        {
            get => Get<IInjectionConfiguration>(Constant.InjectionConfigurationKey);
            set => Set(Constant.InjectionConfigurationKey, value);
        }

        public IDynamicProxyConfiguration DynamicProxy
        {
            get => Get<IDynamicProxyConfiguration>(Constant.DynamicProxyConfigurationKey);
            set => Set(Constant.DynamicProxyConfigurationKey, value);
        }

        public ContainerCongfiguration()
        {
            Set(Constant.InjectionConfigurationKey, new InjectionConfiguration());
            Set(Constant.DynamicProxyConfigurationKey, new DynamicProxyConfiguration());
        }
    }
}
