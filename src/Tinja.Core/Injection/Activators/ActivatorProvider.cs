using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activators;

namespace Tinja.Core.Injection.Activators
{
    public class ActivatorProvider : IActivatorProvider
    {
        private readonly IActivatorFactory _factory;

        private readonly Dictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>> _activators;

        private static readonly Func<IServiceResolver, IServiceLifeScope, object> Default = (resolver, scope) => null;

        public ActivatorProvider(IActivatorFactory factory)
        {
            _factory = factory;
            _activators = new Dictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>>();
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType)
        {
            if (_activators.TryGetValue(serviceType, out var activator))
            {
                return activator;
            }

            //reenterable
            return _activators[serviceType] = _factory.CreateActivator(serviceType) ?? Default;
        }
    }
}

