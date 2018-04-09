using Microsoft.Extensions.DependencyInjection;
using System;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return container.AddScoped(typeof(IServiceProvider), resolver => resolver).BuildResolver();
        }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            return services.BuildContainer();
        }
    }
}
