using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptorMetadata
    {
        public int Priority { get; set; } = -1;

        public bool Inherited { get; set; }

        public Type InterceptorType { get; }

        public HashSet<MemberInfo> Targets { get; }

        public InterceptorMetadata(Type interceptorType, int priority, bool inherited, HashSet<MemberInfo> targets)
        {
            Priority = priority;
            Inherited = inherited;
            InterceptorType = interceptorType;
            Targets = targets ?? new HashSet<MemberInfo>();
        }

        public InterceptorMetadata(InterceptorAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentNullException(nameof(attr));
            }

            Priority = attr.Priority;
            Inherited = attr.Inherited;
            InterceptorType = attr.InterceptorType;
            Targets = new HashSet<MemberInfo>();
        }

        public void AddTarget(MemberInfo memberInfo)
        {
            Targets.Add(memberInfo);
        }
    }
}
