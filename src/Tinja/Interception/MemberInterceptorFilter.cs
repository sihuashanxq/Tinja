using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;

namespace Tinja.Interception
{
    public class MemberInterceptorFilter 
    {
        private ConcurrentDictionary<MemberInfo, IInterceptor[]> _memberInterceptors;

        public MemberInterceptorFilter()
        {
            _memberInterceptors = new ConcurrentDictionary<MemberInfo, IInterceptor[]>();
        }

        public IInterceptor[] Filter(IEnumerable<InterceptionTargetBinding> interceptions, MemberInfo target)
        {
            return _memberInterceptors.GetOrAdd(target, _ =>
            {
                var map = new Dictionary<IInterceptor, int>();

                foreach (var item in interceptions)
                {
                    var member = item.Target.Members.FirstOrDefault(n => n.MemberInfo == target || n.MemberInfo == target.DeclaringType);
                    if (member != null)
                    {
                        map[item.Interceptor] = member.Priority;
                    }
                }

                return map
                    .AsEnumerable()
                    .OrderByDescending(i => i.Value)
                    .Select(i => i.Key).ToArray();
            });
        }
    }
}
