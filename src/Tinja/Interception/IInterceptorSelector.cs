using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorSelector
    {
        IInterceptor[] Select(object target, MethodInfo method, IInterceptor[] interceptors);
    }
}
