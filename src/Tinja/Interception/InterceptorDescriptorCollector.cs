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
    internal class InterceptorDescriptorCollector : IInterceptorDescriptorCollector
    {
        protected InterceptionConfiguration Configuration { get; }

        protected IMemberCollectorFactory MemberCollectorFactory { get; }

        protected IEnumerable<IInterceptorDescriptorProvider> Providers => Configuration.Providers;

        protected ConcurrentDictionary<Tuple<Type, Type>, InterceptorDescriptorCollection> Caches { get; }

        public InterceptorDescriptorCollector(InterceptionConfiguration configuration, IMemberCollectorFactory memberCollectorFactory)
        {
            Configuration = configuration;
            MemberCollectorFactory = memberCollectorFactory;
            Caches = new ConcurrentDictionary<Tuple<Type, Type>, InterceptorDescriptorCollection>();
        }

        public InterceptorDescriptorCollection Collect(Type serviceType, Type implementionType)
        {
            if (!Configuration.EnableInterception)
            {
                return new InterceptorDescriptorCollection(serviceType, implementionType);
            }

            return Caches.GetOrAdd(Tuple.Create(serviceType, implementionType), key => CollectInterceptors(serviceType, implementionType));
        }

        protected virtual InterceptorDescriptorCollection CollectInterceptors(Type serviceType, Type implementionType)
        {
            var interceptors = new InterceptorDescriptorCollection(serviceType, implementionType);
            var proxyMembers = MemberCollectorFactory.Create(serviceType, implementionType)
                .Collect()
                .Where(i => !i.IsEvent);

            foreach (var proxyMember in proxyMembers)
            {
                var typeInterceptors = CollectInterceptors(proxyMember.DeclaringType, proxyMember.Interfaces);
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

        protected virtual IEnumerable<InterceptorDescriptor> CollectInterceptors(MemberInfo memberInfo, IEnumerable<MemberInfo> interfaces)
        {
            foreach (var item in interfaces.Concat(new[] { memberInfo }))
            {
                foreach (var attr in item.GetInterceptorAttributes())
                {
                    yield return new InterceptorDescriptor(attr.Order, attr.InterceptorType, memberInfo);
                }

                foreach (var descriptor in Providers.SelectMany(provider => provider.GetInterceptors(item)))
                {
                    yield return new InterceptorDescriptor(descriptor.Order, descriptor.InterceptorType, memberInfo);
                }
            }
        }
    }
}
