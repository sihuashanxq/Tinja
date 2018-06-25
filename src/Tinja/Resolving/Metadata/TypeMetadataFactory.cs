using System;
using System.Collections.Concurrent;

namespace Tinja.Resolving.Metadata
{
    public class TypeMetadataFactory : ITypeMetadataFactory
    {
        private readonly ConcurrentDictionary<Type, TypeMetadata> _caches;

        public TypeMetadataFactory()
        {
            _caches = new ConcurrentDictionary<Type, TypeMetadata>();
        }

        public TypeMetadata Create(Type serviceType)
        {
            return _caches.GetOrAdd(serviceType, _ => new TypeMetadata(serviceType));
        }
    }
}
