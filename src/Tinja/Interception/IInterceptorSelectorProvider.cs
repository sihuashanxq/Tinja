using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorSelectorProvider
    {
        IEnumerable<IInterceptorSelector> GetSelectors(MemberInfo memberInfo);
    }
}
