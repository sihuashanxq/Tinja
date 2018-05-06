using System.Threading.Tasks;

namespace Tinja.Interception
{
    public interface IObjectMethodExecutor
    {
        Task<object> ExecuteAsync(object instance, object[] paramterValues);
    }
}
