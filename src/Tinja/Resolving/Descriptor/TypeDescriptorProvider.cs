using System;
using System.Collections.Concurrent;

namespace Tinja.Resolving.Descriptor
{
    public class TypeDescriptorProvider : ITypeDescriptorProvider
    {
        public ConcurrentDictionary<Type, TypeDescriptor> _caches;

        public TypeDescriptorProvider()
        {
            _caches = new ConcurrentDictionary<Type, TypeDescriptor>();
        }

        public TypeDescriptor Get(Type type)
        {
            return _caches.GetOrAdd(type, (_) => new TypeDescriptor(type));
        }
    }
}
