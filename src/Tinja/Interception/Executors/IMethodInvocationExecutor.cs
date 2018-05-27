using Tinja.Interception.Executors;

namespace Tinja.Interception
{
    public interface IMethodInvocationExecutor
    {
        object Execute(IMethodInvocation invocation);
    }
}
