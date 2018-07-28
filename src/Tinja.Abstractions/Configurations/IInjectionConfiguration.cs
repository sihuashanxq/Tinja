namespace Tinja.Abstractions.Configurations
{
    public interface IInjectionConfiguration : IConfiguration
    {
        bool EnablePropertyInjection { get; set; }
    }
}
