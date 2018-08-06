using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Dependencies;

namespace Tinja.Core.Injection.Dependencies
{
    /// <summary>
    /// the default implementation for <see cref="ICallDependencyElementBuilderFactory"/>
    /// </summary>
    public class CallDependElementBuilderFactory : ICallDependencyElementBuilderFactory
    {
        private readonly IServiceEntryFactory _serviceEntryFactory;

        private readonly IInjectionConfiguration _injectionConfiguration;

        public CallDependElementBuilderFactory(IServiceEntryFactory serviceEntryFactory, IInjectionConfiguration injectionConfiguration)
        {
            _serviceEntryFactory = serviceEntryFactory ?? throw new NullReferenceException(nameof(serviceEntryFactory));
            _injectionConfiguration = injectionConfiguration ?? throw new NullReferenceException(nameof(injectionConfiguration));
        }

        public ICallDependencyElementBuilder CreateBuilder()
        {
            return new CallDependElementBuilder(_serviceEntryFactory, _injectionConfiguration);
        }
    }
}
