using System.Reflection;

namespace Tinja.Interception
{
    public interface IIntereceptorSelector
    {
        IIntereceptor[] Select(object target, MethodInfo targetMethod, IIntereceptor[] intereceptors);
    }
}
