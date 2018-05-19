using System;
using System.Collections.Generic;

using Tinja.Resolving;

namespace Tinja.Interception
{
    public class InterceptorCollector : IInterceptorCollector
    {
        private IServiceResolver _resolver;

        private IInterceptionTargetProvider _targetProvider;

        public InterceptorCollector(IServiceResolver resolver, IInterceptionTargetProvider targetProvider)
        {
            _resolver = resolver;
            _targetProvider = targetProvider;
        }

        public IEnumerable<InterceptionTargetBinding> Collect(Type baseType, Type inheriteType)
        {
            var targets = _targetProvider.GetTargets(baseType, inheriteType);
            var interceptors = new List<InterceptionTargetBinding>();

            foreach (var target in targets)
            {
                var interceptor = _resolver.Resolve(target.InterceptorType) as IInterceptor;
                if (interceptor == null)
                {
                    throw new NotSupportedException($"can not resolve the interceptor with type:{target.InterceptorType.FullName}");
                }

                interceptors.Add(new InterceptionTargetBinding(interceptor, target));
            }

            return interceptors;
        }
    }
}
