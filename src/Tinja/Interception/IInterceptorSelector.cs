using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorSelector
    {
        IInterceptor[] Select(MethodInfo method, IInterceptor[] interceptors);
    }
}
