using System;
using System.Collections.Concurrent;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivatorProvider : IServiceActivatorProvider
    {
        static Func<IServiceResolver, IServiceLifeScope, object> EmptyFactory = (resolver, scope) => null;

        private IServiceContextBuilder _builder;

        private ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>> _activators;

        public ServiceActivatorProvider(IServiceContextBuilder builder)
        {
            _builder = builder;
            _activators = new ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>>();
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType)
        {
            return _activators.GetOrAdd(serviceType, type =>
            {
                var context = _builder.BuildContext(type);
                if (context == null)
                {
                    return EmptyFactory;
                }

                if (context is ServiceFactoryContext factoryContext)
                {
                    return (resolver, scope) =>
                         scope.ApplyServiceLifeStyle(
                            context,
                            scopeResolver => factoryContext.ImplementionFactory(scopeResolver)
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

        protected virtual Func<IServiceResolver, IServiceLifeScope, object> Get(ServiceCallDependency chain)
        {
            if (chain.ContainsPropertyCircularDependencies())
            {
                return new ServicePropertyCircularActivatorFactory().Create(chain);
            }

            return new ServiceActivatorFactory().Create(chain);
        }

        protected virtual ServiceCallDependency GetDependencyChain(IServiceContext context)
        {
            return new ServiceCallDependencyBuilder(_builder).Build(context);
        }
    }
}

