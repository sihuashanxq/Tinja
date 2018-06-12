using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Extensions;
using Tinja.Interception.Members;

namespace Tinja.Interception
{
    internal class MemberInterceptionCollector : IMemberInterceptionCollector
    {
        private readonly ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<MemberInterception>> _caches;

        protected IMemberCollectorFactory MemberCollectorFactory { get; }

        protected IEnumerable<IMemberInterceptionProvider> Providers { get; }

        public MemberInterceptionCollector(IEnumerable<IMemberInterceptionProvider> providers, IMemberCollectorFactory memberCollectorFactory)
        {
            _caches = new ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<MemberInterception>>();
            Providers = providers;
            MemberCollectorFactory = memberCollectorFactory;
        }

        public IEnumerable<MemberInterception> Collect(Type serviceType, Type implementionType, bool caching = true)
        {
            return caching ? _caches.GetOrAdd(Tuple.Create(serviceType, implementionType), key => CollectFromType(serviceType, implementionType)) : CollectFromType(serviceType, implementionType);
        }

        protected virtual IEnumerable<MemberInterception> CollectFromType(Type serviceType, Type implementionType)
        {
            var map = new Dictionary<Type, MemberInterception>();
            var members = MemberCollectorFactory
                .Create(serviceType, implementionType)
                .Collect();

            foreach (var item in members.Where(i => !i.IsEvent))
            {
                CollectFromMember(item.Member, item.InterfaceMembers, map);
                CollectFromMember(item.DeclaringType, item.Interfaces, map);
            }

            return map.Values.ToList();
        }

        protected virtual void CollectFromMember(MemberInfo memberInfo, IEnumerable<MemberInfo> interfaceMembers, Dictionary<Type, MemberInterception> map)
        {
            var attrs = new HashSet<InterceptorAttribute>();

            foreach (var member in interfaceMembers.Concat(new[] { memberInfo }))
            {
                foreach (var attr in member.GetInterceptorAttributes())
                {
                    if (attr != null)
                    {
                        attrs.Add(attr);
                    }
                }

                CollectFromProvider(member, map);
            }

            foreach (var attr in attrs)
            {
                var item = map.GetValueOrDefault(attr.InterceptorType) ?? (map[attr.InterceptorType] = new MemberInterception()
                {
                    Interceptor = attr.InterceptorType,
                    Prioritys = new Dictionary<MemberInfo, long>()
                });

                item.Prioritys[memberInfo] = attr.Priority;
            }
        }

        protected virtual void CollectFromProvider(MemberInfo memberInfo, Dictionary<Type, MemberInterception> map)
        {
            foreach (var provider in Providers)
            {
                var entries = provider.GetInterceptions(memberInfo);
                if (entries == null)
                {
                    continue;
                }

                foreach (var entry in entries)
                {
                    var item = map.GetValueOrDefault(entry.Interceptor);
                    if (item == null)
                    {
                        map[entry.Interceptor] = entry;
                        continue;
                    }

                    foreach (var kv in entry.Prioritys)
                    {
                        item.Prioritys[kv.Key] = kv.Value;
                    }
                }
            }
        }
    }
}
