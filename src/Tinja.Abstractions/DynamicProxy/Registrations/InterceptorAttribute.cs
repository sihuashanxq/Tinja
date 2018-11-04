using System;
using Tinja.Abstractions.Extensions;

namespace Tinja.Abstractions.DynamicProxy.Registrations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class InterceptorAttribute : Attribute
    {
        public long? Order { get; set; }

        public bool Inherited { get; set; }

        public Type InterceptorType { get; }

        public InterceptorAttribute(Type interceptorType)
        {
            if (interceptorType.IsType<IInterceptor>())
            {
                InterceptorType = interceptorType;
            }
            else
            {
                throw new NotSupportedException($"Type:{interceptorType.FullName} can not cast to the interface:{typeof(IInterceptor).FullName}");
            }
        }
    }
}
