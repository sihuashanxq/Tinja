using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    public interface IInterceptorMetadataCollector
    {
        IEnumerable<InterceptorMetadata> Collect(MemberMetadata metadata);
    }
}
