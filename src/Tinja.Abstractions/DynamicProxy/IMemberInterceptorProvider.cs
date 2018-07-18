using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IMemberInterceptorProvider
    {
        InterceptorEntry[] GetInterceptors(MemberInfo memberInfo);
    }
}
