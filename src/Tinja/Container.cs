using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Registration;
using Tinja.Resolving;
using Tinja.Resolving.Service;
using Tinja.Resolving.Context;

namespace Tinja
{
    public class Container : IContainer
    {
        private IServiceResolver _resolver;

        private IServiceRegistrar _registrar;

        private ILifeStyleScope _lifeStyleScope;

        private IResolvingContextBuilder _resolvingContextBuilder;

        private ConcurrentDictionary<Type, List<Component>> _components { get; }

        public Container()
        {
            _components = new ConcurrentDictionary<Type, List<Component>>();
            _registrar = new ServiceRegistrar(_components);
            _lifeStyleScope = new LifeStyleScope();
            _resolvingContextBuilder = new ResolvingContextBuilder(_components);
            _resolver = new ServiceResolver(
                this, 
                _lifeStyleScope, 
                new ServiceInfoFactory(), 
                _resolvingContextBuilder
            );
        }

        public object Resolve(Type serviceType)
        {
            return _resolver.Resolve(serviceType);
        }

        public void Register(Type serviceType, Type implType, LifeStyle lifeStyle)
        {
            _registrar.Register(serviceType, implType, lifeStyle);
        }

        public void Register(Type serviceType, Func<IContainer, object> implFacotry, LifeStyle lifeStyle)
        {
            _registrar.Register(serviceType, implFacotry, lifeStyle);
        }

        public void Dispose()
        {
            _lifeStyleScope.Dispose();
        }
    }
}
