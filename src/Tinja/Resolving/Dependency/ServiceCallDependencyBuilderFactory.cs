using Tinja.Configuration;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallDependencyBuilderFactory : IServiceCallDependencyBuilderFactory
    {
        private readonly IServiceConfiguration _configuration;

        private readonly IServiceContextFactory _serviceContextFactory;

        public ServiceCallDependencyBuilderFactory(IServiceContextFactory serviceContextFactory, IServiceConfiguration configuration)
        {
            _configuration = configuration;
            _serviceContextFactory = serviceContextFactory;
        }

        public IServiceCallDependencyElement CreateBuilder()
        {
            return new ServiceCallDependencyBuilder(_serviceContextFactory, _configuration);
        }
    }
}
