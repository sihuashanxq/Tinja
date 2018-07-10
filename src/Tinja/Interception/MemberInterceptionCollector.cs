using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Configuration;
using Tinja.Extensions;
using Tinja.Interception.Members;

namespace Tinja.Interception
{
    internal class MemberInterceptionCollector : IMemberInterceptionCollector
    {
        protected InterceptionConfiguration Configuration { get; }

        protected IMemberCollectorFactory MemberCollectorFactory { get; }

        protected IEnumerable<IMemberInterceptionProvider> Providers => Configuration.Providers;

        protected ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<MemberInterception>> Caches { get; }

        public MemberInterceptionCollector(InterceptionConfiguration configuration, IMemberCollectorFactory memberCollectorFactory)
        {
            Configuration = configuration;
            MemberCollectorFactory = memberCollectorFactory;
            Caches = new ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<MemberInterception>>();
        }

        public IEnumerable<MemberInterception> Collect(Type serviceType, Type implementionType)
        {
            if (!Configuration.EnableInterception)
            {
                return new MemberInterception[0];
            }

            return Caches.GetOrAdd(Tuple.Create(serviceType, implementionType), key => CollectFromType(serviceType, implementionType));
        }

        protected virtual IEnumerable<MemberInterception> CollectFromType(Type serviceType, Type implementionType)
        {
            var typeInterceptions = new Dictionary<Type, MemberInterception>();
            var collectedMembers = MemberCollectorFactory.Create(serviceType, implementionType).Collect();

            foreach (var item in collectedMembers.Where(i => !i.IsEvent))
            {
                CollectFromMember(item.Member, item.InterfaceMembers, typeInterceptions);
                CollectFromMember(item.DeclaringType, item.Interfaces, typeInterceptions);
            }

            return typeInterceptions.Values.ToList();
        }

        protected virtual void CollectFromMember(MemberInfo memberInfo, IEnumerable<MemberInfo> interfaceMembers, Dictionary<Type, MemberInterception> typeInterceptions)
        {
            var attrs = new HashSet<InterceptorAttribute>();

            foreach (var member in interfaceMembers.Concat(new[] { memberInfo }))
            {
                foreach (var attr in member.GetInterceptorAttributes().Where(item => item != null))
                {
                    attrs.Add(attr);
                }

                CollectFromProvider(member, typeInterceptions);
            }

            foreach (var attr in attrs)
            {
                var item = typeInterceptions.GetValueOrDefault(attr.InterceptorType) ?? (typeInterceptions[attr.InterceptorType] = new MemberInterception()
                {
                    InterceptorType = attr.InterceptorType,
                    MemberOrders = new Dictionary<MemberInfo, long>()
                });

                item.MemberOrders[memberInfo] = attr.Order;
            }
        }

        protected virtual void CollectFromProvider(MemberInfo memberInfo, Dictionary<Type, MemberInterception> typeInterceptions)
        {
            foreach (var provider in Providers)
            {
                var interceptions = provider.GetInterceptions(memberInfo);
                if (interceptions == null)
                {
                    continue;
                }

                foreach (var interception in interceptions)
                {
                    if (!typeInterceptions.TryGetValue(interception.InterceptorType, out var orders))
                    {
                        typeInterceptions[interception.InterceptorType] = interception;
                        continue;
                    }

                    foreach (var kv in interception.MemberOrders)
                    {
                        orders.MemberOrders[kv.Key] = kv.Value;
                    }
                }
            }
        }
    }
}
