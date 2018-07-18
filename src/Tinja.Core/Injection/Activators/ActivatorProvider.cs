using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activators;

namespace Tinja.Core.Injection.Activators
{
    public class ActivatorProvider : IActivatorProvider
    {
        private readonly IActivatorFactory _factory;

        private readonly Dictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>> _activatorCaches;

        private static readonly Func<IServiceResolver, IServiceLifeScope, object> Default = (resolver, scope) => null;

        public ActivatorProvider(IActivatorFactory factory)
        {
            _factory = factory;
            _activatorCaches = new Dictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>>();
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType)
        {
            if (_activatorCaches.TryGetValue(serviceType, out var activator))
            {
                return activator;
            }

            //reenterable
            return _activatorCaches[serviceType] = _factory.CreateActivator(serviceType) ?? Default;
        }
    }
}

