using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Graphs;

namespace Tinja.Core.Injection.Dependencies
{
    /// <summary>
    /// the default implementation for <see cref="ICallDependElementBuilderFactory"/>
    /// </summary>
    public class GraphSiteBuilderFactory : IGraphSiteBuilderFactory
    {
        private readonly IServiceEntryFactory _serviceEntryFactory;

        private readonly IInjectionConfiguration _injectionConfiguration;

        public GraphSiteBuilderFactory(IServiceEntryFactory serviceEntryFactory, IInjectionConfiguration injectionConfiguration)
        {
            _serviceEntryFactory = serviceEntryFactory ?? throw new ArgumentNullException(nameof(serviceEntryFactory));
            _injectionConfiguration = injectionConfiguration ?? throw new ArgumentNullException(nameof(injectionConfiguration));
        }

        public IGraphSiteBuilder Create()
        {
            return new GraphSiteBuilder(_serviceEntryFactory, _injectionConfiguration);
        }
    }
}
