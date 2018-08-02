using System;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.DynamicProxy
{
    public class InterceptorFactory : IInterceptorFactory
    {
        private readonly IServiceResolver _serviceResolver;

        public InterceptorFactory(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));
        }

        public IInterceptor Create(Type interceptorType)
        {
            if (interceptorType == null)
            {
                throw new NullReferenceException(nameof(interceptorType));
            }

            return _serviceResolver.ResolveServiceRequired<IInterceptor>(interceptorType);
        }
    }
}
