using System;
using Tinja.LifeStyle;
using Tinja.Resolving.Activation;
using Tinja.Resolving.Context;
using Tinja.Resolving.Dependency.Builder;
using Tinja.Resolving.Service;

namespace Tinja.Resolving
{
    public class ServiceResolver : IServiceResolver
    {
        /// <summary>
        /// <see cref="IServiceLifeStyleScope"/>
        /// </summary>
        public IServiceLifeStyleScope Scope { get; }

        /// <summary>
        /// <see cref="Chain.ServiceDependencyBuilder"/>
        /// </summary>
        internal IServiceInfoFactory ServiceInfoFactory { get; }

        /// <summary>
        /// <see cref="IResolvingContextBuilder"/>
        /// </summary>
        internal IResolvingContextBuilder ResolvingContextBuilder { get; }

        /// <summary>
        /// <see cref="IServiceActivatorProvider"/>
        /// </summary>
        internal IServiceActivatorProvider ServiceActivationBuilder { get; }

        static Func<IServiceResolver, IServiceLifeStyleScope, object> DefaultFacotry = (resolver, scope) => null;

        public ServiceResolver(IResolvingContextBuilder builder, IServiceLifeStyleScopeFactory scopeFactory)
        {
            Scope = scopeFactory.Create(this);
            ResolvingContextBuilder = builder;

            ServiceInfoFactory = this.Resolve<IServiceInfoFactory>();
            ServiceActivationBuilder = this.Resolve<IServiceActivatorProvider>();
        }

        internal ServiceResolver(IServiceResolver root)
        {
            Scope = root.Resolve<IServiceLifeStyleScopeFactory>().Create(this, root.Scope);
            ServiceInfoFactory = root.Resolve<IServiceInfoFactory>();
            ResolvingContextBuilder = root.Resolve<IResolvingContextBuilder>();
            ServiceActivationBuilder = root.Resolve<IServiceActivatorProvider>();
        }

        public object Resolve(Type serviceType)
        {
            return GetInstanceFactory(serviceType)(this, Scope);
        }

        protected virtual Func<IServiceResolver, IServiceLifeStyleScope, object> GetInstanceFactory(Type serviceType)
        {
            var factory = ServiceActivationBuilder?.Get(serviceType);
            if (factory != null)
            {
                return factory;
            }

            var context = ResolvingContextBuilder.BuildResolvingContext(serviceType);
            if (context == null)
            {
                return DefaultFacotry;
            }

            var component = context.Component;
            if (component.ImplementionFactory != null)
            {
                if (component.LifeStyle != ServiceLifeStyle.Transient ||
                    component.ImplementionFactory.Method.ReturnType.Is(typeof(IDisposable)))
                {
                    return (resolver, scope) =>
                    {
                        return scope.ApplyServiceLifeStyle(
                            context,
                            scopeResolver => component.ImplementionFactory(scopeResolver)
                        );
                    };
                }

                return (resolver, scope) => component.ImplementionFactory(resolver);
            }

            var chain = new ServiceDependencyBuilder(ResolvingContextBuilder).BuildDependChain(context);
            if (chain == null)
            {
                return DefaultFacotry;
            }

            return ServiceActivationBuilder.Get(chain) ?? DefaultFacotry;
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
