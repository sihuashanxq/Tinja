using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy
{
    internal class DelegateInterceptor : IInterceptor
    {
        public Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> Handler { get; }

        public DelegateInterceptor(Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> handler)
        {
            Handler = handler ?? throw new NullReferenceException(nameof(handler));
        }

        public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            return Handler(invocation, next);
        }
    }
}
