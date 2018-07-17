using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Interception;

namespace ConsoleApp
{
    public class MemberInterceptionProvider : IInterceptorDefinitonProvider
    {
        public IEnumerable<InterceptorDefinition> GetDefinitions(MemberInfo memberInfo)
        {
            if (memberInfo.DeclaringType != null &&
                memberInfo.DeclaringType.Name.StartsWith("UserService"))
            {
                yield return new InterceptorDefinition(10, typeof(UserServiceInterceptor),memberInfo);
            }
        }
    }
}
