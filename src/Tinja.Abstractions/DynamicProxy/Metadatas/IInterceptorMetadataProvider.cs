using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    public interface IInterceptorMetadataProvider
    {
        IEnumerable<InterceptorMetadata> GetInterceptors(MemberInfo memberInfo);
    }
}
