using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IObjectMethodExecutor
    {
        Task<object> ExecuteAsync(object instance, object[] paramterValues);
    }
}
