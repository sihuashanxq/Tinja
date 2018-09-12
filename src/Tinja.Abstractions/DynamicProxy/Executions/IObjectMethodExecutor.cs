namespace Tinja.Abstractions.DynamicProxy.Executions
{
    /// <summary>
    /// a interface to invoke method by the given instance and arguments
    /// </summary>
    public interface IObjectMethodExecutor
    {
        /// <summary>
        /// invoke method by the given instance and arguments
        /// </summary>
        /// <typeparam name="TResult">the type of return value</typeparam>
        /// <param name="instance"> an instance of type defined the method</param>
        /// <param name="arguments">call arguments</param>
        /// <returns></returns>
        TResult Execute<TResult>(object instance, object[] arguments);
    }
}
