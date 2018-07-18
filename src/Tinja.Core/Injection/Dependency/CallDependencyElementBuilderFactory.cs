using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependency;

namespace Tinja.Core.Injection.Dependency
{
    public class CallDependencyElementBuilderFactory : ICallDependencyElementBuilderFactory
    {
        private readonly IServiceConfiguration _configuration;

        private readonly IServiceDescriptorFactory _serviceContextFactory;

        public CallDependencyElementBuilderFactory(IServiceDescriptorFactory serviceContextFactory, IServiceConfiguration configuration)
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
