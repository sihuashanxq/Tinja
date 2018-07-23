using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Definitions
{
    public interface IInterceptorDefinitionProvider
    {
        IEnumerable<InterceptorDefinition> GetInterceptors(MemberInfo memberInfo);
    }
}
