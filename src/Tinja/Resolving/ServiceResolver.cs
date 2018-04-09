using System;
using Tinja.LifeStyle;
using Tinja.Resolving.Activation;
using Tinja.Resolving.Chain;
using Tinja.Resolving.Context;

namespace Tinja.Resolving
{
    public class ServiceResolver : IServiceResolver
    {
        /// <summary>
        /// <see cref="IServiceLifeStyleScope"/>
        /// </summary>
        public IServiceLifeStyleScope Scope { get; }

        /// <summary>
        /// <see cref="Chain.ServiceChainBuilder"/>
        /// </summary>
        internal ServiceChainBuilder ServiceChainBuilder { get; }

        /// <summary>
        /// <see cref="IResolvingContextBuilder"/>
        /// </summary>
        internal IResolvingContextBuilder ResolvingContextBuilder { get; }

        /// <summary>
        /// <see cref="IServiceActivationBuilder"/>
        /// </summary>
        internal IServiceActivationBuilder ServiceActivationBuilder { get; }

        public ServiceResolver(IResolvingContextBuilder builder, IServiceLifeStyleScopeFactory scopeFactory)
        {
            Scope = scopeFactory.Create(this);
            ResolvingContextBuilder = builder;
            ServiceChainBuilder = this.GetService<ServiceChainBuilder>();
            ServiceActivationBuilder = this.GetService<IServiceActivationBuilder>();
        }

        internal ServiceResolver(IServiceResolver root)
        {
            Scope = root.GetService<IServiceLifeStyleScopeFactory>().Create(this, root.Scope);
            ServiceChainBuilder = root.GetService<ServiceChainBuilder>();
            ResolvingContextBuilder = root.GetService<IResolvingContextBuilder>();
            ServiceActivationBuilder = root.GetService<IServiceActivationBuilder>();
        }

        public object GetService(Type serviceType)
        {
            var factory = ServiceActivationBuilder?.Build(serviceType);
            if (factory != null)
            {
                return factory(this, Scope);
            }

            var context = ResolvingContextBuilder.BuildResolvingContext(serviceType);
            if (context == null)
            {
                return null;
            }

            return GetService(context);
        }

        protected virtual object GetService(IResolvingContext context)
        {
            var component = context.Component;
            if (component.ImplementionFactory != null)
            {
                if (component.LifeStyle != ServiceLifeStyle.Transient ||
                    component.ImplementionFactory.Method.ReturnType.Is(typeof(IDisposable)))
                {
                    return Scope.ApplyInstanceLifeStyle(context, resolver => component.ImplementionFactory(resolver));
                }

                return component.ImplementionFactory(this);
            }

            var chain = ServiceChainBuilder.BuildChain(context);
            if (chain == null)
            {
                return null;
            }

            var facotry = ServiceActivationBuilder.Build(chain);
            if (facotry == null)
            {
                return null;
            }

            return facotry(this, Scope);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
