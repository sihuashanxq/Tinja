using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceLifeScope : IServiceLifeScope
    {
        private bool _disposed;

        private readonly IServiceResolver _resolver;

        private readonly IServiceLifeScope _rootScope;

        private readonly List<object> _needCollectedObjects;

        private readonly Dictionary<object, object> _scopedSingleObjects;

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
            _scopedSingleObjects = new Dictionary<object, object>();
        }

        protected virtual object GetOrAddScopedInstance(object cacheKey, Func<IServiceResolver, object> factory)
        {
            if (_scopedSingleObjects.TryGetValue(cacheKey, out var obj))
            {
                return obj;
            }

            lock (_scopedSingleObjects)
            {
                if (_scopedSingleObjects.TryGetValue(cacheKey, out obj))
                {
                    return obj;
                }

                return _scopedSingleObjects[cacheKey] = factory(_resolver);
            }
        }

        protected virtual object GetOrAddTransientInstance(object cacheKey, Func<IServiceResolver, object> factory)
        {
            var instance = factory(_resolver);
            if (instance is IDisposable)
            {
                _needCollectedObjects.Add(instance);
            }

            return instance;
        }

        protected virtual object GetOrAddSingletonInstance(object cacheKey, Func<IServiceResolver, object> factory)
        {
            return _rootScope == null
                ? GetOrAddScopedInstance(cacheKey, factory)
                : _rootScope.GetOrAddResolvedService(cacheKey, ServiceLifeStyle.Singleton, factory);
        }

        public object GetOrAddResolvedService(object cacheKey, ServiceLifeStyle lifeStyle, Func<IServiceResolver, object> factory)
        {
            if (_disposed)
            {
                throw new NotSupportedException("scope has disposed!");
            }

            switch (lifeStyle)
            {
                case ServiceLifeStyle.Transient:
                    return GetOrAddTransientInstance(cacheKey, factory);
                case ServiceLifeStyle.Scoped:
                    return GetOrAddScopedInstance(cacheKey, factory);
                default:
                    return GetOrAddSingletonInstance(cacheKey, factory);
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
