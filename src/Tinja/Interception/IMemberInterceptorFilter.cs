using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IMemberInterceptorFilter
    {
        IInterceptor[] Filter(IEnumerable<InterceptionTargetBinding> interceptions, MemberInfo memberInfo);
    }
}
