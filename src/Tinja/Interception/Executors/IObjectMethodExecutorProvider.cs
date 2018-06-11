using System.Reflection;

namespace Tinja.Interception.Executors
{
    public interface IObjectMethodExecutorProvider
    {
        IObjectMethodExecutor GetExecutor(MethodInfo methodInfo);
    }
}
