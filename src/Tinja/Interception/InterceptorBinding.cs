using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptorBinding
    {
        public int Priority { get; }

        public IInterceptor Interceptor { get; }

        public HashSet<MemberInfo> Targets { get; }

        public InterceptorBinding(IInterceptor interceptor, HashSet<MemberInfo> targets, int priority)
        {
            Targets = targets;
            Priority = priority;
            Interceptor = interceptor;
        }
    }
}
