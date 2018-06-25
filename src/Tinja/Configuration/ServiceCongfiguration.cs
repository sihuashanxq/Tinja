using System.Collections.Generic;
using Tinja.Interception;

namespace Tinja.Configuration
{
    public class ServiceCongfiguration : IServiceConfiguration
    {
        public InjectionConfiguration Injection { get; }

        public List<IMemberInterceptionProvider> InterceptionProviders { get; }

        public ServiceCongfiguration()
        {
            Injection = new InjectionConfiguration();
            InterceptionProviders = new List<IMemberInterceptionProvider>();
        }
    }
}
