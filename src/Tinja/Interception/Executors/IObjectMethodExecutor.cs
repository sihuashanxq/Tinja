using System.Threading.Tasks;

namespace Tinja.Interception.Executors
{
    public interface IObjectMethodExecutor
    {
        Task<object> ExecuteAsync(object instance, object[] paramterValues);
    }
}
