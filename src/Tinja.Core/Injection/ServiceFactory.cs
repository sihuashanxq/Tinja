using System;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceFactory : IServiceFactory
    {
        internal ServiceLifeScope Scope { get; }

        public ServiceFactory(ServiceLifeScope scope)
        {
            Scope = scope;
        }

        public object CreateService(Func<IServiceResolver, object> factory)
        {
            var service = factory(Scope.ServiceResolver);

            CaptureDisposableService(service);

            return service;
        }

        public object CreateService(long serviceId, Func<IServiceResolver, object> factory)
        {
            if (Scope.CacheResolvedServices.TryGetValue(serviceId, out var service))
            {
                return service;
            }

            lock (Scope.CacheResolvedServices)
            {
                if (Scope.CacheResolvedServices.TryGetValue(serviceId, out service))
                {
                    return service;
                }

                service = Scope.CacheResolvedServices[serviceId] = factory(Scope.ServiceResolver);

                CaptureDisposableService(service);

                return service;
            }
        }

        protected void CaptureDisposableService(object service)
        {
            if (service is IDisposable disposable)
            {
                Scope.DisposableServices.Add(disposable);
            }
        }
    }
}
