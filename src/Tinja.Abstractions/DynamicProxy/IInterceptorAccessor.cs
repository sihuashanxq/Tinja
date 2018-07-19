using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorAccessor
    {
        InterceptorEntry[] GetOrCreateInterceptors(MemberInfo memberInfo);
    }
}
