using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorSelectorProvider
    {
        IEnumerable<IInterceptorSelector> GetSelectors(MemberInfo memberInfo);
    }
}
