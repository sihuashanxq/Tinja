using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptionMetadata
    {
        public Type InterceptorType { get; }

        public HashSet<InterceptionMemberMetadata> Targets { get; }

        public InterceptionMetadata(Type interceptorType)
        {
            InterceptorType = interceptorType;
            Targets = new HashSet<InterceptionMemberMetadata>();
        }

        public void AddTarget(MemberInfo memberInfo, int priorty)
        {
            Targets.Add(new InterceptionMemberMetadata() { Member = memberInfo, Priority = priorty });
        }
    }
}
