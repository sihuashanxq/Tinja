using System;
using System.Collections.Generic;
using Tinja.Resolving;

namespace Tinja.Interception
{
    public class InterceptorCollector : IInterceptorCollector
    {
        private readonly IServiceResolver _resolver;

        private readonly IMemberInterceptionCollector _collector;

        internal InterceptorCollector(IServiceResolver resolver, IMemberInterceptionCollector collector)
        {
            _resolver = resolver;
            _collector = collector;
        }

        public IEnumerable<MemberInterceptionBinding> Collect(Type serviceType, Type implementionType)
        {
            foreach (var item in _collector.Collect(serviceType, implementionType) ?? new MemberInterception[0])
            {
                var interceptor = (IInterceptor)_resolver.Resolve(item.Interceptor);
                if (interceptor != null)
                {
                    yield return new MemberInterceptionBinding(interceptor, item);
                }
                else
                {
                    throw new InvalidOperationException($"can not resolve the interceptor with type:{item.Interceptor.FullName}");
                }
            }
        }
    }
}
