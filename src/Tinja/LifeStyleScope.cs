using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Resolving.ReslovingContext;

namespace Tinja
{
    public class LifeStyleScope : ILifeStyleScope
    {
        private List<object> _transientObjects;

        private ConcurrentDictionary<Type, object> _scopedObjects;

        public LifeStyleScope()
        {
            _transientObjects = new List<object>();
            _scopedObjects = new ConcurrentDictionary<Type, object>();
        }

        public object GetOrAddLifeScopeInstance(IResolvingContext context, Func<IResolvingContext, object> factory)
        {
            if (context.Component.LifeStyle == LifeStyle.Transient)
            {
                var instance = factory(context);
                if (instance is IDisposable)
                {
                    _transientObjects.Add(instance);
                }

                return instance;
            }

            return _scopedObjects.GetOrAdd(context.ReslovingType, (k) => factory(context));
        }

        public object GetOrAddLifeScopeInstance2(Type instanceType, LifeStyle lifeStyle, Func<object> factory)
        {
            if (lifeStyle == LifeStyle.Transient)
            {
                var instance = factory();
                if (instance is IDisposable)
                {
                    _transientObjects.Add(instance);
                }

                return instance;
            }

            return _scopedObjects.GetOrAdd(instanceType, (k) => factory());
        }

        public void Dispose()
        {
            foreach (var item in _transientObjects)
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

            _scopedObjects.Clear();
            _transientObjects.Clear();
        }
    }
}
