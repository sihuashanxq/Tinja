using System.Collections.Generic;
using Tinja.Interception;

namespace Tinja.Configuration
{
    public interface IServiceConfiguration
    {
        InjectionConfiguration Injection { get; }

        List<IMemberInterceptionProvider> InterceptionProviders { get; }
    }
}
