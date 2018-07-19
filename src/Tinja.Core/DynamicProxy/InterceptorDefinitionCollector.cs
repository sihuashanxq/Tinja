using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection.Extensions;
using Tinja.Core.DynamicProxy.Members;

namespace Tinja.Core.DynamicProxy
{
    internal class InterceptorDefinitionCollector : IInterceptorDefinitionCollector
    {
        protected InterceptionConfiguration Configuration { get; }

        protected ITypeMemberCollectorFactory MemberCollectorFactory { get; }

        protected IEnumerable<IInterceptorDefinitionProvider> Providers => Configuration.Providers;

        protected ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<InterceptorDefinition>> Caches { get; }

        public InterceptorDefinitionCollector(InterceptionConfiguration configuration, ITypeMemberCollectorFactory memberCollectorFactory)
        {
            Configuration = configuration;
            MemberCollectorFactory = memberCollectorFactory;
            Caches = new ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<InterceptorDefinition>>();
        }

        public IEnumerable<InterceptorDefinition> CollectDefinitions(Type serviceType, Type implementionType)
        {
            if (!Configuration.EnableInterception)
            {
                return new InterceptorDefinition[0];
            }

            return Caches.GetOrAdd(Tuple.Create(serviceType, implementionType), key => CollectInterceptors(serviceType, implementionType));
        }

        protected virtual IEnumerable<InterceptorDefinition> CollectInterceptors(Type serviceType, Type implementionType)
        {
            var interceptors = new List<InterceptorDefinition>();
            var proxyMembers = MemberCollectorFactory.Create(serviceType, implementionType)
                .Collect()
                .Where(i => !i.IsEvent);

            foreach (var proxyMember in proxyMembers)
            {
                var typeInterceptors = CollectInterceptors(proxyMember.DeclaringType, proxyMember.InterfaceInherits);
                var memberInterceptors = CollectInterceptors(proxyMember.Member, proxyMember.InterfaceMembers);

                if (typeInterceptors != null)
                {
                    interceptors.AddRange(typeInterceptors);
                }

                if (memberInterceptors != null)
                {
                    interceptors.AddRange(memberInterceptors);
                }
            }

            return interceptors;
        }

        protected virtual IEnumerable<InterceptorDefinition> CollectInterceptors(MemberInfo memberInfo, IEnumerable<MemberInfo> interfaces)
        {
            foreach (var item in interfaces.Concat(new[] { memberInfo }))
            {
                foreach (var attr in item.GetInterceptorAttributes())
                {
                    yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, memberInfo);
                }

                foreach (var descriptor in Providers.SelectMany(provider => provider.GetDefinitions(item)))
                {
                    yield return new InterceptorDefinition(descriptor.Order, descriptor.InterceptorType, memberInfo);
                }
            }
        }
    }
}
