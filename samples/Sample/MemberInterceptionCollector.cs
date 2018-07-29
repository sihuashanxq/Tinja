using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace ConsoleApp
{
    public class MemberInterceptionCollector : IInterceptorMetadataCollector
    {
        public IEnumerable<InterceptorMetadata> Collect(MemberMetadata metadata)
        {
            if (metadata.Member.DeclaringType != null &&
                metadata.Member.DeclaringType.Name.StartsWith("UserService"))
            {
                yield return new InterceptorMetadata(10, typeof(UserServiceInterceptor), metadata.Member);
            }
        }
    }
}
