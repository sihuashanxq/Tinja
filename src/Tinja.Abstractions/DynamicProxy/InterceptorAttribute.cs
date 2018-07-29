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
                throw new NotSupportedException($"type:{interceptorType.FullName} must implement the interface{typeof(IInterceptor).FullName}");
            }

            InterceptorType = interceptorType;
        }
    }
}
