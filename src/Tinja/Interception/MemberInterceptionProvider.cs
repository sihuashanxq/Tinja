using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Interception.Members;

namespace Tinja.Interception
{
    internal class MemberInterceptionProvider : IMemberInterceptionProvider
    {
        private readonly ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<MemberInterception>> _caches;

        protected IMemberCollectorFactory MemberCollectorFactory { get; }

        protected IEnumerable<IMemberAddtionalInterceptionProvider> Providers { get; }

        public MemberInterceptionProvider(IEnumerable<IMemberAddtionalInterceptionProvider> providers, IMemberCollectorFactory memberCollectorFactory)
        {
            Providers = providers;
            MemberCollectorFactory = memberCollectorFactory;
            _caches = new ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<MemberInterception>>();
        }

        public IEnumerable<MemberInterception> GetInterceptions(Type serviceType, Type implementionType)
        {
            return _caches.GetOrAdd(Tuple.Create(serviceType, implementionType), key => CollectInterceptions(serviceType, implementionType));
        }

        protected virtual IEnumerable<MemberInterception> CollectInterceptions(Type serviceType, Type implementionType)
        {
            var map = new Dictionary<Type, MemberInterception>();
            var members = MemberCollectorFactory
                .Create(serviceType, implementionType)
                .Collect();

            foreach (var item in members.Where(i => !i.IsEvent))
            {
                CollectInterceptions(item.Member, item.InterfaceMembers, map);
                CollectInterceptions(item.DeclaringType, item.Interfaces, map);
                CollectAdditionInterceptions(item.Member, map);
                CollectAdditionInterceptions(item.DeclaringType, map);
            }

            return map.Values.ToList();
        }

        protected virtual void CollectInterceptions(MemberInfo memberInfo, IEnumerable<MemberInfo> interfaceMembers, Dictionary<Type, MemberInterception> map)
        {
            var attributes = interfaceMembers
                .SelectMany(i => i.GetInterceptorAttributes())
                .Where(i => i.Inherited)
                .Concat(memberInfo.GetInterceptorAttributes())
                .Distinct();

            foreach (var attribute in attributes)
            {
                var interception = map.GetValueOrDefault(attribute.InterceptorType);
                if (interception == null)
                {
                    interception = map[attribute.InterceptorType] = new MemberInterception()
                    {
                        Interceptor = attribute.InterceptorType,
                        Prioritys = new Dictionary<MemberInfo, long>()
                    };
                }

                interception.Prioritys[memberInfo] = attribute.Priority;
            }
        }

        protected virtual void CollectAdditionInterceptions(MemberInfo memberInfo, Dictionary<Type, MemberInterception> map)
        {
            foreach (var provider in Providers)
            {
                var interceptions = provider.GetInterceptions(memberInfo);
                if (interceptions == null)
                {
                    continue;
                }

                foreach (var item in interceptions)
                {
                    var interception = map.GetValueOrDefault(item.Interceptor);
                    if (interception == null)
                    {
                        map[item.Interceptor] = item;
                        continue;
                    }

                    foreach (var kv in item.Prioritys)
                    {
                        interception.Prioritys[kv.Key] = kv.Value;
                    }
                }
            }
        }
    }
}
