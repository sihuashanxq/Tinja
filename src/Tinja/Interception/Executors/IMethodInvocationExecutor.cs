namespace Tinja.Interception
{
    public interface IMethodInvocationExecutor
    {
        object Execute(MethodInvocation invocation);
    }
}
