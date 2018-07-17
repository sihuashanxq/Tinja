using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorDefinitonProvider
    {
        IEnumerable<InterceptorDefinition> GetDefinitions(MemberInfo memberInfo);
    }
}
