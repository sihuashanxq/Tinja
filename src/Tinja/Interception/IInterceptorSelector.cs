using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorSelector
    {
        bool Supported(MemberInfo memberInfo);

        IInterceptor[] Select(MemberInfo memberInfo, IInterceptor[] interceptors);
    }
}
