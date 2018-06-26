using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Interception;

namespace ConsoleApp
{
    public class MemberInterceptionProvider : IMemberInterceptionProvider
    {
        public IEnumerable<MemberInterception> GetInterceptions(MemberInfo memberInfo)
        {
            if (memberInfo.DeclaringType != null &&
                memberInfo.DeclaringType.Name.StartsWith("UserService"))
            {
                yield return new MemberInterception()
                {
                    Interceptor = typeof(UserServiceInterceptor),
                    Prioritys = new Dictionary<MemberInfo, long>()
                    {
                        [memberInfo] = long.MaxValue
                    }
                };
            }
        }
    }
}
