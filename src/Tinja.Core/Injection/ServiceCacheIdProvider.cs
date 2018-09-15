using System.Collections.Generic;

namespace Tinja.Core.Injection
{
    internal class ServiceCacheIdProvider
    {
        private int _seed = 1;

        private readonly Dictionary<object, int> _ids;

        internal ServiceCacheIdProvider()
        {
            _ids = new Dictionary<object, int>();
        }

        internal int GetCacheId(object serviceKey)
        {
            lock (_ids)
            {
                if (_ids.TryGetValue(serviceKey, out var id))
                {
                    return id;
                }

                return _ids[serviceKey] = _seed++;
            }
        }
    }
}
