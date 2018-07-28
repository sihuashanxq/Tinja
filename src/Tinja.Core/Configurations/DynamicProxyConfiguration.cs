namespace Tinja.Core.Configurations
{
    using Tinja.Abstractions.Configurations;

    public class DynamicProxyConfiguration : Configuration, IDynamicProxyConfiguration
    {
        public bool EnableDynamicProxy
        {
            get => Get<bool>(Constant.EnableDynamicProxyKey);
            set => Set(Constant.EnableDynamicProxyKey, value);
        }

        public bool EnableInterfaceProxy
        {
            get => Get<bool>(Constant.EnableInterfaceProxyKey);
            set => Set(Constant.EnableInterfaceProxyKey, value);
        }

        public bool EnableAstractionClassProxy
        {
            get => Get<bool>(Constant.EnableAstractionClassProxyKey);
            set => Set(Constant.EnableAstractionClassProxyKey, value);
        }

        public DynamicProxyConfiguration()
        {
            Set(Constant.EnableDynamicProxyKey, true);
            Set(Constant.EnableInterfaceProxyKey, true);
            Set(Constant.EnableAstractionClassProxyKey, true);
        }
    }
}
