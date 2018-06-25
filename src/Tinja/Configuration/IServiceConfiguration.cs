namespace Tinja.Configuration
{
    public interface IServiceConfiguration
    {
        InjectionConfiguration Injection { get; }

        InterceptionConfiguration Interception { get; }
    }
}
