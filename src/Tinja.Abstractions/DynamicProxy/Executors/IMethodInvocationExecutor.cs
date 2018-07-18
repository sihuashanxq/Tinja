using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executors
{
    public interface IMethodInvocationExecutor
    {
        TResult Execute<TResult>(IMethodInvocation methodInvocation);

        Task<TResult> ExecuteAsync<TResult>(IMethodInvocation methodInvocation);

        ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocation methodInvocation);
    }
}
