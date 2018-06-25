using System.Collections.Generic;
using Tinja.Interception;

namespace Tinja.Configuration
{
    public class ServiceCongfiguration : IServiceConfiguration
    {
        public InjectionConfiguration Injection { get; }

        public InterceptionConfiguration Interception { get; }

        public ServiceCongfiguration()
        {
            Injection = new InjectionConfiguration();
            Interception = new InterceptionConfiguration();
        }
    }
}
