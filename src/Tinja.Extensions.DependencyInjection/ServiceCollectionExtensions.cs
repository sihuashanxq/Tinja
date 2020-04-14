using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Abstractions;
using Tinja.Abstractions.Injection;
using Tinja.Core;

namespace Tinja.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IContainer BuildContainer(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            var container = new Container();

            foreach (var item in serviceCollection)
            {
                switch (item.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        AddServiceDefinition(container, item, ServiceLifeStyle.Singleton);
                        break;
                    case ServiceLifetime.Scoped:
                        AddServiceDefinition(container, item, ServiceLifeStyle.Scoped);
                        break;
                    case ServiceLifetime.Transient:
                        AddServiceDefinition(container, item, ServiceLifeStyle.Transient);
                        break;
                    default:
                        throw new NotSupportedException($"Lifetime:{item.Lifetime} is not supported!");
                }
            }

            container.AddSingleton<IServiceScopeFactory, ServiceScopeAdapterFactory>();
            container.AddSingleton<IServiceProviderFactory<IContainer>, ServiceProviderAdapterFactory>();

            return container;
        }

        private static void AddServiceDefinition(IContainer container, Microsoft.Extensions.DependencyInjection.ServiceDescriptor descriptor, ServiceLifeStyle lifeStyle)
        {
            if (descriptor.ImplementationFactory != null)
            {
                container.AddService(descriptor.ServiceType, resolver => descriptor.ImplementationFactory(resolver.ResolveService<IServiceProvider>()), lifeStyle);
            }
            else if (descriptor.ImplementationInstance != null)
            {
                container.AddSingleton(descriptor.ServiceType, descriptor.ImplementationInstance);
            }
            else
            {
                container.AddService(descriptor.ServiceType, descriptor.ImplementationType, lifeStyle);
            }
        }
    }
}
