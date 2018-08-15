using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodInvocationExecutor
    {
        public void Execute(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            invoker.InvokeAsync(invocation).Wait();
        }

        public Task ExecuteAsync(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            return invoker.InvokeAsync(invocation);
        }

        public async Task<TResult> ExecuteAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            await invoker.InvokeAsync(invocation);

            return await (Task<TResult>)invocation.ResultValue;
        }

        public async ValueTask ExecuteValueTaskAsync(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            //Task,!ValueTask
            await invoker.InvokeAsync(invocation);
        }

        public async ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            await invoker.InvokeAsync(invocation);

            return await (Task<TResult>)invocation.ResultValue;
        }
    }
}
