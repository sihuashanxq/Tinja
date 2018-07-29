using Tinja.Abstractions.Configurations;

namespace Tinja.Abstractions.Injection.Configurations
{
    public interface IInjectionConfiguration : IConfiguration
    {
        bool EnablePropertyInjection { get; set; }
    }
}
