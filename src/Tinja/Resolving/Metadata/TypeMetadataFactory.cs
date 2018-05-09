using System;
using System.Collections.Concurrent;

namespace Tinja.Resolving.Metadata
{
    public class TypeMetadataFactory : ITypeMetadataFactory
    {
        private ConcurrentDictionary<Type, TypeMetadata> _metas;

        public TypeMetadataFactory()
        {
            _metas = new ConcurrentDictionary<Type, TypeMetadata>();
        }

        public TypeMetadata Create(Type serviceType)
        {
            return _metas.GetOrAdd(serviceType, (_) => new TypeMetadata(serviceType));
        }
    }
}
