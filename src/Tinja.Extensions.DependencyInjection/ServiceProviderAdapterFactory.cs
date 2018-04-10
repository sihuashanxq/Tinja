using Microsoft.Extensions.DependencyInjection;
using System;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceProviderAdapterFactory : IServiceProviderFactory<IContainer>
    {
        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return container
                  .AddScoped(typeof(IServiceProvider), resolver => new ServiceProviderAdapter(resolver))
                  .BuildResolver()
                  .Resolve<IServiceProvider>();
        }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            return services.BuildContainer();
        }
    }
}
