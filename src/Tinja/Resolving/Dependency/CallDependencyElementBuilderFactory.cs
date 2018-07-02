using Tinja.Configuration;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class CallDependencyElementBuilderFactory : ICallDependencyElementBuilderFactory
    {
        private readonly IServiceConfiguration _configuration;

        private readonly IServiceContextFactory _serviceContextFactory;

        public CallDependencyElementBuilderFactory(IServiceContextFactory serviceContextFactory, IServiceConfiguration configuration)
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
