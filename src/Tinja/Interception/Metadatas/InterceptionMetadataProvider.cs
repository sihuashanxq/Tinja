using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptionMetadataProvider : IInterceptionMetadataProvider
    {
        private ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<InterceptionMetadata>> _metaCache;

        public InterceptionMetadataProvider()
        {
            _metaCache = new ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<InterceptionMetadata>>();
        }

        public IEnumerable<InterceptionMetadata> GetInterceptionMetadatas(Type serviceType, Type implementionType)
        {
            return _metaCache.GetOrAdd(GetCacheKey(serviceType, implementionType), _ =>
            {
                return null;
            });
        }

        protected IEnumerable<InterceptionMetadata> Collect(IEnumerable<TypeMember> typeMembers)
        {
            var metas = new HashSet<InterceptionMetadata>();
        }

        protected virtual InterceptionMetadata[] CollectInterceptionMetaMetaDatas(
            MemberInfo target,
            IEnumerable<MemberInfo> interfaceMembers,
            Dictionary<Type, InterceptionMetadata> interceptorMap
        )
        {
            var interceptorAttributes = interfaceMembers
                .SelectMany(i => i.GetInterceptorAttributes())
                .Where(i => i.Inherited)
                .Concat(target.GetInterceptorAttributes())
                .ToList();

            for (var i = 0; i < interceptorAttributes.Count; i++)
            {
                var attribute = interceptorAttributes[i];
                var metaData = interceptorMap.GetValueOrDefault(attribute.InterceptorType);
                if (metaData == null)
                {
                    metaData = interceptorMap[attribute.InterceptorType] = new InterceptionMetadata(attribute);
                }
            }

            return interceptorMap;
        }

        private static Tuple<Type, Type> GetCacheKey(Type serviceType, Type implementionType)
        {
            return Tuple.Create(serviceType, implementionType);
        }
    }
}
