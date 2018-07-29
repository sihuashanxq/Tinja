using Tinja.Abstractions.Configurations;
using Tinja.Abstractions.DynamicProxy.Configurations;

namespace Tinja.Core.DynamicProxy.Configurations
{
    public class DynamicProxyConfiguration : Configuration, IDynamicProxyConfiguration
    {
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
            EnableInterfaceProxy = true;
            EnableAstractionClassProxy = true;
        }
    }
}
