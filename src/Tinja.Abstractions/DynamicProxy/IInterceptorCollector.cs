using System.Collections.Generic;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorCollector
    {
        IEnumerable<InterceptorDefinition> Collect(MemberMetadata metadata);
    }
}
