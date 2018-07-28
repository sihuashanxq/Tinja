using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    public interface IInterceptorMetadataProvider
    {
        IEnumerable<InterceptorMetadata> GetMetadatas(MemberInfo memberInfo);
    }
}
