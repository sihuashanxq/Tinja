using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Builder;
using Tinja.Resolving.Descriptor;
using Tinja.Resolving.ReslovingContext;

namespace Tinja.Resolving
{
    public class ServiceResolver : IServiceResolver
    {
        private IContainer _container;

        private ILifeStyleScope _lifeScope;

        private ITypeDescriptorProvider _typeDescriptorProvider;

        private IResolvingContextBuilder _resolvingContextBuilder;

        private IServiceFactoryBuilder _instanceFacotryBuilder;

        public ServiceResolver(
            IContainer container,
            ILifeStyleScope lifeScope,
            ITypeDescriptorProvider typeDescriptorProvider,
            IResolvingContextBuilder resolvingContextBuilder)
        {
            _container = container;
            _lifeScope = lifeScope;
            _typeDescriptorProvider = typeDescriptorProvider;
            _resolvingContextBuilder = resolvingContextBuilder;
            _instanceFacotryBuilder = new ServiceFactoryBuilder();
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

            return _lifeScope.GetOrAddLifeScopeInstance(context, ctx =>
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

            var node = new ServiceNodeBuilder(_typeDescriptorProvider, _resolvingContextBuilder).Build(resolvingContext);
            if (node == null)
            {
                return null;
            }

            return _instanceFacotryBuilder.Build(node);
        }
    }
}
