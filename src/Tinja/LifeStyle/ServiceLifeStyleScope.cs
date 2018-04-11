using System;
using System.Collections.Generic;
using Tinja.Resolving;
using Tinja.Resolving.Context;

namespace Tinja.LifeStyle
{
    public class ServiceLifeStyleScope : IServiceLifeStyleScope
    {
        private bool _disposed;

        private IServiceResolver _resolver;

        private IServiceLifeStyleScope _root;

        private List<object> _transientDisposeObjects;

        private Dictionary<Type, object> _scopedObjects;

        internal ServiceLifeStyleScope(IServiceResolver resolver, IServiceLifeStyleScope root) : this(resolver)
        {
            if (!(root is ServiceLifeStyleScope))
            {
                _root = root;
            }
            else
            {
                _root = (root as ServiceLifeStyleScope)._root ?? root;
            }
        }

        public ServiceLifeStyleScope(IServiceResolver resolver)
        {
            _resolver = resolver;
            _transientDisposeObjects = new List<object>();
            _scopedObjects = new Dictionary<Type, object>();
        }

        public object ApplyInstanceLifeStyle(IResolvingContext context, Func<IServiceResolver, object> factory)
        {
            if (_disposed)
            {
                throw new NotSupportedException("scope has disposed!");
            }

            switch (context.Component.LifeStyle)
            {
                case ServiceLifeStyle.Transient:
                    return ApplyTransientInstance(context, factory);
                case ServiceLifeStyle.Scoped:
                    return ApplyScopedInstance(context, factory);
                default:
                    return ApplySingletonInstance(context, factory);
            }
        }

        protected virtual object ApplyScopedInstance(IResolvingContext context, Func<IServiceResolver, object> factory)
        {
            if (!_scopedObjects.ContainsKey(context.ServiceType))
            {
                lock (_scopedObjects)
                {
                    if (!_scopedObjects.ContainsKey(context.ServiceType))
                    {
                        var instance = factory(_resolver);

                        if (!_scopedObjects.ContainsKey(context.ServiceType))
                        {
                            return _scopedObjects[context.ServiceType] = instance;
                        }
                    }
                }
            }

            return _scopedObjects[context.ServiceType];
        }

        protected virtual object ApplyTransientInstance(IResolvingContext context, Func<IServiceResolver, object> factory)
        {
            var instance = factory(_resolver);
            if (instance is IDisposable)
            {
                _transientDisposeObjects.Add(instance);
            }

            return instance;
        }

        protected virtual object ApplySingletonInstance(IResolvingContext context, Func<IServiceResolver, object> factory)
        {
            if (_root == null)
            {
                return ApplyScopedInstance(context, factory);
            }

            return _root.ApplyInstanceLifeStyle(context, factory);
        }

        ~ServiceLifeStyleScope()
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
