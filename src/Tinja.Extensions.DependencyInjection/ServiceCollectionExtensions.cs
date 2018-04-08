using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.LifeStyle;

namespace Tinja.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IContainer BuildContainer(this IServiceCollection services)
        {
            var ioc = new Container();

            if (services == null)
            {
                return ioc;
            }

            foreach (var item in services)
            {
                switch (item.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        AddService(ioc, item, ServiceLifeStyle.Singleton);
                        break;
                    case ServiceLifetime.Scoped:
                        AddService(ioc, item, ServiceLifeStyle.Scoped);
                        break;
                    case ServiceLifetime.Transient:
                        AddService(ioc, item, ServiceLifeStyle.Transient);
                        break;
                }
            }

            ioc.AddSingleton<IServiceScopeFactory, TinjaServiceScopeFactory>();

            return ioc;
        }

        private static void AddService(IContainer ioc, ServiceDescriptor service, ServiceLifeStyle lifeStyle)
        {
            if (service.ImplementationFactory != null)
            {
                ioc.AddService(
                    service.ServiceType,
                    (resolver) => service.ImplementationFactory((IServiceProvider)resolver.Resolve(typeof(IServiceProvider))),
                    lifeStyle
                );
            }

            else if (service.ImplementationInstance != null)
            {
                ioc.AddService(
                    service.ServiceType,
                    (resolver) => service.ImplementationInstance,
                    lifeStyle
                );
            }
            else
            {
                ioc.AddService(service.ServiceType, service.ImplementationType, lifeStyle);
            }
        }
    }
}
