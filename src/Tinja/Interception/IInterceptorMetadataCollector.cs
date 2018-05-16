using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace Tinja.Interception
{
    public interface IInterceptorMetadataCollector
    {
        IEnumerable<InterceptorMetadata> Collect(Type serviceType, Type implementionType);
    }

    public class InterceptorMetadataCollector : IInterceptorMetadataCollector
    {
        private ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<InterceptorMetadata>> _metas;

        public InterceptorMetadataCollector()
        {
            _metas = new ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<InterceptorMetadata>>();
        }

        public IEnumerable<InterceptorMetadata> Collect(Type serviceType, Type implementionType)
        {
            return _metas.GetOrAdd(GetCacheKey(serviceType, implementionType), _ =>
            {
                return null;
            });
        }

        public IEnumerable<InterceptorMetadata> Collect(IEnumerable<TypeMembers.TypeMemberMetadata> typeMembers)
        {
            return null;
        }

        public Tuple<Type, Type> GetCacheKey(Type serviceType, Type implementionType)
        {
            return Tuple.Create(serviceType, implementionType);
        }
    }
}
