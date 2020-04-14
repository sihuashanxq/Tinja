using System;
using System.Collections.Concurrent;
using System.Threading;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependencies;
using Tinja.Abstractions.Injection.Dependencies.Elements;

namespace Tinja.Core.Injection.Activations
{
    internal class ActivatorProvider
    {
        private readonly ActivatorBuilder _builder;

        private readonly ICallDependElementBuilderFactory _factory;

        private readonly ConcurrentDictionary<Type, Func<IServiceResolver, ServiceLifeScope, object>> _defaultActivators;

        private readonly ConcurrentDictionary<ServiceTypeTag, Func<IServiceResolver, ServiceLifeScope, object>> _tagsActivators;

        private static readonly Func<IServiceResolver, IServiceLifeScope, object> DefaultProvider = (resolver, scope) => null;

        internal ActivatorProvider(ServiceLifeScope scope, ICallDependElementBuilderFactory factory)
        {
            _factory = factory;
            _builder = new ActivatorBuilder(scope.Root);
            _tagsActivators = new ConcurrentDictionary<ServiceTypeTag, Func<IServiceResolver, ServiceLifeScope, object>>();
            _defaultActivators = new ConcurrentDictionary<Type, Func<IServiceResolver, ServiceLifeScope, object>>();
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(Type serviceType)
        {
            if (_defaultActivators.TryGetValue(serviceType, out var item))
            {
                return item;
            }

            return _defaultActivators[serviceType] = Get(_factory.Create()?.Build(serviceType));
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(Type serviceType, string tag)
        {
            if (tag == null)
            {
                return Get(serviceType);
            }

            var typeTag = new ServiceTypeTag(serviceType, tag);
            if (_tagsActivators.TryGetValue(typeTag, out var item))
            {
                return item;
            }

            return _tagsActivators[typeTag] = Get(_factory.Create()?.Build(serviceType, tag));
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(CallDependElement element)
        {
            if (element == null)
            {
                return DefaultProvider;
            }

            return _builder.Build(element) ?? DefaultProvider;
        }

        private class ServiceTypeTag
        {
            public Type Type;

            public string Tag;

            public ServiceTypeTag(Type type, string tag)
            {
                Tag = tag;
                Type = type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(obj, this))
                {
                    return true;
                }

                if (obj is ServiceTypeTag tt)
                {
                    return tt.Tag == Tag && tt.Type == Type;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return (Type.GetHashCode() * 31) ^ (Tag?.GetHashCode() ?? 0);
            }
        }
    }
}

