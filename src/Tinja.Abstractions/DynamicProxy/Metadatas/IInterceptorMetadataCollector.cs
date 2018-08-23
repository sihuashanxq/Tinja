using System.Collections.Generic;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    public interface IInterceptorMetadataCollector
    {
        IEnumerable<InterceptorMetadata> Collect(MemberMetadata metadata);
    }
}
