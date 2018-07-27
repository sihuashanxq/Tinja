using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace ConsoleApp
{
    public class MemberInterceptionProvider : IInterceptorMetadataProvider
    {
        public IEnumerable<InterceptorMetadata> GetInterceptorMetadatas(MemberInfo memberInfo)
        {
            if (memberInfo.DeclaringType != null &&
                memberInfo.DeclaringType.Name.StartsWith("UserService"))
            {
                yield return new InterceptorMetadata(10, typeof(UserServiceInterceptor), memberInfo);
            }
        }
    }
}
