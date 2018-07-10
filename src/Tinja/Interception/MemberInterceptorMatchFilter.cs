using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;

namespace Tinja.Interception
{
    public class MemberInterceptorMatchFilter
    {
        private readonly ConcurrentDictionary<MemberInfo, IInterceptor[]> _memberInterceptors;

        public MemberInterceptorMatchFilter()
        {
            _memberInterceptors = new ConcurrentDictionary<MemberInfo, IInterceptor[]>();
        }

        public IInterceptor[] Filter(IEnumerable<MemberInterceptionBinding> interceptionBindings, MemberInfo methodInfo)
        {
            return _memberInterceptors.GetOrAdd(methodInfo, _ =>
            {
                if (interceptionBindings == null)
                {
                    return new IInterceptor[0];
                }

                var matchInterceptorOrders = new Dictionary<IInterceptor, long>();

                foreach (var binding in interceptionBindings)
                {
                    if (binding.MemberInterception.MemberOrders.TryGetValue(methodInfo, out var order) ||
                        binding.MemberInterception.MemberOrders.TryGetValue(methodInfo.DeclaringType, out order))
                    {
                        matchInterceptorOrders[binding.Interceptor] = order;
                    }
                }

                return matchInterceptorOrders.OrderByDescending(i => i.Value).Select(i => i.Key).ToArray();
            });
        }
    }
}
