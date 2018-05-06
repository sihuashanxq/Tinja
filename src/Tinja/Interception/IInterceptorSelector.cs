using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorSelector
    {
        IInterceptor[] Select(object target, MethodInfo targetMethod, IInterceptor[] intereceptors);
    }
}
