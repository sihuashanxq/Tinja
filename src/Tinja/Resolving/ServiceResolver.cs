using System;
using Tinja.Resolving.Activation;
using Tinja.Resolving.Chain;
using Tinja.Resolving.Service;
using Tinja.Resolving.Context;

namespace Tinja.Resolving
{
    public class ServiceResolver : IServiceResolver
    {
        private IContainer _container;

        private ILifeStyleScope _lifeScope;

        private IServiceInfoFactory _typeDescriptorProvider;

        private IResolvingContextBuilder _resolvingContextBuilder;

        private IServiceActivationBuilder _instanceFacotryBuilder;

        public ServiceResolver(
            IContainer container,
            ILifeStyleScope lifeScope,
            IServiceInfoFactory typeDescriptorProvider,
            IResolvingContextBuilder resolvingContextBuilder)
        {
            _container = container;
            _lifeScope = lifeScope;
            _typeDescriptorProvider = typeDescriptorProvider;
            _resolvingContextBuilder = resolvingContextBuilder;
            _instanceFacotryBuilder = new ServiceActivationBuilder();
        }

        public object Resolve(Type resolvingType)
        {
            var factory = _instanceFacotryBuilder.Build(resolvingType);
            if (factory != null)
            {
                return factory(_container, _lifeScope);
            }

            var context = _resolvingContextBuilder.BuildResolvingContext(resolvingType);
            if (context == null)
            {
                return null;
            }

            return _lifeScope.ApplyLifeScope(context, ctx =>
            {
                var iFactory = GetInstanceFactory(ctx);
                if (iFactory == null)
                {
                    return null;
                }

                return iFactory(_container, _lifeScope);
            });
        }

        protected Func<IContainer, ILifeStyleScope, object> GetInstanceFactory(IResolvingContext resolvingContext)
        {
            var component = resolvingContext.Component;
            if (component.ImplementionFactory != null)
            {
                return (o, scoped) =>
                {
                    return resolvingContext.Component.ImplementionFactory(_container);
                };
            }

            var node = new ServiceConstructorChainFactory(new ServiceChainScope(), _typeDescriptorProvider, _resolvingContextBuilder).BuildChain(resolvingContext);
            if (node == null)
            {
                return null;
            }

            return _instanceFacotryBuilder.Build(node);
        }
    }
}
