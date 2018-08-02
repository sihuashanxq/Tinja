using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Abstractions;
using Tinja.Abstractions.Extensions;
using Tinja.Core;
using Tinja.Core.Extensions;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceProviderAdapterFactory : IServiceProviderFactory<IContainer>
    {
        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return container
                  .AddScoped(typeof(IServiceProvider), resolver => new ServiceProviderAdapter(resolver))
                  .BuildServiceResolver()
                  .ResolveService<IServiceProvider>();
        }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            return services.BuildContainer();
        }
    }
}
