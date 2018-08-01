using System;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    internal class ServiceFactory : IServiceFactory
    {
        internal ServiceLifeScope Scope { get; }

        internal ServiceFactory(ServiceLifeScope scope)
        {
            Scope = scope;
        }

        public object CreateService(Func<IServiceResolver, object> factory)
        {
            var service = factory(Scope.ServiceResolver);
            if (service is IDisposable disposable)
            {
                Scope.DisposableServices.Add(disposable);
            }

            return service;
        }

        public object CreateService(int serviceId, Func<IServiceResolver, object> factory)
        {
            if (Scope.ResolvedServices.TryGetValue(serviceId, out var service))
            {
                return service;
            }

            lock (Scope.ResolvedServices)
            {
                if (Scope.ResolvedServices.TryGetValue(serviceId, out service))
                {
                    return service;
                }

                service = Scope.ResolvedServices[serviceId] = factory(Scope.ServiceResolver);

                if (service is IDisposable disposable)
                {
                    Scope.DisposableServices.Add(disposable);
                }

                return service;
            }
        }
    }
}
