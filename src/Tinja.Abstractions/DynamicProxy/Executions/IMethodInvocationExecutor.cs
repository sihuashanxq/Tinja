using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IMethodInvocationExecutor
    {
        TResult Execute<TResult>(IMethodInvocation methodInvocation);

        Task<TResult> ExecuteAsync<TResult>(IMethodInvocation methodInvocation);

        ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocation methodInvocation);
    }
}
