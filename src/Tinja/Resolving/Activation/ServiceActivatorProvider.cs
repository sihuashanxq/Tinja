using System;
using System.Collections.Concurrent;
using Tinja.Resolving.Context;
using Tinja.Resolving.Dependency;
using Tinja.Resolving.Dependency.Builder;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivatorProvider : IServiceActivatorProvider
    {
        static Func<IServiceResolver, IServiceLifeScope, object> EmptyFactory = (resolver, scope) => null;

        private IResolvingContextBuilder _builder;

        private ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>> _activators;

        public ServiceActivatorProvider(IResolvingContextBuilder builder)
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
                    {
                        return scope.ApplyServiceLifeStyle(
                            context,
                            scopeResolver => context.Component.ImplementionFactory(scopeResolver)
                        );
                    };
                }

                var chain = CreateDependencyBuilder().BuildDependChain(context);
                if (chain == null)
                {
                    return EmptyFactory;
                }

                return Get(chain);
            });
        }

        protected virtual Func<IServiceResolver, IServiceLifeScope, object> Get(ServiceDependChain chain)
        {
            if (chain.ContainsPropertyCircularDependencies())
            {
                return new ServicePropertyCircularInjectionActivatorFactory().CreateActivator(chain);
            }

            return new ServiceInjectionActivatorFactory().CreateActivator(chain);
        }

        protected virtual ServiceDependencyBuilder CreateDependencyBuilder()
        {
            return new ServiceDependencyBuilder(_builder);
        }
    }
}

