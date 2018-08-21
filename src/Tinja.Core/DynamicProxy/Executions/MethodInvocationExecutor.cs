using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public static class MethodInvocationExecutor
    {
        public static void Execute(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            invoker.InvokeAsync(invocation).Wait();
        }

        public static TResult Execute<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            invoker.InvokeAsync(invocation).Wait();

            return (TResult)invocation.ResultValue;
        }

        public static Task ExecuteAsync(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            return invoker.InvokeAsync(invocation);
        }

        public static async Task<TResult> ExecuteAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            await invoker.InvokeAsync(invocation);

            return await (Task<TResult>)invocation.ResultValue;
        }

        public static async ValueTask ExecuteValueTaskAsync(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            //Task,!ValueTask
            await invoker.InvokeAsync(invocation);
        }

        public static async ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            await invoker.InvokeAsync(invocation);

            return await (Task<TResult>)invocation.ResultValue;
        }
    }
}
