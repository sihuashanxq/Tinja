using System.Reflection;
using System.Collections.Concurrent;

namespace Tinja.Interception
{
    /// <summary>
    /// </summary>
    public class MemberInterceptorProvider : IMemberInterceptorProvider
    {
        private readonly IInterceptorSelectorProvider _interceptorSelectorProvider;

        private readonly ConcurrentDictionary<MemberInfo, InterceptorEntry[]> _interceptorCaches;

        public MemberInterceptorProvider(IInterceptorSelectorProvider interceptorSelectorProvider)
        {
            _interceptorSelectorProvider = interceptorSelectorProvider;
            _interceptorCaches = new ConcurrentDictionary<MemberInfo, InterceptorEntry[]>();
        }

        public InterceptorEntry[] GetInterceptors(MemberInfo memberInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}
