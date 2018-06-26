namespace Tinja.Configuration
{
    public class InjectionConfiguration
    {
        public bool EnablePropertyInjection { get; set; } = true;

        public uint PropertyCircularDepth { get; set; } = 2;
    }
}
