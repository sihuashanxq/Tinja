using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Resolving;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceScopeAdapterFactory : IServiceScopeFactory
    {
        protected IServiceResolver Resolver { get; }

        public ServiceScopeAdapterFactory(IServiceResolver resolver)
        {
            Resolver = resolver;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScopeAdapter(Resolver.CreateScope().Resolve<IServiceProvider>());
        }
    }
}
