using System.Collections.Generic;

namespace Tinja.Core.Injection
{
    internal class ServiceIdProvider
    {
        private int _seed = 1;

        private readonly Dictionary<object, int> _ids;

        internal ServiceIdProvider()
        {
            _ids = new Dictionary<object, int>();
        }

        internal int GetServiceId(object key)
        {
            lock (_ids)
            {
                if (_ids.TryGetValue(key, out var id))
                {
                    return id;
                }

                return _ids[key] = _seed++;
            }
        }
    }
}
