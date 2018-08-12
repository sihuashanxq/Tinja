using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IObjectMethodExecutor
    {
        Task ExecuteAsync(object instance, object[] paramterValues);
    }
}
