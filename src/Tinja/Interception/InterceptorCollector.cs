using System;
using System.Collections.Generic;

using Tinja.Resolving;

namespace Tinja.Interception
{
    public class InterceptorCollector : IInterceptorCollector
    {
        private IServiceResolver _resolver;

        private IMemberInterceptionProvider _targetProvider;

        public InterceptorCollector(IServiceResolver resolver, IMemberInterceptionProvider targetProvider)
        {
            _resolver = resolver;
            _targetProvider = targetProvider;
        }

        public IEnumerable<MemberInterceptionBinding> Collect(Type serviceType, Type implementionType)
        {
            var targets = _targetProvider.GetInterceptions(serviceType, implementionType);
            if (targets == null)
            {
                targets = new MemberInterception[0];
            }

            foreach (var target in targets)
            {
                var interceptor = _resolver.Resolve(target.Interceptor) as IInterceptor;
                if (interceptor == null)
                {
                    throw new NotSupportedException($"can not resolve the interceptor with type:{target.Interceptor.FullName}");
                }

                yield return new MemberInterceptionBinding(interceptor, target);
            }
        }
    }
}
