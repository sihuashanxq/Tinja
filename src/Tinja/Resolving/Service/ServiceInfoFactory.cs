using System;
using System.Collections.Concurrent;

namespace Tinja.Resolving.Service
{
    public class ServiceInfoFactory : IServiceInfoFactory
    {
        private ConcurrentDictionary<Type, ServiceInfo> _serviceInfoCaches;

        public ServiceInfoFactory()
        {
            _serviceInfoCaches = new ConcurrentDictionary<Type, ServiceInfo>();
        }

        public ServiceInfo Create(Type serviceType)
        {
            return _serviceInfoCaches.GetOrAdd(serviceType, (_) => new ServiceInfo(serviceType));
        }
    }
}
