using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Abstractions.Configuration
{
    public class InterceptionConfiguration
    {
        public bool EnableInterception { get; set; } = true;

        public List<IInterceptorMetadataCollector> Collectors { get; }

        public InterceptionConfiguration()
        {
            Collectors = new List<IInterceptorMetadataCollector>();
        }
    }
}
