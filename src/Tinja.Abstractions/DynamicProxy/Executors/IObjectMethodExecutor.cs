using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executors
{
    public interface IObjectMethodExecutor
    {
        Task<object> ExecuteAsync(object instance, object[] paramterValues);
    }
}
