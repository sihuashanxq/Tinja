using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptorDeclare
    {
        public Type InterceptorType { get; }

        public HashSet<Type> DeclaredTypes { get; }

        public HashSet<MemberInfo> DeclaredMembers { get; }

        public InterceptorDeclare(Type interceptorType)
        {
            InterceptorType = interceptorType;
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
