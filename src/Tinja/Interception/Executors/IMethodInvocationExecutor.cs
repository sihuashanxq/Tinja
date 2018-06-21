using System.Threading.Tasks;

namespace Tinja.Interception.Executors
{
    public interface IMethodInvocationExecutor
    {
        TResult Execute<TResult>(IMethodInvocation invocation);

        Task<TResult> ExecuteAsync<TResult>(IMethodInvocation invocation);

        ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocation invocation);
    }
}
