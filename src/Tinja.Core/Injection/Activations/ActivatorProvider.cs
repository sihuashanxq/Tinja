using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependencies;
using Tinja.Abstractions.Injection.Dependencies.Elements;

namespace Tinja.Core.Injection.Activations
{
    internal class ActivatorProvider
    {
        private readonly ActivatorBuilder _builder;

        private readonly ICallDependElementBuilderFactory _factory;

        private readonly ConcurrentDictionary<Type, Func<IServiceResolver, ServiceLifeScope, object>> _activators;

        private static readonly Func<IServiceResolver, IServiceLifeScope, object> Default = (resolver, scope) => null;

        internal ActivatorProvider(ServiceLifeScope scope, ICallDependElementBuilderFactory factory)
        {
            _factory = factory;
            _builder = new ActivatorBuilder(scope.Root);
            _activators = new ConcurrentDictionary<Type, Func<IServiceResolver, ServiceLifeScope, object>>();
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(Type serviceType)
        {
            if (_activators.TryGetValue(serviceType, out var item))
            {
                return item;
            }

            return _activators[serviceType] = Get(_factory.CreateBuilder()?.Build(serviceType));
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(CallDependElement element)
        {
            if (element == null)
            {
                return Default;
            }

            return _builder.Build(element) ?? Default;
        }
    }
}

