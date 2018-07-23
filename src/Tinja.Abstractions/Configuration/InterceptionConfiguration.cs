using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Definitions;

namespace Tinja.Abstractions.Configuration
{
    public class InterceptionConfiguration
    {
        public bool EnableInterception { get; set; } = true;

        public List<IInterceptorDefinitionCollector> Collectors { get; }

        public InterceptionConfiguration()
        {
            Collectors = new List<IInterceptorDefinitionCollector>();
        }
    }
}
