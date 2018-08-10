using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorSelector
    {
        bool Supported(MemberInfo memberInfo);

        IInterceptor[] Select(MemberInfo memberInfo, IEnumerable<IInterceptor> interceptors);
    }
}
