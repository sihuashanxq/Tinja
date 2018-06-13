using System;
using System.Collections.Concurrent;
using Tinja.Resolving.Context;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivatorProvider : IServiceActivatorProvider
    {
        private static readonly Func<IServiceResolver, IServiceLifeScope, object> Default = (resolver, scope) => null;

        private readonly IServiceContextFactory _contextFactory;

        private readonly ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>> _activators;

        public ServiceActivatorProvider(IServiceContextFactory ctxFactory)
        {
            _contextFactory = ctxFactory;
            _activators = new ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeScope, object>>();
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType)
        {
            return _activators.GetOrAdd(serviceType, type =>
            {
                var context = _contextFactory.CreateContext(type);
                if (context == null)
                {
                    return Default;
                }

                if (context.ImplementionFactory != null)
                {
                    return (resolver, scope) =>
                         scope.ApplyServiceLifeStyle(
                            context,
                            scopeResolver => context.ImplementionFactory(scopeResolver)
                         );
                }

                var callDependency = CreateCallDependency(context);
                if (callDependency != null)
                {
                    return GetActivator(callDependency);
                }

                return Default;
            });
        }

        protected virtual Func<IServiceResolver, IServiceLifeScope, object> GetActivator(ServiceCallDependency callDependency)
        {
            return callDependency.ContainsPropertyCircularDependencies() ? new ServicePropertyCircularDependencyActivatorFactory().Create(callDependency) : new ServiceActivatorFactory().Create(callDependency);
        }

        protected virtual ServiceCallDependency CreateCallDependency(ServiceContext context)
        {
            return new ServiceCallDependencyBuilder(_contextFactory).Build(context);
        }
    }
}

