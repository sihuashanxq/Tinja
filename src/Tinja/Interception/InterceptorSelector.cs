using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception
{
    /// <summary>
    /// cache selectors
    /// </summary>
    public class InterceptorSelector 
    {
        private readonly IEnumerable<IInterceptorSelector> _selectors;

        public InterceptorSelector(IEnumerable<IInterceptorSelector> selectors)
        {
            _selectors = selectors;
        }

        public IInterceptor[] Select(MemberInfo memberInfo, IInterceptor[] interceptors)
        {
            if (memberInfo == null)
            {
                throw new NullReferenceException(nameof(memberInfo));
            }

            if (interceptors == null)
            {
                throw new NullReferenceException(nameof(interceptors));
            }

            if (_selectors == null)
            {
                return interceptors;
            }

            return _selectors.Aggregate(interceptors, (current, selector) => selector.Select(memberInfo, current));
        }
    }
}
