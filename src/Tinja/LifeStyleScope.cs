using System;
using System.Collections.Generic;
using Tinja.Resolving.Context;

namespace Tinja
{
    public class LifeStyleScope : ILifeStyleScope
    {
        private List<object> _disposables;

        private Dictionary<Type, object> _scopes;

        public LifeStyleScope()
        {
            _disposables = new List<object>();
            _scopes = new Dictionary<Type, object>();
        }

        public object ApplyLifeScope(IResolvingContext context, Func<IResolvingContext, object> factory)
        {
            if (context.Component.LifeStyle == LifeStyle.Transient)
            {
                var o = factory(context);
                if (o is IDisposable)
                {
                    _disposables.Add(o);
                }

                return o;
            }

            if (!_scopes.ContainsKey(context.ReslovingType))
            {
                lock (_scopes)
                {
                    if (!_scopes.ContainsKey(context.ReslovingType))
                    {
                        return _scopes[context.ReslovingType] = factory(context);
                    }
                }
            }

            return _scopes[context.ReslovingType];
        }

        public void Dispose()
        {
            foreach (var item in _disposables)
            {
                if (item is IDisposable dispose)
                {
                    dispose.Dispose();
                }
            }

            foreach (var item in _scopes.Values)
            {
                if (item is IDisposable dispose)
                {
                    dispose.Dispose();
                }
            }

            _scopes.Clear();
            _disposables.Clear();
        }
    }
}
