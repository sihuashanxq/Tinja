using System.Reflection;

namespace Tinja.Interception
{
    public interface IMemberInterceptorProvider
    {
        InterceptorEntry[] GetInterceptors(MemberInfo memberInfo);
    }
}
