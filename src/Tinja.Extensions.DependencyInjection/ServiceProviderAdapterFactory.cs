using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Abstractions;
using Tinja.Core;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceProviderAdapterFactory : IServiceProviderFactory<IContainer>
    {
        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return container
                  .AddScoped<IServiceProvider>(r => new ServiceProviderAdapter(r))
                  .BuildServiceResolver()
                  .ResolveService<IServiceProvider>();
        }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            return services.BuildContainer();
        }
    }
}
