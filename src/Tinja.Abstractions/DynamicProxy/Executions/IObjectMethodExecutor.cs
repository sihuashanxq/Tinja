namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IObjectMethodExecutor
    {
        TResult Execute<TResult>(object instance, object[] parameterValues);
    }
}
