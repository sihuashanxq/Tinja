using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorSelector
    {
        IInterceptor[] Select(MemberInfo memberInfo, IInterceptor[] interceptors);
    }
}
