namespace Tinja.Configuration
{
    public class InjectionConfiguration
    {
        public bool PropertyInjectionEnabled { get; set; } = true;

        public uint PropertyInjectionCircularDepth { get; set; } = 2;
    }
}
