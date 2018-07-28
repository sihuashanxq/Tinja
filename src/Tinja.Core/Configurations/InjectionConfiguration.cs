namespace Tinja.Core.Configurations
{
    using Tinja.Abstractions.Configurations;

    public class InjectionConfiguration : Configuration, IInjectionConfiguration
    {
        public bool EnablePropertyInjection
        {
            get => Get<bool>(Constant.EnablePropertyInjectionKey);
            set => Set(Constant.EnablePropertyInjectionKey, value);
        }

        public InjectionConfiguration()
        {
            Set(Constant.EnablePropertyInjectionKey, true);
        }
    }
}
