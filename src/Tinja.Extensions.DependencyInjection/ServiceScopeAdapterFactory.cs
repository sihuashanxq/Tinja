using Microsoft.Extensions.DependencyInjection;
using Tinja.Resolving;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceScopeAdapterFactory : IServiceScopeFactory
    {
        public IServiceResolver Resolver { get; }

        public ServiceScopeAdapterFactory(IServiceResolver resolver)
        {
            Resolver = resolver;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScopeAdapter(Resolver.CreateScope());
        }
    }
}
