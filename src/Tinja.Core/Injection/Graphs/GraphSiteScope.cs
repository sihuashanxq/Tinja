using System;
using System.Collections.Generic;

namespace Tinja.Core.Injection.Graphs
{
    public class GraphSiteScope
    {
        public Stack<Type> Stack { get; set; }

        internal GraphSiteScope()
        {
            Stack = new Stack<Type>();
        }

        internal bool Contains(Type typeInfo)
        {
            return Stack.Contains(typeInfo);
        }

        internal IDisposable CreateScope(Type typeInfo)
        {
            if (typeInfo == null)
            {
                return Disposable.Empty;
            }

            Stack.Push(typeInfo);

            return new Disposable(() => Stack.Pop());
        }

        internal IDisposable CreateNewScope(Type typeInfo)
        {
            var stack = Stack;

            Stack = new Stack<Type>();
            Stack.Push(typeInfo);

            return new Disposable(() => Stack = stack);
        }

        internal GraphSiteScope Clone()
        {
            var scope = new GraphSiteScope();

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
