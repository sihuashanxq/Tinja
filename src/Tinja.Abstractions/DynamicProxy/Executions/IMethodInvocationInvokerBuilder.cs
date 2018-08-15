namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IMethodInvocationInvokerBuilder
    {
        IMethodInvocationInvoker BuildInvoker<TResult>(IMethodInvocation invocation);

        IMethodInvocationInvoker BuildTaskAsyncInvoker(IMethodInvocation invocation);

        IMethodInvocationInvoker BuildTaskAsyncInvoker<TResult>(IMethodInvocation invocation);

        IMethodInvocationInvoker BuildValueTaskAsyncInvoker(IMethodInvocation invocation);

        IMethodInvocationInvoker BuildValueTaskAsyncInvoker<TResult>(IMethodInvocation invocation);
    }
}
