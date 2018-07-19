using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace ConsoleApp
{
    public class MemberInterceptionProvider : IInterceptorDefinitionProvider
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
