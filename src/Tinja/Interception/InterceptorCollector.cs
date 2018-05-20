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

        public IEnumerable<InterceptionTargetBinding> Collect(Type baseType, Type implementionType)
        {
            var targets = _targetProvider.GetTargets(baseType, implementionType);
            if (targets == null)
            {
                targets = new InterceptionTarget[0];
            }

            foreach (var target in targets)
            {
                var interceptor = _resolver.Resolve(target.InterceptorType) as IInterceptor;
                if (interceptor == null)
                {
                    throw new NotSupportedException($"can not resolve the interceptor with type:{target.InterceptorType.FullName}");
                }

                yield return new InterceptionTargetBinding(interceptor, target);
            }
        }
    }
}
