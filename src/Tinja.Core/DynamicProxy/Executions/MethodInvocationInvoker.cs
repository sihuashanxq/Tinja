using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    [DisableProxy]
    public class MethodInvocationInvoker : IMethodInvocationInvoker
    {
        private readonly Func<IMethodInvocation, Task> _invoker;

        public MethodInvocationInvoker(Func<IMethodInvocation, Task> invoker)
        {
            _invoker = invoker ?? throw new NullReferenceException(nameof(invoker));
        }

        public Task InvokeAsync(IMethodInvocation invocation)
        {
            return _invoker(invocation);
        }
    }
}
