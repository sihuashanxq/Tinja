using System.Collections.Generic;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    public interface IInterceptorMetadataProvider
    {
        IEnumerable<InterceptorMetadata> GetInterceptorMetadatas(MemberInfo memberInfo);
    }
}
