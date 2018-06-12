using System;
using System.Collections.Generic;
using Tinja.Resolving;
using Tinja.Resolving.Context;

namespace Tinja.ServiceLife
{
    public class ServiceLifeScope : IServiceLifeScope
    {
        private bool _disposed;

        private readonly IServiceResolver _resolver;

        private readonly IServiceLifeScope _root;

        private readonly List<object> _transientDisposeObjects;

        private Dictionary<Type, object> _scopedObjects;

        internal ServiceLifeScope(IServiceResolver resolver, IServiceLifeScope root) : this(resolver)
        {
            if (!(root is ServiceLifeScope))
            {
                _root = root;
            }
            else
            {
                _root = (root as ServiceLifeScope)._root ?? root;
            }
        }

        public ServiceLifeScope(IServiceResolver resolver)
        {
            _resolver = resolver;
            _transientDisposeObjects = new List<object>();
            _scopedObjects = new Dictionary<Type, object>();
        }

        public object ApplyServiceLifeStyle(IServiceContext context, Func<IServiceResolver, object> factory)
        {
            if (_disposed)
            {
                throw new NotSupportedException("scope has disposed!");
            }

            switch (context.LifeStyle)
            {
                case ServiceLifeStyle.Transient:
                    return ApplyTransientInstance(context.ServiceType, factory);
                case ServiceLifeStyle.Scoped:
                    return ApplyScopedInstance(context.ServiceType, factory);
                default:
                    return ApplySingletonInstance(context.ServiceType, factory);
            }
        }

        protected virtual object ApplyScopedInstance(Type serviceType, Func<IServiceResolver, object> factory)
        {
            if (!_scopedObjects.ContainsKey(serviceType))
            {
                lock (_scopedObjects)
                {
                    if (!_scopedObjects.ContainsKey(serviceType))
                    {
                        return _scopedObjects[serviceType] = factory(_resolver);
                    }
                }
            }

            return _scopedObjects[serviceType];
        }

        protected virtual object ApplyTransientInstance(Type serviceType, Func<IServiceResolver, object> factory)
        {
            var instance = factory(_resolver);
            if (instance is IDisposable)
            {
                _transientDisposeObjects.Add(instance);
            }

            return instance;
        }

        protected virtual object ApplySingletonInstance(Type serviceType, Func<IServiceResolver, object> factory)
        {
            if (_root == null)
            {
                return ApplyScopedInstance(serviceType, factory);
            }

            return _root.ApplyServiceLifeStyle(serviceType, ServiceLifeStyle.Singleton, factory);
        }

        public object ApplyServiceLifeStyle(Type serviceType, ServiceLifeStyle lifeStyle, Func<IServiceResolver, object> factory)
        {
            if (_disposed)
            {
                throw new NotSupportedException("scope has disposed!");
            }

            switch (lifeStyle)
            {
                case ServiceLifeStyle.Transient:
                    return ApplyTransientInstance(serviceType, factory);
                case ServiceLifeStyle.Scoped:
                    return ApplyScopedInstance(serviceType, factory);
                default:
                    return ApplySingletonInstance(serviceType, factory);
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
}
