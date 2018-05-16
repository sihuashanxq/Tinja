using System.Reflection;

namespace Tinja.Interception
{
    public interface IObjectMethodExecutorProvider
    {
        IObjectMethodExecutor GetExecutor(MethodInfo methodInfo);
    }
}
