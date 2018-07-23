using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Extensions;
using Tinja.Core.Injection.Extensions;

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
            return new ServiceScopeAdapter(_serviceResolver.CreateScope().Resolve<IServiceProvider>());
        }
    }
}
