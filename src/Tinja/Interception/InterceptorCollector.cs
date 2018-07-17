using System;
using System.Collections.Generic;
using Tinja.Resolving;

namespace Tinja.Interception
{
    public class InterceptorCollector : IInterceptorCollector
    {
        private readonly IServiceResolver _serviceResolver;

        private readonly IInterceptorDefinitionCollector _interceptorCollector;

        internal InterceptorCollector(IServiceResolver serviceResolver, IInterceptorDefinitionCollector interceptorCollector)
        {
            _serviceResolver = serviceResolver;
            _interceptorCollector = interceptorCollector;
        }

        public IEnumerable<InterceptorEntry> Collect(Type serviceType, Type implementionType)
        {
            foreach (var item in _interceptorCollector.CollectDefinitions(serviceType, implementionType))
            {
                var interceptor = (IInterceptor)_serviceResolver.Resolve(item.InterceptorType);
                if (interceptor == null)
                {
                    throw new InvalidOperationException($"can not resolve the interceptor with type:{item.InterceptorType.FullName}");
                }

                yield return new InterceptorEntry(interceptor, item);
            }
        }
    }
}
