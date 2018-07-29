using Tinja.Abstractions.DynamicProxy.Configurations;
using Tinja.Abstractions.Injection.Configurations;

namespace Tinja.Abstractions.Configurations
{
    public interface IContainerConfiguration : IConfiguration
    {
        IInjectionConfiguration Injection { get; set; }

        IDynamicProxyConfiguration DynamicProxy { get; set; }
    }
}
