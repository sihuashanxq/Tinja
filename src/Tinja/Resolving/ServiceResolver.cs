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
        /// <see cref="ServiceChainBuilder"/>
        /// </summary>
        internal ServiceChainBuilder ChainBuilder { get; }

        /// <summary>
        /// <see cref="IServiceLifeStyleScope"/>
        /// </summary>
        internal IServiceLifeStyleScope LifeStyleScope { get; }

        /// <summary>
        /// <see cref="IResolvingContextBuilder"/>
        /// </summary>
        internal IResolvingContextBuilder ContextBuilder { get; }

        /// <summary>
        /// <see cref="IServiceActivationBuilder"/>
        /// </summary>
        internal IServiceActivationBuilder ActivationBuilder { get; }

        public ServiceResolver(IResolvingContextBuilder contextBuilder)
        {
            LifeStyleScope = new ServiceLifeStyleScope();
            LifeStyleScope.ApplyLifeScope(typeof(IResolvingContextBuilder), contextBuilder, ServiceLifeStyle.Singleton);

            ContextBuilder = contextBuilder;
            ChainBuilder = this.Resolve<ServiceChainBuilder>();
            ActivationBuilder = this.Resolve<IServiceActivationBuilder>();
        }

        internal ServiceResolver(IServiceLifeStyleScope scope, IResolvingContextBuilder contextBuilder)
        {
            ContextBuilder = contextBuilder;
            LifeStyleScope = new ServiceLifeStyleScope(scope);

            ChainBuilder = this.Resolve<ServiceChainBuilder>();
            ActivationBuilder = this.Resolve<IServiceActivationBuilder>();
        }

        public object Resolve(Type resolvingType)
        {
            var factory = ActivationBuilder?.Build(resolvingType);
            if (factory != null)
            {
                return factory(this, LifeStyleScope);
            }

            var context = ContextBuilder.BuildResolvingContext(resolvingType);
            if (context == null)
            {
                return null;
            }

            return Resolve(context);
        }

        protected virtual object Resolve(IResolvingContext context)
        {
            var component = context.Component;
            if (component.ImplementionFactory != null)
            {
                if (component.LifeStyle != ServiceLifeStyle.Transient ||
                    component.ImplementionFactory.Method.ReturnType.Is(typeof(IDisposable)))
                {
                    return LifeStyleScope.ApplyLifeScope(
                        context,
                        _ => component.ImplementionFactory(this)
                    );
                }

                return component.ImplementionFactory(this);
            }

            var chain = ChainBuilder.BuildChain(context);
            if (chain == null)
            {
                return null;
            }

            var facotry = ActivationBuilder.Build(chain);
            if (facotry == null)
            {
                return null;
            }

            return facotry(this, LifeStyleScope);
        }

        public void Dispose()
        {
            LifeStyleScope.Dispose();
        }
    }
}
