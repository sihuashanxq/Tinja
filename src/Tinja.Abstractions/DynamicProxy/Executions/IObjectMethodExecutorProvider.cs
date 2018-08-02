using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IObjectMethodExecutorProvider
    {
        IObjectMethodExecutor GetExecutor(MethodInfo methodInfo);
    }
}
