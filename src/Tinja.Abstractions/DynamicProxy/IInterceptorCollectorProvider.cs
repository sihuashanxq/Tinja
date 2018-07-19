using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorCollectorProvider
    {
        IEnumerable<IInterceptorCollector> GetCollectors(MemberInfo memberInfo);
    }
}
