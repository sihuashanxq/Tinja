using System;
using Tinja.Abstractions;

namespace Tinja.Extensions.DependencyInjection
{
    public static class ContainerExtensions
    {
        public static IServiceProvider BuildServiceProvider(this IContainer container)
        {
            return new ServiceProviderAdapterFactory().CreateServiceProvider(container);
        }
    }
}
