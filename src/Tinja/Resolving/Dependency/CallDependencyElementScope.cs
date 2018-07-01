using System;
using System.Collections.Generic;

namespace Tinja.Resolving.Dependency
{
    public class CallDependencyElementScope
    {
        protected Stack<Type> DependencyStack { get; }

        public CallDependencyElementScope()
        {
            DependencyStack = new Stack<Type>();
        }

        public bool Contains(Type typeInfo)
        {
            return DependencyStack.Contains(typeInfo);
        }

        public IDisposable Begin(Type typeInfo)
        {
            if (typeInfo == null)
            {
                return DisposableActionWrapper.Empty;
            }

            DependencyStack.Push(typeInfo);

            return new DisposableActionWrapper(() => DependencyStack.Pop());
        }

        protected class DisposableActionWrapper : IDisposable
        {
            public static readonly IDisposable Empty = new DisposableActionWrapper(() => { });

            private readonly Action _dispose;

            public DisposableActionWrapper(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                _dispose?.Invoke();
            }
        }
    }
}
