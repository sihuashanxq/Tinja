using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Tinja.LifeStyle;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivatorProvider : IServiceActivatorProvider
    {
        private ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeStyleScope, object>> Cache { get; }

        public ServiceActivatorProvider()
        {
            Cache = new ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeStyleScope, object>>();
        }

        public Func<IServiceResolver, IServiceLifeStyleScope, object> Get(ServiceDependChain chain)
        {
            return Cache.GetOrAdd(chain.Context.ServiceType, (k) => BuildFactory(chain, new HashSet<ServiceDependChain>()));
        }

        public Func<IServiceResolver, IServiceLifeStyleScope, object> Get(Type resolvingType)
        {
            if (Cache.TryGetValue(resolvingType, out var factory))
            {
                return factory;
            }

            return null;
        }

        static Func<IServiceResolver, IServiceLifeStyleScope, object> BuildFactory(ServiceDependChain node, HashSet<ServiceDependChain> injectedProperties)
        {
            if (node.ContainsPropertyCircularDependencies())
            {
                return new ServicePropertyCircularInjectionActivatorFactory().CreateActivator(node);
            }

            return new ServiceInjectionActivatorFactory().CreateActivator(node);
        }
    }
}

