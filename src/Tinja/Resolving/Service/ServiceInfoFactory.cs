using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Tinja.Annotations;

namespace Tinja.Resolving.Service
{
    public class ServiceInfoFactory : IServiceInfoFactory
    {
        public ConcurrentDictionary<Type, ServiceInfo> _caches;

        public ServiceInfoFactory()
        {
            _caches = new ConcurrentDictionary<Type, ServiceInfo>();
        }

        public ServiceInfo Create(Type serviceType)
        {
            return _caches.GetOrAdd(serviceType, (_) => new ServiceInfo(serviceType));
        }
    }
}
