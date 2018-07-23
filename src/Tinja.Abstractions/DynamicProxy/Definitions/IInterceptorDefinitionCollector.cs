using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Abstractions.DynamicProxy.Definitions
{
    public interface IInterceptorDefinitionCollector
    {
        IEnumerable<InterceptorDefinition> Collect(MemberMetadata metadata);
    }
}
