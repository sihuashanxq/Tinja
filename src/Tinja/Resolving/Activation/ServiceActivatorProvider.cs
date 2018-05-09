using System;
using System.Collections.Concurrent;
using Tinja.Resolving;
using Tinja.Resolving.Dependency;
using Tinja.Resolving.Dependency.Builder;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivatorProvider : IServiceActivatorProvider
    {
        static Func<IServiceResolver, IServiceLifeScope, object> EmptyFactory = (resolver, scope) => null;

        private IServiceResolvingContextBuilder _builder;

        private ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>> _activators;

        public ServiceActivatorProvider(IServiceResolvingContextBuilder builder)
        {
            _builder = builder;
            _activators = new ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>>();
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType)
        {
            return _activators.GetOrAdd(serviceType, type =>
            {
                var context = _builder.BuildResolvingContext(type);
                if (context == null)
                {
                    return EmptyFactory;
                }

                if (context.Component.ImplementionFactory != null)
                {
                    return (resolver, scope) =>
                         scope.ApplyServiceLifeStyle(
                            context,
                            scopeResolver => context.Component.ImplementionFactory(scopeResolver)
                         );
                }

                var chain = GetDependencyChain(context);
                if (chain == null)
                {
                    return EmptyFactory;
                }

                return Get(chain);
            });
        }

        protected virtual Func<IServiceResolver, IServiceLifeScope, object> Get(ServiceDependencyChain chain)
        {
            if (chain.ContainsPropertyCircularDependencies())
            {
                return new ServicePropertyCircularActivatorFactory().Create(chain);
            }

            return new ServiceActivatorFactory().Create(chain);
        }

        protected virtual ServiceDependencyChain GetDependencyChain(IServiceResolvingContext context)
        {
            return new ServiceConstructorDependencyChainBuilder(_builder).Build(context);
        }
    }
}

