﻿using System;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Extensions;

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

            return _serviceResolver.ResolveRequired<IInterceptor>(interceptorType);
        }
    }
}
