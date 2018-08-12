namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IMethodInvocationInvokerBuilder
    {
        IMethodInvocationInvoker Build(IMethodInvocation invocation);
    }
}
