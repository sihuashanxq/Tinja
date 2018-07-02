using System;
using System.Collections.Generic;
using Tinja.Resolving;

namespace Tinja.ServiceLife
{
    public class ServiceLifeScope : IServiceLifeScope
    {
        private bool _disposed;

        private readonly IServiceResolver _resolver;

        private readonly IServiceLifeScope _rootScope;

        private readonly List<object> _needCollectedObjects;

        private readonly Dictionary<Type, object> _scopedSingleObjects;

        internal ServiceLifeScope(IServiceResolver resolver, IServiceLifeScope root) : this(resolver)
        {
            if (!(root is ServiceLifeScope))
            {
                _rootScope = root;
            }
            else
            {
                _rootScope = ((ServiceLifeScope)root)._rootScope ?? root;
            }
        }

        public ServiceLifeScope(IServiceResolver resolver)
        {
            _resolver = resolver;
            _needCollectedObjects = new List<object>();
            _scopedSingleObjects = new Dictionary<Type, object>();
        }

        public virtual void AddResolvedService(object instance)
        {
            if (_rootScope != null)
            {
                _rootScope.AddResolvedService(instance);
            }
            else
            {
                _needCollectedObjects.Add(instance);
            }
        }

        protected virtual object GetOrAddScopedInstance(Type serviceType, Func<IServiceResolver, object> factory)
        {
            if (_scopedSingleObjects.TryGetValue(serviceType, out var obj))
            {
                return obj;
            }

            lock (_scopedSingleObjects)
            {
                if (_scopedSingleObjects.TryGetValue(serviceType, out obj))
                {
                    return obj;
                }

                return _scopedSingleObjects[serviceType] = factory(_resolver);
            }
        }

        protected virtual object GetOrAddTransientInstance(Type serviceType, Func<IServiceResolver, object> factory)
        {
            var instance = factory(_resolver);
            if (instance is IDisposable)
            {
                _needCollectedObjects.Add(instance);
            }

            return instance;
        }

        protected virtual object GetOrAddSingletonInstance(Type serviceType, Func<IServiceResolver, object> factory)
        {
            return _rootScope == null
                ? GetOrAddScopedInstance(serviceType, factory)
                : _rootScope.GetOrAddResolvedService(serviceType, ServiceLifeStyle.Singleton, factory);
        }

        public object GetOrAddResolvedService(Type serviceType, ServiceLifeStyle lifeStyle, Func<IServiceResolver, object> factory)
        {
            if (_disposed)
            {
                throw new NotSupportedException("scope has disposed!");
            }

            switch (lifeStyle)
            {
                case ServiceLifeStyle.Transient:
                    return GetOrAddTransientInstance(serviceType, factory);
                case ServiceLifeStyle.Scoped:
                    return GetOrAddScopedInstance(serviceType, factory);
                default:
                    return GetOrAddSingletonInstance(serviceType, factory);
            }
        }

        ~ServiceLifeScope()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            lock (this)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                foreach (var item in _needCollectedObjects)
                {
                    if (item is IDisposable dispose)
                    {
                        dispose.Dispose();
                    }
                }

                foreach (var item in _scopedSingleObjects.Values)
                {
                    if (item is IDisposable dispose)
                    {
                        dispose.Dispose();
                    }
                }

                _needCollectedObjects.Clear();
                _scopedSingleObjects.Clear();
            }
        }
    }
}
