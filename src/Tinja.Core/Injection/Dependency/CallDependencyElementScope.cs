using System;
using System.Collections.Generic;

namespace Tinja.Core.Injection.Dependency
{
    public class CallDependencyElementScope
    {
        protected Stack<Type> Stack { get; }

        public CallDependencyElementScope()
        {
            Stack = new Stack<Type>();
        }

        public bool Contains(Type typeInfo)
        {
            return Stack.Contains(typeInfo);
        }

        public IDisposable Begin(Type typeInfo)
        {
            if (typeInfo == null)
            {
                return DisposableActionWrapper.Empty;
            }

            Stack.Push(typeInfo);

            return new DisposableActionWrapper(() => Stack.Pop());
        }

        private class DisposableActionWrapper : IDisposable
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
