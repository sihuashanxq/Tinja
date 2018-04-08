using System;
using System.Collections.Generic;
using Tinja.Resolving.Context;

namespace Tinja.LifeStyle
{
    public class ServiceLifeStyleScope : IServiceLifeStyleScope
    {
        private List<object> _transientDisposeObjects;

        private Dictionary<Type, object> _scopedObjects;

        internal IServiceLifeStyleScope RootScope { get; }

        public ServiceLifeStyleScope(IServiceLifeStyleScope lifeStyleScope)
            : this()
        {
            if (lifeStyleScope is ServiceLifeStyleScope scope)
            {
                while (scope != null && scope.RootScope != null)
                {
                    RootScope = scope.RootScope;

                    if (scope.RootScope is ServiceLifeStyleScope)
                    {
                        scope = scope.RootScope as ServiceLifeStyleScope;
                    }
                }
            }
            else
            {
                RootScope = lifeStyleScope;
            }
        }

        public ServiceLifeStyleScope()
        {
            _transientDisposeObjects = new List<object>();
            _scopedObjects = new Dictionary<Type, object>();
        }

        public object ApplyLifeScope(IResolvingContext context, Func<IResolvingContext, object> factory)
        {
            if (context.Component.LifeStyle == ServiceLifeStyle.Transient)
            {
                var o = factory(context);
                if (o is IDisposable)
                {
                    _transientDisposeObjects.Add(o);
                }

                return o;
            }

            if (context.Component.LifeStyle == ServiceLifeStyle.Singleton && RootScope != null)
            {
                return RootScope.ApplyLifeScope(context, factory);
            }

            if (!_scopedObjects.ContainsKey(context.ReslovingType))
            {
                lock (_scopedObjects)
                {
                    if (!_scopedObjects.ContainsKey(context.ReslovingType))
                    {
                        return _scopedObjects[context.ReslovingType] = factory(context);
                    }
                }
            }

            return _scopedObjects[context.ReslovingType];
        }

        public void ApplyLifeScope(Type serviceType, object instance, ServiceLifeStyle lifeStyle)
        {
            if (instance == null)
            {
                return;
            }

            if (lifeStyle == ServiceLifeStyle.Transient &&
                !instance.GetType().Is(typeof(IDisposable)))
            {
                return;
            }

            if (lifeStyle == ServiceLifeStyle.Transient)
            {
                _transientDisposeObjects.Add(instance);
                return;
            }

            if (lifeStyle == ServiceLifeStyle.Singleton && RootScope != null)
            {
                RootScope.ApplyLifeScope(serviceType, instance, lifeStyle);
                return;
            }

            if (!_scopedObjects.ContainsKey(serviceType))
            {
                lock (_scopedObjects)
                {
                    if (!_scopedObjects.ContainsKey(serviceType))
                    {
                        _scopedObjects[serviceType] = instance;
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var item in _transientDisposeObjects)
            {
                if (item is IDisposable dispose)
                {
                    dispose.Dispose();
                }
            }

            foreach (var item in _scopedObjects.Values)
            {
                if (item is IDisposable dispose)
                {
                    dispose.Dispose();
                }
            }

            _transientDisposeObjects.Clear();
            _scopedObjects.Clear();
        }
    }
}
