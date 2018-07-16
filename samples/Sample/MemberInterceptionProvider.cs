using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Interception;

namespace ConsoleApp
{
    public class MemberInterceptionProvider : IInterceptorDescriptorProvider
    {
        public IEnumerable<InterceptorDescriptor> GetInterceptors(MemberInfo memberInfo)
        {
            if (memberInfo.DeclaringType != null &&
                memberInfo.DeclaringType.Name.StartsWith("UserService"))
            {
                yield return new InterceptorDescriptor(10, typeof(UserServiceInterceptor),memberInfo);
            }
        }
    }
}
