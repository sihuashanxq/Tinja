using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tinja.Extensions.DependencyInjection
{
    public class TinjaServiceProviderFactory<TServiceProvider> : IServiceProviderFactory<TServiceProvider>
    {
        public TServiceProvider CreateBuilder(IServiceCollection services)
        {
            throw new NotImplementedException();
        }

        public IServiceProvider CreateServiceProvider(TServiceProvider containerBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
