using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptionTarget
    {
        public Type InterceptorType { get; set; }

        public HashSet<InterceptionMemberPriority> Members { get; set; }
    }

    public class InterceptionMemberPriority
    {
        public int Priority { get; set; }

        public MemberInfo MemberInfo { get; set; }
    }
}
