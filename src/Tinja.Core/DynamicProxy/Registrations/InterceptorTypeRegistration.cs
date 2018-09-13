using System;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.DynamicProxy.Registrations
{
    internal class InterceptorTypeRegistration : InterceptorRegistration
    {
        internal Type InterecptorType { get; }

        internal ServiceLifeStyle LifeStyle { get; }

        internal InterceptorTypeRegistration(Type interceptorType)
            : this(interceptorType, ServiceLifeStyle.Transient)
        {

        }

        public InterceptorTypeRegistration(Type interceptorType, ServiceLifeStyle lifeStyle)
        {
            if (interceptorType.IsNotType(typeof(IInterceptor)))
            {
                throw new NotSupportedException($"Type:{interceptorType.FullName} can not cast to the interface:{typeof(IInterceptor).FullName}");
            }

            LifeStyle = lifeStyle;
            InterecptorType = interceptorType;
        }
    }
}
