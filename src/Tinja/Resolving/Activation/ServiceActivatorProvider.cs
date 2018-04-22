using System;
using System.Collections.Concurrent;

using Tinja.ServiceLife;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivatorProvider : IServiceActivatorProvider
    {
        protected ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>> Activators { get; }

        public ServiceActivatorProvider()
        {
            Activators = new ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>>();
        }

        public Func<IServiceResolver, IServiceLifeScope, object> Get(ServiceDependChain chain)
        {
            return Activators.GetOrAdd(chain.Context.ServiceType, (k) => GetActivator(chain));
        }

        public Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType)
        {
            if (Activators.TryGetValue(serviceType, out var factory))
            {
                return factory;
            }

            return null;
        }

        private Func<IServiceResolver, IServiceLifeScope, object> GetActivator(ServiceDependChain chain)
        {
            if (chain.ContainsPropertyCircularDependencies())
            {
                return new ServicePropertyCircularInjectionActivatorFactory().CreateActivator(chain);
            }

            return new ServiceInjectionActivatorFactory().CreateActivator(chain);
        }
    }
}

