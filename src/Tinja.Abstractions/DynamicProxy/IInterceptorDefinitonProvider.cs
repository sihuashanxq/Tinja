using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorDefinitonProvider
    {
        IEnumerable<InterceptorDefinition> GetDefinitions(MemberInfo memberInfo);
    }
}
