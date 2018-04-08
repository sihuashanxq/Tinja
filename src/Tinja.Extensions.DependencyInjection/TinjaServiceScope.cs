using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Resolving;

namespace Tinja.Extensions.DependencyInjection
{
    public class TinjaServiceScope : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; }

        private IServiceResolver _serviceResolver;

        public TinjaServiceScope(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
            ServiceProvider = serviceResolver.Resolve<IServiceProvider>();
        }

        public void Dispose()
        {
            _serviceResolver.Dispose();
        }
    }
}
