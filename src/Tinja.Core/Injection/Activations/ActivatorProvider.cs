using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependencies;

namespace Tinja.Core.Injection.Activations
{
    internal class ActivatorProvider
    {
        private readonly ActivatorBuilder _builder;

        private readonly ICallDependElementBuilderFactory _factory;

        private readonly Dictionary<Type, Func<IServiceResolver, ServiceLifeScope, object>> _caches;

        private static readonly Func<IServiceResolver, IServiceLifeScope, object> Default = (resolver, scope) => null;

        internal ActivatorProvider(ServiceLifeScope scope, ICallDependElementBuilderFactory factory)
        {
            _factory = factory;
            _builder = new ActivatorBuilder(scope.Root);
            _caches = new Dictionary<Type, Func<IServiceResolver, ServiceLifeScope, object>>();
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(Type serviceType)
        {
            if (_caches.TryGetValue(serviceType, out var item))
            {
                return item;
            }

            var element = _factory.CreateBuilder()?.Build(serviceType);
            if (element == null)
            {
                return _caches[serviceType] = Default;
            }

            return _caches[serviceType] = _builder.Build(element) ?? Default;
        }
    }
}

