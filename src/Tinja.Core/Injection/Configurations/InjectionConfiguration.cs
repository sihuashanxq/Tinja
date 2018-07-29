using Tinja.Abstractions.Configurations;
using Tinja.Abstractions.Injection.Configurations;

namespace Tinja.Core.Injection.Configurations
{
    public class InjectionConfiguration : Configuration, IInjectionConfiguration
    {
        public bool EnablePropertyInjection
        {
            get => Get<bool>(Constant.EnablePropertyInjectionKey);
            set => Set(Constant.EnablePropertyInjectionKey, value);
        }

        public InjectionConfiguration()
        {
            EnablePropertyInjection = true;
        }
    }
}
