using System;
using System.Collections.Generic;

namespace Tinja.Core.Injection.Dependencies
{
    public class CallDependElementScope
    {
        public Stack<Type> Stack { get; }

        internal CallDependElementScope()
        {
            Stack = new Stack<Type>();
        }

        internal bool Contains(Type typeInfo)
        {
            return Stack.Contains(typeInfo);
        }

        internal IDisposable Begin(Type typeInfo)
        {
            if (typeInfo == null)
            {
                return Disposable.Empty;
            }

            Stack.Push(typeInfo);

            return new Disposable(() => Stack.Pop());
        }

        internal CallDependElementScope Clone()
        {
            var scope = new CallDependElementScope();

            foreach (var item in Stack.ToArray())
            {
                scope.Stack.Push(item);
            }

            return scope;
        }

        private class Disposable : IDisposable
        {
            public static readonly IDisposable Empty = new Disposable(() => { });

            private readonly Action _dispose;

            public Disposable(Action dispose)
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
