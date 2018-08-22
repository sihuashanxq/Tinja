using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorEntryProvider
    {
        IEnumerable<IInterceptor> GetInterceptors(MemberInfo memberInfo);
    }
}
