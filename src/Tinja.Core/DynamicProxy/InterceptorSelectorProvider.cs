using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy
{
    [DisableProxy]
    public class InterceptorSelectorProvider : IInterceptorSelectorProvider
    {
        private readonly IEnumerable<IInterceptorSelector> _selectors;

        public InterceptorSelectorProvider(IEnumerable<IInterceptorSelector> selectors)
        {
            _selectors = selectors;
        }

        public IEnumerable<IInterceptorSelector> GetSelectors(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                return new IInterceptorSelector[0];
            }

            return _selectors.Where(item => item.Supported(memberInfo));
        }
    }
}
