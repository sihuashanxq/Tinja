using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Abstractions;
using Tinja.Abstractions.Injection.Extensions;
using Tinja.Core.Injection.Extensions;

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
