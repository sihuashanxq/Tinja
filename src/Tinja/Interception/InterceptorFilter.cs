using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;

namespace Tinja.Interception
{
    /// <summary>
    /// </summary>
    public class InterceptorFilter
    {
        private readonly InterceptorSelector _selector;

        private readonly ConcurrentDictionary<MemberInfo, IInterceptor[]> _cache;

        public InterceptorFilter(InterceptorSelector selector)
        {
            _selector = selector;
            _cache = new ConcurrentDictionary<MemberInfo, IInterceptor[]>();
        }

        public IInterceptor[] Filter(IEnumerable<InterceptorEntry> entries, MemberInfo memberInfo)
        {
            return _cache.GetOrAdd(memberInfo, _ =>
            {
                if (entries == null)
                {
                    return new IInterceptor[0];
                }

                var interceptors = entries
                    .Where(item => item.Descriptor.TargetMember == memberInfo || item.Descriptor.TargetMember == memberInfo.DeclaringType)
                    .OrderByDescending(item => item.Descriptor.Order)
                    .Select(item => item.Interceptor)
                    .ToArray();

                if (_selector != null)
                {
                    return _selector.Select(memberInfo, interceptors);
                }

                return interceptors;
            });
        }
    }
}
