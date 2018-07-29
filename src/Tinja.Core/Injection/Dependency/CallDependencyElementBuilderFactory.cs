using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Dependency;
using Tinja.Abstractions.Injection.Descriptors;

namespace Tinja.Core.Injection.Dependency
{
    /// <summary>
    /// the default implementation for <see cref="ICallDependencyElementBuilderFactory"/>
    /// </summary>
    public class CallDependencyElementBuilderFactory : ICallDependencyElementBuilderFactory
    {
        private readonly IInjectionConfiguration _configuration;

        private readonly IServiceDescriptorFactory _serviceContextFactory;

        public CallDependencyElementBuilderFactory(IServiceDescriptorFactory serviceContextFactory, IInjectionConfiguration configuration)
        {
            _configuration = configuration;
            _serviceContextFactory = serviceContextFactory;
        }

        public ICallDependencyElementBuilder CreateBuilder()
        {
            return new CallDependencyElementBuilder(_serviceContextFactory, _configuration);
        }
    }
}
