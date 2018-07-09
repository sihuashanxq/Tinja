using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;

namespace Tinja.Interception
{
    public class MemberInterceptorFilter
    {
        private readonly ConcurrentDictionary<MemberInfo, IInterceptor[]> _memberInterceptors;

        public MemberInterceptorFilter()
        {
            _memberInterceptors = new ConcurrentDictionary<MemberInfo, IInterceptor[]>();
        }

        public IInterceptor[] Filter(IEnumerable<MemberInterceptionBinding> interceptions, MemberInfo target)
        {
            return _memberInterceptors.GetOrAdd(target, _ =>
            {
                var map = new Dictionary<IInterceptor, long>();

                foreach (var item in interceptions)
                {
                    if (item.MemberInterception.Prioritys.ContainsKey(target))
                    {
                        map[item.Interceptor] = item.MemberInterception.Prioritys.GetValueOrDefault(target);
                        continue;
                    }

                    if (item.MemberInterception.Prioritys.ContainsKey(target.DeclaringType))
                    {
                        map[item.Interceptor] = item.MemberInterception.Prioritys.GetValueOrDefault(target.DeclaringType);
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
