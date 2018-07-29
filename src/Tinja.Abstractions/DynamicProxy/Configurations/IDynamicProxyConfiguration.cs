using Tinja.Abstractions.Configurations;

namespace Tinja.Abstractions.DynamicProxy.Configurations
{
    /// <summary>
    /// </summary>
    public interface IDynamicProxyConfiguration : IConfiguration
    {
        bool EnableDynamicProxy { get; set; }

        bool EnableInterfaceProxy { get; set; }

        bool EnableAstractionClassProxy { get; set; }
    }
}
