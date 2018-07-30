using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    /// <summary>
    /// the interface to provide the <see cref="IInterceptorSelector"/> for member
    /// </summary>
    public interface IInterceptorSelectorProvider
    {
        IEnumerable<IInterceptorSelector> GetSelectors(MemberInfo memberInfo);
    }
}
