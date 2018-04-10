using System;

namespace Tinja.Extensions.DependencyInjection
{
    public static class ContainerExtensions
    {
        public static IServiceProvider BuildServiceProvider(this IContainer ioc)
        {
            return new ServiceProviderAdapterFactory().CreateServiceProvider(ioc);
        }
    }
}
