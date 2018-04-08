using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Resolving;

namespace Tinja.Extensions.DependencyInjection
{
    public class TinjaServiceProvider : IServiceProvider
    {
        private IServiceResolver _serviceResolver;

        public TinjaServiceProvider(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
        }

        public object GetService(Type serviceType)
        {
            return _serviceResolver.Resolve(serviceType);
        }
    }
}
