using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Executors
{
    public interface IObjectMethodExecutorProvider
    {
        IObjectMethodExecutor GetExecutor(MethodInfo methodInfo);
    }
}
