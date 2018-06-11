namespace Tinja.Interception.Executors
{
    public interface IMethodInvocationExecutor
    {
        object Execute(IMethodInvocation invocation);
    }
}
