using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependencies;

namespace Tinja.Core.Injection.Activations
{
    internal class ActivatorProvider
    {
        private readonly ActivatorBuilder _activatorBuilder;

        private readonly ICallDependElementBuilder _callDependElementBuilder;

        private readonly Dictionary<Type, Func<IServiceResolver, ServiceLifeScope, object>> _caches;

        private static readonly Func<IServiceResolver, IServiceLifeScope, object> Default = (resolver, scope) => null;

        internal ActivatorProvider(ServiceLifeScope scope, ICallDependElementBuilderFactory factory)
        {
            _activatorBuilder = new ActivatorBuilder(scope.Root);
            _callDependElementBuilder = factory.CreateBuilder();

            _caches = new Dictionary<Type, Func<IServiceResolver, ServiceLifeScope, object>>();
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(Type serviceType)
        {
            if (_caches.TryGetValue(serviceType, out var item))
            {
                return item;
            }

            var element = _callDependElementBuilder.Build(serviceType);
            if (element == null)
            {
                return _caches[serviceType] = Default;
            }

            return _caches[serviceType] = _activatorBuilder.Build(element) ?? Default;
        }
    }
}

