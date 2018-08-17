using System.Collections.Generic;

namespace Tinja.Core.Injection
{
    internal class ServiceCacheIdProvider
    {
        private int _seed = 1;

        private readonly Dictionary<object, int> _idCaches;

        public ServiceCacheIdProvider()
        {
            _idCaches = new Dictionary<object, int>();
        }

        internal int GetServiceCacheId(object serviceKey)
        {
            if (_idCaches.TryGetValue(serviceKey, out var serviceId))
            {
                return serviceId;
            }

            lock (_idCaches)
            {
                if (_idCaches.TryGetValue(serviceKey, out serviceId))
                {
                    return serviceId;
                }

                return _idCaches[serviceKey] = _seed++;
            }
        }
    }
}
