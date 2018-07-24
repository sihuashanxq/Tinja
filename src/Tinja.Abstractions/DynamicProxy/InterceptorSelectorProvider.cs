using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public class InterceptorSelectorProvider : IInterceptorSelectorProvider
    {
        private readonly IEnumerable<IInterceptorSelector> _selectors;

        public InterceptorSelectorProvider(IEnumerable<IInterceptorSelector> selectors)
        {
            _selectors = selectors;
        }

        public IEnumerable<IInterceptorSelector> GetSelectors(MemberInfo memberInfo)
        {
            return _selectors.Where(item => item.Supported(memberInfo));
        }
    }
}
