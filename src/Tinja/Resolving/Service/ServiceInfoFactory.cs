using System;
using System.Collections.Concurrent;

namespace Tinja.Resolving.Service
{
    public class ServiceInfoFactory : IServiceInfoFactory
    {
        public ConcurrentDictionary<Type, ServiceInfo> _caches;

        public ServiceInfoFactory()
        {
            _caches = new ConcurrentDictionary<Type, ServiceInfo>();
        }

        public ServiceInfo Create(Type type)
        {
            return _caches.GetOrAdd(type, (_) => new ServiceInfo(type));
        }
    }
}
