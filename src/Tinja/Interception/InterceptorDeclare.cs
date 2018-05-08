using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptorDeclare
    {
        public InterceptorAttribute Interceptor { get; }

        public HashSet<Type> DeclaredTypes { get; }

        public HashSet<MemberInfo> DeclaredMembers { get; }

        public InterceptorDeclare(InterceptorAttribute interceptor)
        {
            Interceptor = interceptor;
            DeclaredTypes = new HashSet<Type>();
            DeclaredMembers = new HashSet<MemberInfo>();
        }

        public bool Supported(MethodInfo methodInfo)
        {
            return
                DeclaredMembers.Any(i => i == methodInfo) ||
                DeclaredTypes.Any(i => i == methodInfo.DeclaringType);
        }
    }
}
