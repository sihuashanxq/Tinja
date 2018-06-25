using System;
using System.Collections.Concurrent;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public class ActivatorProvider : IActivatorProvider
    {
        private readonly IActivatorFactory _factory;

        private readonly ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>> _activatorCaches;

        private static readonly Func<IServiceResolver, IServiceLifeScope, object> Default = (resolver, scope) => null;

        public ActivatorProvider(IActivatorFactory factory)
        {
            _factory = factory;
            _activatorCaches = new ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>>();
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType)
        {
            return _activatorCaches.GetOrAdd(serviceType, type => _factory.CreateActivator(type) ?? Default);
        }
    }
}

