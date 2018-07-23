using System;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorAccessor
    {
        IInterceptor[] GetOrCreateInterceptors(MemberInfo memberInfo);
    }
}
