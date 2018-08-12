using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IMethodInvocationExecutor
    {
        TResult Execute<TResult>(IMethodInvocationInvoker invoker,IMethodInvocation invocation);

        Task ExecuteVoidAsync(IMethodInvocationInvoker invoker, IMethodInvocation invocation);

        Task<TResult> ExecuteAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation);

        ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation);
    }
}
