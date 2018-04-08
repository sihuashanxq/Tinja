using Microsoft.Extensions.DependencyInjection;
using Tinja.Resolving;

namespace Tinja.Extensions.DependencyInjection
{
    public class TinjaServiceScopeFactory : IServiceScopeFactory
    {
        public ServiceResolver ServiceResolver { get; }

        public TinjaServiceScopeFactory(ServiceResolver serviceResolver)
        {
            ServiceResolver = serviceResolver;
        }

        public IServiceScope CreateScope()
        {
            return new TinjaServiceScope(ServiceResolver.CreateScope());
        }
    }
}
