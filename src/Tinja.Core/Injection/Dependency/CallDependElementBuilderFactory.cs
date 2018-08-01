using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Dependency;

namespace Tinja.Core.Injection.Dependency
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
            if (serviceEntryFactory == null)
            {
                throw new NullReferenceException(nameof(serviceEntryFactory));
            }

            if (injectionConfiguration == null)
            {
                throw new NullReferenceException(nameof(injectionConfiguration));
            }

            _serviceEntryFactory = serviceEntryFactory;
            _injectionConfiguration = injectionConfiguration;
        }

        public ICallDependencyElementBuilder CreateBuilder()
        {
            return new CallDependElementBuilder(_serviceEntryFactory, _injectionConfiguration);
        }
    }
}
