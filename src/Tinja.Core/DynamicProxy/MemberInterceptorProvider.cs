using System.Collections.Concurrent;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy
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
