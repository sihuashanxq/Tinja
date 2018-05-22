using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception
{
    public class MemberInterceptorFilter : IMemberInterceptorFilter
    {
        public IInterceptor[] Filter(IEnumerable<InterceptionTargetBinding> interceptions, MemberInfo target)
        {
            var map = new Dictionary<IInterceptor, int>();

            foreach (var item in interceptions)
            {
                var member = item.Target.Members.FirstOrDefault(n => n.MemberInfo == target || n.MemberInfo == target.DeclaringType);
                if (member != null)
                {
                    map[item.Interceptor] = member.Priority;
                    break;
                }
            }

            return map
                .AsEnumerable()
                .OrderByDescending(i => i.Value)
                .Select(i => i.Key).ToArray();
        }
    }
}
