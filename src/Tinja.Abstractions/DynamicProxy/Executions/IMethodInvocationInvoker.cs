using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IMethodInvocationInvoker
    {
        Task InvokeAsync(IMethodInvocation invocation);
    }
}
