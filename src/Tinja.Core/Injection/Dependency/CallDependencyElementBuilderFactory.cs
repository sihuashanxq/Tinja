using Tinja.Abstractions.Configurations;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependency;

namespace Tinja.Core.Injection.Dependency
{
    public class CallDependencyElementBuilderFactory : ICallDependencyElementBuilderFactory
    {
        private readonly IContainerConfiguration _configuration;

        private readonly IServiceDescriptorFactory _serviceContextFactory;

        public CallDependencyElementBuilderFactory(IServiceDescriptorFactory serviceContextFactory, IContainerConfiguration configuration)
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
