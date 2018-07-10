using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Resolving;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceScopeAdapterFactory : IServiceScopeFactory
    {
        private readonly IServiceResolver _resolver;

        public ServiceScopeAdapterFactory(IServiceResolver resolver)
        {
            _resolver = resolver;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScopeAdapter(_resolver.CreateScope().Resolve<IServiceProvider>());
        }
    }
}
