using System;
using System.Collections.Generic;
using Tinja.Resolving;
using Tinja.Resolving.Context;

namespace Tinja.LifeStyle
{
    public class ServiceLifeStyleScope : IServiceLifeStyleScope
    {
        private List<object> _transientDisposeObjects;

        private Dictionary<Type, object> _scopedObjects;

        private IServiceResolver _resolver;

        public IServiceLifeStyleScope RootLifeStyleScope { get; }

        internal ServiceLifeStyleScope(IServiceResolver resolver, IServiceLifeStyleScope scope)
            : this(resolver)
        {
            while (scope != null && scope.RootLifeStyleScope != null)
            {
                scope = scope.RootLifeStyleScope;
            }

            RootLifeStyleScope = scope;
        }

        public ServiceLifeStyleScope(IServiceResolver resolver)
        {
            _resolver = resolver;
            _transientDisposeObjects = new List<object>();
            _scopedObjects = new Dictionary<Type, object>();
        }

        public object ApplyInstanceLifeStyle(IResolvingContext context, Func<IServiceResolver, object> factory)
        {
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
            if (!_scopedObjects.ContainsKey(context.ReslovingType))
            {
                lock (_scopedObjects)
                {
                    if (!_scopedObjects.ContainsKey(context.ReslovingType))
                    {
                        return _scopedObjects[context.ReslovingType] = factory(_resolver);
                    }
                }
            }

            return _scopedObjects[context.ReslovingType];
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
            if (RootLifeStyleScope == null)
            {
                return ApplyScopedInstance(context, factory);
            }

            return RootLifeStyleScope.ApplyInstanceLifeStyle(context, factory);
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
