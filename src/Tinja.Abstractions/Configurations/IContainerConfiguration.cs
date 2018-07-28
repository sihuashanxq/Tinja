namespace Tinja.Abstractions.Configurations
{
    public interface IContainerConfiguration : IConfiguration
    {
        IInjectionConfiguration Injection { get; set; }

        IDynamicProxyConfiguration DynamicProxy { get; set; }
    }
}
