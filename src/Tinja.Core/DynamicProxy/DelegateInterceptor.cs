using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy
{
    internal class DelegateInterceptor : IInterceptor
    {
        private readonly Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> _handler;

        public DelegateInterceptor(Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            return _handler(invocation, next);
        }
    }
}
