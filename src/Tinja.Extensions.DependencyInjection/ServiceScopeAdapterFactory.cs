using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Core;
using Tinja.Core.Extensions;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceScopeAdapterFactory : IServiceScopeFactory
    {
        private readonly IServiceResolver _serviceResolver;

        public ServiceScopeAdapterFactory(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScopeAdapter(_serviceResolver.CreateScope().ResolveService<IServiceProvider>());
        }
    }
}
