using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace ConsoleApp
{
    public class MemberInterceptionCollector : IInterceptorMetadataCollector
    {
        public IEnumerable<InterceptorMetadata> Collect(MemberMetadata metadata)
        {
            return new InterceptorMetadata[0];
        }
    }
}
