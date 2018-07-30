using System;
using Tinja.Abstractions.Extensions;

namespace Tinja.Abstractions.DynamicProxy
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class InterceptorAttribute : Attribute
    {
        public long Order { get; set; } = -1;

        public bool Inherited { get; set; } = false;

        public Type InterceptorType { get; }

        public InterceptorAttribute(Type interceptorType)
        {
            if (interceptorType.IsNotType<IInterceptor>())
            {
                throw new NotSupportedException($"Type:{interceptorType.FullName} can not cast to the interface:{typeof(IInterceptor).FullName}");
            }

            InterceptorType = interceptorType;
        }
    }
}
