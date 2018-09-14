using System;
using System.Collections.Generic;

namespace Tinja.Core.Injection
{
    internal class ServiceCacheIdProvider
    {
        private int _seed = 1;

        private readonly Dictionary<object, int> _idMaps;

        public ServiceCacheIdProvider()
        {
            _idMaps = new Dictionary<object, int>();
        }

        internal int GetServiceCacheId(object serviceKey)
        {
            lock (_idMaps)
            {
                if (_idMaps.TryGetValue(serviceKey, out var serviceId))
                {
                    return serviceId;
                }

                return _idMaps[serviceKey] = _seed++;
            }
        }
    }
}
